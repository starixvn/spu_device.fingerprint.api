using System;
using System.Collections.Generic;
using System.Text;

namespace FPS.Client.NetStandard.LogWriter
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class LogQueue.
    /// </summary>
    /// <typeparam name="T">The generic types.</typeparam>
    internal class LogQueue<T> where T : LogMessage
    {
        private Queue<T> _queue;

        public LogQueue()
        {
            _queue = new Queue<T>();
        }

        /// <summary>
        /// Event handler for OnEnqueue.
        /// </summary>
        public event EventHandler OnEnqueue;

        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        public virtual void EventEnqueue(DateTime logTime)
        {
            if (OnEnqueue != null)
            {
                OnEnqueue(logTime, EventArgs.Empty);
            }
        }

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            EventEnqueue(item.LogTime);
        }

        public T Dequeue()
        {
            return _queue.Dequeue();
        }

        public T[] ToArray()
        {
            return _queue.ToArray();
        }

        public void Clear()
        {
            _queue.Clear();
        }
    }
}
