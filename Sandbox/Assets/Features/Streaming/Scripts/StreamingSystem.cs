using System;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Scenes;
using static Unity.Mathematics.math;
using UnityEngine.Profiling;

internal class StreamingCellReference
{
    public StreamingCell m_Cell;
    public uint m_RefCount;
}

public class StreamingSystem : ComponentSystem
{
    // TODO: Passing a config to a system right now is kind of a pain. Let's just have it be a constant value for now.
    private const float c_TileSize = 10.0f;
    private static readonly float3 c_TileReciprocal = new float3(1.0f / c_TileSize);
    private static readonly int3[] c_Neighbours = { new int3(-1, 0, 1), new int3(0, 0, 1), new int3(1, 0, 1),
                                                    new int3(-1, 0, 0), new int3(0, 0, 0), new int3(1, 0, 0),
                                                    new int3(-1, 0, -1), new int3(0, 0, -1), new int3(1, 0, -1) };

    private SplitMap<UInt32, StreamingCellReference> m_LoadedCells = new SplitMap<UInt32, StreamingCellReference>();

    protected override void OnUpdate()
    {
        Profiler.BeginSample("Streaming - Decrease Ref Count");
        // Remove the previous references to the loaded tiles (If they need to stay loaded their reference count will go back up)
        for(int i = 0; i < m_LoadedCells.Keys.Count; i++)
        {
            m_LoadedCells.Values[i].m_RefCount--;
        }
        Profiler.EndSample();

        Profiler.BeginSample("Streaming - Gather Driver Keys");
        List<UInt32> tilesToLoad = new List<UInt32>();
        Entities.WithAll<StreamingDriver>().ForEach(
            (ref Translation pos) =>
            {
                Profiler.BeginSample("Streaming - Adding Tiles");
                int3 tileIndex = new int3(floor((pos.Value) * c_TileReciprocal));
                for(int i = 0; i < c_Neighbours.Length; i++)
                {
                    UInt32 key = ToKey(c_Neighbours[i] + tileIndex);
                    tilesToLoad.Add(key);
                }
                Profiler.EndSample();
            });
        Profiler.EndSample();

        Profiler.BeginSample("Streaming - Load scenes");
        for(int i = 0; i < tilesToLoad.Count; i++)
        {
            UInt32 key = tilesToLoad[i];
            if(!StreamingCellDatabase.s_Instance.m_StreamingCells.HasKey(key))
            {
                continue;
            }

            // TODO: Looping through twice here, once to see if we have the key and once to access it
            // could probably be streamlined.
            if(!m_LoadedCells.HasKey(key))
            {
                Profiler.BeginSample("Streaming - Load Cell");

                StreamingCell cell = StreamingCellDatabase.s_Instance.m_StreamingCells[key];
                m_LoadedCells.Add(key, new StreamingCellReference() { m_Cell = cell, m_RefCount = 0 });

                // Load the layers
                foreach(SubScene subScene in cell.m_Layers)
                {
                    foreach(Entity entity in subScene._SceneEntities)
                    {
                        EntityManager.AddComponentData(entity, new RequestSceneLoaded());
                    }
                }

                Profiler.EndSample();
            }

            m_LoadedCells[key].m_RefCount++;
        }
        Profiler.EndSample();
        
        // Unload and remove cells that
        Profiler.BeginSample("Streaming - Unload Cells");
        for(int i = 0; i < m_LoadedCells.Count; i++)
        {
            if(m_LoadedCells.Values[i].m_RefCount == 0)
            {
                // Unload the layers
                foreach(SubScene subScene in m_LoadedCells.Values[i].m_Cell.m_Layers)
                {
                    foreach(Entity entity in subScene._SceneEntities)
                    {
                        EntityManager.RemoveComponent<RequestSceneLoaded>(entity);
                    }
                }
                m_LoadedCells.RemoveAt(i);
            }
        }
        Profiler.EndSample();
    }

    private UInt32 ToKey(int3 p)
    {
        // Two's complement means that -1 for 32 bits is easily truncated to -1 for 16 bits
        return (UInt32)((p.z << 16) | (p.x & 0xFFFF));
    }
}