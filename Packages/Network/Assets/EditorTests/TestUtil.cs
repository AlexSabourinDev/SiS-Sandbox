using System;
using System.Collections.Generic;
using System.Threading;

namespace Game.Networking
{ 
    public static class TestUtil
    {
        public delegate bool ConditionCallback();

        public static void Wait(ConditionCallback callback, int cycles = 100, int cycleDurationMilliseconds = 16)
        {
            if(cycles < 0 || callback == null)
            {
                return;
            }

            for(int i = 0; i < cycles; ++i)
            {
                if(callback())
                {
                    return;
                }
                Thread.Sleep(cycleDurationMilliseconds);
            }
        }
    }
}
