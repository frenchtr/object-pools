using System.Collections.Generic;

namespace TravisRFrench.ObjectPools.Runtime
{
    public class StackBasedStorage<TEntity> : IStorage<TEntity>
    {
        private readonly Stack<TEntity> stack;
        
        public int Count => this.stack.Count;

        public StackBasedStorage(int capacity = 10)
        {
            this.stack = new Stack<TEntity>(capacity);
        }
        
        public TEntity Retrieve()
        {
            return this.stack.Pop();
        }

        public void Return(TEntity entity)
        {
            this.stack.Push(entity);
        }

        public void Clear()
        {
            this.stack.Clear();
        }
    }
}
