using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Game.Util;

/// <summary>
/// This script just demonstrates how Logger runs on other threads and synchronizes with Unity update thread to output to unity console.
/// </summary>
public class LoggerTest : MonoBehaviour
{
    private volatile int m_MainThreadID = 0;
    private int MainThreadID
    {
        get { int val = m_MainThreadID; Thread.MemoryBarrier(); return val; }
        set { m_MainThreadID = value; Thread.MemoryBarrier(); }
    }

    // Start is called before the first frame update
    void Start()
    {
        MainThreadID = Thread.CurrentThread.ManagedThreadId;

        Task.Run(async () =>
        {
            Log.Info($"I am a big jam donut: MainThread={MainThreadID}, CurrentThread={Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1500);
            Log.Warning($"I am a big jam donut: MainThread={MainThreadID}, CurrentThread={Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1500);
            Log.Error($"I am a big jam donut: MainThread={MainThreadID}, CurrentThread={Thread.CurrentThread.ManagedThreadId}");
        });
    }
}
