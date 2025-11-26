using System.Collections.Generic;

namespace TravisRFrench.ObjectPools.Runtime
{
    public class QueueBasedStorage<TEntity> : IStorage<TEntity>
    {
        private readonly Queue<TEntity> queue;
        
        public int Count => this.queue.Count;

        public QueueBasedStorage(int capacity = 10)
        {
            this.queue = new Queue<TEntity>(capacity);
        }

        public TEntity Retrieve()
        {
            return this.queue.Dequeue();
        }

        public void Return(TEntity entity)
        {
            this.queue.Enqueue(entity);
        }

        public void Clear()
        {
            this.queue.Clear();
        }
    }
}
