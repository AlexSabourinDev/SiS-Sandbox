using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Game.Networking
{
    public class PacketProcessor<ResultT>
    {
        private static readonly int NUM_PROTOCOLS = Enum.GetValues(typeof(Protocol)).Length;

        private Thread[] m_Threads = null;
        private ConcurrentQueue<ResultT>[] m_Queues = null;
        private Action<ResultT>[] m_Callbacks = null;
        private volatile int m_Running = 0;

        private bool IsRunning
        {
            get { int val = m_Running; Thread.MemoryBarrier(); return val != 0; }
            set { m_Running = value ? 1 : 0; Thread.MemoryBarrier(); }
        }

        public void Start(Action<ResultT>[] callbacks)
        {
            if (callbacks.Length != NUM_PROTOCOLS)
            {
                throw new ArgumentException("Invalid argument 'callbacks', the size of the array must match the number of protocols!");
            }

            IsRunning = true;

            m_Threads = new Thread[NUM_PROTOCOLS];
            m_Queues = new ConcurrentQueue<ResultT>[NUM_PROTOCOLS];
            m_Callbacks = callbacks;
            for(int i = 0; i <m_Callbacks.Length; ++i)
            {
                if(m_Callbacks[i] != null)
                {
                    m_Threads[i] = new Thread(ProcessProtocolQueue);
                    m_Queues[i] = new ConcurrentQueue<ResultT>();
                }
            }

            for(int i = 0; i < m_Threads.Length; ++i)
            {
                if(m_Threads[i] != null)
                {
                    m_Threads[i].Start(i);
                }
            }
        }

        public void Stop()
        {
            if(!IsRunning)
            {
                return;
            }

            IsRunning = false;

            for(int i = 0; i < m_Threads.Length; ++i)
            {
                if(m_Threads[i] != null)
                {
                    m_Threads[i].Join();
                }
            }

            m_Threads = null;
            // todo: Drain queues?
            m_Queues = null;
        }

        public bool Enqueue(Protocol protocol, ResultT item)
        {
            var queue = m_Queues[(int)protocol];
            if (queue != null)
            {
                queue.Enqueue(item);
                return true;
            }
            return false;
        }

        private void ProcessProtocolQueue(object arg)
        {
            var callback = m_Callbacks[(int)arg];
            var queue = m_Queues[(int)arg];

            while(IsRunning)
            {
                ResultT result;
                if(queue.TryDequeue(out result))
                {
                    callback(result);
                }
            }
        }
    }
}
