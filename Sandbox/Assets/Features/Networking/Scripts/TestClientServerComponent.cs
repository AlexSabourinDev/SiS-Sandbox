using System;
using UnityEngine;
using Game.Util;

namespace Game.Networking
{
    public class TestClientServerComponent : MonoBehaviour
    {
        [Serializable]
        public struct TemporaryClient
        {
            [SerializeField]
            public float StartTime;
            [SerializeField]
            public float LifeTime;

            public NetDriver Client;
            [HideInInspector]
            public bool Created;
            [HideInInspector]
            public bool Closed;
        }

        [SerializeField]
        private int m_ServerPort = 27000;
        private NetDriver m_Server = null;

        [SerializeField]
        private string m_ClientAddress = "127.0.0.1";
        [SerializeField]
        private int m_ClientPort = 27000;
        private NetDriver m_Client = null;
        [SerializeField]
        TemporaryClient[] m_TempClients = new TemporaryClient[0];
        private float m_Time = 0.0f;

        private void Start()
        {
            LogConfig.LoggingLevel = LogLevel.Debug;

            for(int i = 0; i < m_TempClients.Length; ++i)
            {
                m_TempClients[i].Client = null;
                m_TempClients[i].Created = false;
                m_TempClients[i].Closed = false;
            }


            m_Server = new NetDriver();
            m_Server.Host(m_ServerPort);
            if(m_Server.IsRunning)
            {
                Debug.Log($"Hosting server on port {m_ServerPort}");
            }

            m_Client = new NetDriver();
            m_Client.Connect(m_ClientAddress, m_ClientPort);
            if(m_Client.IsRunning)
            {
                Debug.Log($"Connected to server {m_ClientAddress}:{m_ClientPort}");
            }

        }
    
        private void OnDestroy()
        {
            Debug.Log("Shutting down client...");
            m_Client.Close(ShutdownType.Immediate);

            Debug.Log("Shutting down server...");
            m_Server.Close(ShutdownType.Immediate);
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            m_Time += delta;
            for(int i = 0; i < m_TempClients.Length; ++i)
            {
                if(!m_TempClients[i].Created)
                {
                    if(m_Time >= m_TempClients[i].StartTime)
                    {
                        m_TempClients[i].Created = true;
                        m_TempClients[i].Client = new NetDriver();
                        m_TempClients[i].Client.Connect(m_ClientAddress, m_ClientPort);
                    }
                }
                else if(!m_TempClients[i].Closed)
                {
                    if(m_Time >= m_TempClients[i].StartTime + m_TempClients[i].LifeTime)
                    {
                        m_TempClients[i].Closed = true;
                        m_TempClients[i].Client.Close(ShutdownType.Notify);
                    }
                }
            }
        }
    }
}
