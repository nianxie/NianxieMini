using System.Threading;

namespace XLua
{
    public class LockFreeQueue<T>
    {
        internal class SingleLinkNode<U> where U : T
        {
            public SingleLinkNode<U> Next;
            public U Item;
        }

        private static bool CAS<TV>(ref TV location, TV comparand, TV newValue) where TV : class
        {
            return comparand == Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        SingleLinkNode<T> head;
        SingleLinkNode<T> tail;
        int count;

        public int Count
        {
            get { return count; }
        }

        public bool IsEmpty
        {
            get { return count <= 0; }
        }

        public LockFreeQueue()
        {
            head = new SingleLinkNode<T>();
            tail = head;
            count = 0;
        }

        public void Enqueue(T item)
        {
            SingleLinkNode<T> oldTail = null;
            SingleLinkNode<T> oldTailNext;

            SingleLinkNode<T> newNode = new SingleLinkNode<T>();
            newNode.Item = item;

            bool newNodeAdded = false;
            while (!newNodeAdded)
            {
                oldTail = tail;
                oldTailNext = oldTail.Next;

                if (tail == oldTail)
                {
                    if (oldTailNext == null)
                        newNodeAdded = CAS<SingleLinkNode<T>>(ref tail.Next, null, newNode);
                    else
                        CAS<SingleLinkNode<T>>(ref tail, oldTail, oldTailNext);
                }
            }

            CAS<SingleLinkNode<T>>(ref tail, oldTail, newNode);
            Interlocked.Increment(ref count);
        }

        public bool TryDequeue(out T item)
        {
            item = default(T);
            SingleLinkNode<T> oldHead = null;

            bool haveAdvancedHead = false;
            while (!haveAdvancedHead)
            {
                oldHead = head;
                SingleLinkNode<T> oldTail = tail;
                SingleLinkNode<T> oldHeadNext = oldHead.Next;

                if (oldHead == head)
                {
                    if (oldHead == oldTail)
                    {
                        if (oldHeadNext == null)
                        {
                            return false;
                        }

                        CAS<SingleLinkNode<T>>(ref tail, oldTail, oldHeadNext);
                    }
                    else
                    {
                        item = oldHeadNext.Item;
                        haveAdvancedHead = CAS<SingleLinkNode<T>>(ref head, oldHead, oldHeadNext);
                    }
                }
            }

            Interlocked.Decrement(ref count);
            return true;
        }

        public T Dequeue()
        {
            T result;
            if (TryDequeue(out result)) return result;
            return default(T);
        }

        public void Clear()
        {
            while (Count > 0)
            {
                Dequeue();
            }
        }
    }
}