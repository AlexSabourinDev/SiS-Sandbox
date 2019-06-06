using System;
using System.Reflection;
using System.Collections.Generic;

namespace Game.Networking
{
    public class ReplicationType
    {
        private List<Action<IReplicated, NetStream>> m_Methods = new List<Action<IReplicated, NetStream>>();
        private Dictionary<string, uint> m_MethodLookUp = new Dictionary<string, uint>();

        public Type ObjectType { get; private set; }

        public ReplicationType()
        {
            
        }
        public ReplicationType(Type objectType)
        {
            ObjectType = objectType;
        }

        public void RegisterMethod<T, ArgType>(string methodName) where ArgType : IReplicatedArgument, new()
        {
            MethodInfo method = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                throw new InvalidOperationException("A method with that name does not exist!");
            }
            Action<T, ArgType> callback = (Action<T, ArgType>)method.CreateDelegate(typeof(Action<T, ArgType>));
            Action<IReplicated, NetStream> interopMethod = (IReplicated obj, NetStream ns) =>
            {
                ArgType arg = new ArgType();
                arg.NetSerialize(ns);
                callback((T)obj, arg);
            };
            m_MethodLookUp.Add(methodName, (uint)m_Methods.Count);
            m_Methods.Add(interopMethod);
        }

        public uint GetMethodID(string name)
        {
            uint id = 0xFFFFFFFF;
            m_MethodLookUp.TryGetValue(name, out id);
            return id;
        }

        public Action<IReplicated, NetStream> GetMethod(uint id)
        {
            if(id < m_Methods.Count)
            {
                return m_Methods[(int)id];
            }
            return null;
        }
    }
}
