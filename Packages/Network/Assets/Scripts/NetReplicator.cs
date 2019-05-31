using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Util;

namespace Game.Networking
{
    public class NetReplicator : INetReplicator
    {
        public struct ReplicatedObject
        {
            public IReplicated Handle { get; set; }
            public uint ID { get; set; }
            public ReplicationType Type { get; set; }
        }

        protected const uint NULL_OBJECT_ID = 0xFFFFFFFF;
        protected const uint MAX_OBJECT_COUNT = 1000000;
        protected readonly ReplicatedObject NULL_OBJECT = new ReplicatedObject() { Handle = null, ID = NULL_OBJECT_ID, Type = null };

        // Naive implementation, this will not scale in the future but I don't know
        // any problems that exist with <1000 objects for now.
        // todo: Research into alternative data structures that may scale well that provides
        // fast object search by ID + unique ID generation
        protected List<ReplicatedObject> m_Objects = new List<ReplicatedObject>();
        protected List<ReplicationType> m_Types = new List<ReplicationType>();
        protected List<uint> m_FreeIDs = new List<uint>();
        protected uint m_CurrentID = 0;

        // protected uint m_TerribleIDGenerator = 1;
        // 
        // private uint GenerateID()
        // {
        //     return m_TerribleIDGenerator++;
        // }

        public void Init()
        {
            object[] args = new object[1];
            List<Type> replicatedTypes = new List<Type>();
            GetReplicatedTypes(replicatedTypes);
            for(int i = 0; i < replicatedTypes.Count; ++i)
            {
                // Interfaces can't implement static methods.
                if(replicatedTypes[i].IsInterface)
                {
                    continue;
                }
                ReplicationType type = new ReplicationType(replicatedTypes[i]);

                MethodInfo method = replicatedTypes[i].GetMethod("StaticNetInitialize", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if(method != null && method.IsStatic)
                {
                    args[0] = type;
                    method.Invoke(null, args);
                }
                m_Types.Add(type);
            }
        }

        private void GetReplicatedTypes(List<Type> replicatedTypes)
        {
            Type replicatedType = typeof(IReplicated);
            Assembly[] assembly = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assembly.Length; ++i)
            {
                Type[] types = assembly[i].GetTypes();
                for (int k = 0; k < types.Length; ++k)
                {
                    if (types[k] == replicatedType)
                    {
                        continue;
                    }
                    else if (types[k].GetInterface(replicatedType.FullName) == replicatedType)
                    {
                        replicatedTypes.Add(types[k]);
                    }
                }
            }
        }
        
        public void ProcessPacket(RemoteMethodPacket packet)
        {
            for(int i = 0; i < m_Objects.Count; ++i)
            {
                if(m_Objects[i].ID == packet.ObjectID)
                {
                    var type = m_Objects[i].Type;
                    var method = type.GetMethod(packet.MethodID);
                    if(method != null)
                    {
                        NetStream ns = new NetStream();
                        ns.Open(packet.Data);
                        method(m_Objects[i].Handle, ns);
                        ns.Close();
                    }
                }
            }
        }
        
        public void ProcessPacketBytes(byte[] bytes)
        {
            if(bytes == null || bytes.Length < (NetStream.HEADER_SIZE + 1))
            {
                return;
            }

            // Read the header:
            Protocol protocol = (Protocol)bytes[NetStream.HEADER_SIZE];
            switch (protocol)
            {
                case Protocol.RemoteMethod:
                {
                    RemoteMethodPacket packet = new RemoteMethodPacket();
                    if(packet.Read(bytes))
                    {
                        ProcessPacket(packet);
                    }
                    break;
                }
                default:
                    break;
            }

            
        }

        public void Allocate(IReplicated obj)
        {
            if(NetUtil.IsNull(obj))
            {
                throw new ArgumentNullException("Argument 'obj' is null.");
            }

            Type type = obj.GetType();
            ReplicationType replicationType = null;
            for(int i = 0; i < m_Types.Count; ++i)
            {
                if(m_Types[i].ObjectType == type)
                {
                    replicationType = m_Types[i];
                    break;
                }
            }

            // todo: This is probably impossible unless we don't call Init or the assembly gets loaded after Init gets called.
            if(replicationType == null)
            {
                throw new ArgumentException("Invalid argument 'obj' in order for an object to be replicated it must implement the IReplicated interface.");
            }

            uint id = 0;
            if(m_FreeIDs.Count > 0)
            {
                id = m_FreeIDs[m_FreeIDs.Count - 1];
                m_FreeIDs.RemoveAt(m_FreeIDs.Count - 1);
            }
            else
            {
                id = m_CurrentID++;
            }


            while(id >= m_Objects.Count)
            {
                m_Objects.Add(NULL_OBJECT);
            }
            int index = (int)id;

            // todo: This shouldn't really happen, if it does we could possibly recover by reallocating an ID until it's
            // or just crash.
            if (!NetUtil.IsNull(m_Objects[index].Handle))
            {
                Log.Error($"NetReplicator ID generator has generated an ID that is already in use.");
            }

            obj.NetworkID = id;
            obj.NetworkType = replicationType;
            m_Objects[index] = new ReplicatedObject() { ID = id, Handle = obj, Type = replicationType };
        }

        public void Free(IReplicated obj)
        {
            if (NetUtil.IsNull(obj))
            {
                throw new ArgumentNullException("Argument 'obj' is null.");
            }

            uint id = obj.NetworkID;
            obj.NetworkID = 0;

            int index = (int)id;
            if(index < m_Objects.Count)
            {
                if(m_Objects[index].Handle != obj)
                {
                    throw new ArgumentException("Argument 'obj' does not belong to this replicator.");
                }
                m_Objects[index] = NULL_OBJECT;
            }
            else
            {
                throw new InvalidOperationException("Replicated object does not exist within the replicator.");
            }

        }
    }
}
