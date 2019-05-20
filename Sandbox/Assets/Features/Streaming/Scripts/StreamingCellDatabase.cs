using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Scenes;
using Unity.Mathematics;

[Serializable]
public class StreamingCell
{
    public List<SubScene> m_Layers;
}

[Serializable]
public class StreamingCellMap : SplitMap<UInt32, StreamingCell> {}

[ExecuteInEditMode]
public class StreamingCellDatabase : MonoBehaviour
{
    [SerializeField]
    public StreamingCellMap m_StreamingCells = new StreamingCellMap();

    public static StreamingCellDatabase s_Instance = null;

    private void Awake()
    {
        System.Diagnostics.Debug.Assert(s_Instance == null);
        s_Instance = this;
    }

    private void OnDestroy()
    {
        s_Instance = null;
    }
}
