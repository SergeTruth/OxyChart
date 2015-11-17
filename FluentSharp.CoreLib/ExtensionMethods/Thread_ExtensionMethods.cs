using System;
using System.Diagnostics;
using System.Threading;

namespace FluentSharp.CoreLib
{
    public static class Thread_ExtensionMethods
    {
        public static string name(this Thread thread)
        {
            if (thread.notNull())
                return thread.Name;
            return null;
        }
        public static StackTrace stackTrace(this Thread thread)
        {
            if (thread.notNull())
                try
                {
                    return new StackTrace(thread, true);
                }
                catch (Exception ex)
                {
                    ex.log("[thread][stacktrace]");
                }                            
            return null;
        }
        public static int       sleep(this int sleepPeriod)
        {
            Thread.Sleep(sleepPeriod);
            return sleepPeriod;
        }
    }
}