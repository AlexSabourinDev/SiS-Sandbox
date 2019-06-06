using System;
using System.Collections.Generic;
using Game.Util;

namespace Game.Networking
{
    /// <summary>
    /// As of now this is a very basic 'virtual network' class that will help simulate various 
    /// network topologies and behavior that might occur on a real network.
    /// </summary>
    public class VirtualNetwork
    {
        private Dictionary<string, IVirtualNode> m_Nodes = new Dictionary<string, IVirtualNode>();
        private readonly object m_Lock = new object();



        public void Connect(IVirtualNode node)
        {
            lock(m_Lock)
            {
                if(m_Nodes.ContainsKey(node.VirtualAddress))
                {
                    Log.Error($"Cannot add virtual node with address={node.VirtualAddress}");
                    throw new InvalidOperationException("A node with that address already exists.");
                }
                else
                {
                    m_Nodes.Add(node.VirtualAddress, node);

                    Log.Info($"[Network] {node.VirtualAddress} has connected to the network.");
                }
            }
        }

        public void Disconnect(IVirtualNode node)
        {
            lock(m_Lock)
            {
                if(!m_Nodes.Remove(node.VirtualAddress))
                {
                    Log.Error($"Cannot remove virtual node with address={node.VirtualAddress}");
                    throw new InvalidOperationException("A node with that address does not exist!");
                }
                else
                {
                    Log.Info($"[Network] {node.VirtualAddress} has disconencted from the network.");
                }
            }
        }

        public bool Send(string target, IVirtualNode sender, byte[] data)
        {
            IVirtualNode targetNode = null;
            if(m_Nodes.TryGetValue(target, out targetNode))
            {
                targetNode.OnReceive(sender, data);
                return true;
            }
            return false;
        }
    }
}
