using System;
using System.Collections;
using Foundation.Tasks;

namespace Realtime.Messaging.Internal
{
    /// <summary>
    /// X-Platform Timer
    /// </summary>
    public class TaskTimer
    {
        private Action _elapsed = delegate { };
        public event Action Elapsed
        {
            add
            {
                _elapsed = (Action)Delegate.Combine(_elapsed, value);
            }
            remove
            {
                _elapsed = (Action)Delegate.Remove(_elapsed, value);
            }
        }

        /// <summary>
        /// Run again after elapsed
        /// </summary>
        public bool AutoReset;

        /// <summary>
        /// seconds
        /// </summary>
        public int Interval = 10;

        /// <summary>
        /// Is Running
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Run counter
        /// </summary>
        public int Run { get; protected set; }

        /// <summary>
        /// Starts Run
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            Run++;
            TaskManager.StartRoutine(GoAsync(Run));
        }

        /// <summary>
        /// Stops Run
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            Run++;
            TaskManager.StopRoutine(GoAsync(Run));
        }

        void Raise()
        {
            if (_elapsed != null)
                _elapsed();
        }

        IEnumerator GoAsync(int i)
        {
            yield return TaskManager.WaitForSeconds(Interval);

            // is running and same run
            if (i == Run && IsRunning)
            {
                Raise();

                if (AutoReset)
                    Start();
            }
        }


    }
}