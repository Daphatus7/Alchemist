using System;
using System.Collections.Generic;

namespace _Script.Utilities
{
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private SortedList<TPriority, TElement> _priorityQueue = new SortedList<TPriority, TElement>();
        public int Count => _priorityQueue.Count;
        public bool IsEmpty => _priorityQueue.Count == 0;

        public void Enqueue(TElement element, TPriority priority)
        {
            _priorityQueue.Add(priority, element);
        }

        public TElement Dequeue()
        {
            if (_priorityQueue.Count == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            var element = _priorityQueue.Values[0];
            _priorityQueue.RemoveAt(0);
            return element;
        }


    }
}