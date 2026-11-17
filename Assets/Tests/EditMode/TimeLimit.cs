using System;
using System.Threading;

namespace ArObjectLabels.Tests
{
    /// <summary>
    /// Runs a rule on its own thread and fails the test if the rule does not finish in time. The Timeout
    /// attribute only looks at the clock after a test returns, so it cannot stop a loop that never ends.
    /// This can. Only for rules that touch no Unity object.
    /// </summary>
    public static class TimeLimit
    {
        public const int Milliseconds = 2000;

        public static T Run<T>(Func<T> rule)
        {
            T result = default;
            Exception failure = null;
            var worker = new Thread(() =>
            {
                try
                {
                    result = rule();
                }
                catch (ThreadAbortException)
                {
                    // The time limit ended this thread.
                }
                catch (Exception error)
                {
                    failure = error;
                }
            });
            worker.IsBackground = true;
            worker.Start();

            if (!worker.Join(Milliseconds))
            {
                worker.Abort();
                bool stopped = worker.Join(1000);
                throw new TimeoutException("The rule did not finish within " + Milliseconds + " ms."
                    + (stopped ? "" : " Its thread is still running until the next script reload."));
            }
            if (failure != null)
            {
                throw failure;
            }
            return result;
        }
    }
}
