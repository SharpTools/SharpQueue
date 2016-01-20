
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpQueue.Util
{
    public static class Try {
        public static void ThreeTimes(Action func, int delayBetweenTriesInMilli = 0) {
            Again(func, 3, delayBetweenTriesInMilli);
        }

        private static void Again(Action func, int times, int delayBetweenTriesInMilli) {
            for (int i = 0; i < times; i++) {
                try {
                    func.Invoke();
                    return;
                }
                catch {
                    if (times < 0) {
                        throw;
                    }
                    if (delayBetweenTriesInMilli > 0) {
                        Sleep(delayBetweenTriesInMilli);
                    }
                    Again(func, --times, delayBetweenTriesInMilli);
                }
            }
        }

        public static void Sleep(int timeInMilli) {
#if NET40
            Thread.Sleep(timeInMilli);
#else
            Task.Delay(timeInMilli).Wait();
#endif
        }
    }
}
