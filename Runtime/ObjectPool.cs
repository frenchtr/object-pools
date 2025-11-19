using System;
using System.Collections.Generic;
using System.Linq;

namespace TravisRFrench.ObjectPools.Runtime
{
    public class ObjectPool<TEntity> : IObjectPool<TEntity>
    {
        private readonly Func<TEntity> createMethod;
        private readonly Action<TEntity> destroyMethod;
        private readonly RecycleMode recycleMode;
        private readonly Stack<TEntity> available;
        private readonly List<TEntity> active;
        
        public int Count => this.available.Count;
        public int Capacity { get; }

        public event Action<TEntity> Created;
        public event Action<TEntity> Retrieved;
        public event Action<TEntity> Returned;
        public event Action<TEntity> Destroyed;

        public ObjectPool(Func<TEntity> createMethod, Action<TEntity> destroyMethod, int capacity = 10, RecycleMode recycleMode = RecycleMode.FirstInFirstOut)
        {
            this.createMethod = createMethod;
            this.destroyMethod = destroyMethod;
            this.recycleMode = recycleMode;
            this.Capacity = capacity;
            
            this.available = new Stack<TEntity>(capacity);
            this.active = new List<TEntity>();
        }
        
        public void Initialize()
        {
            for (var i = 0; i < this.Capacity; i++)
            {
                var entity = this.Create();
                this.available.Push(entity);
            }
        }

        public TEntity Retrieve()
        {
            if (!this.available.Any())
            {
                return this.RecycleOrThrow();
            }
            
            var entity = this.available.Pop();
            this.active.Add(entity);
            this.Retrieved?.Invoke(entity);
            
            return entity;
        }

        public void Return(TEntity entity)
        {
            this.active.Remove(entity);
            this.available.Push(entity);
            this.Returned?.Invoke(entity);
        }

        private TEntity Create()
        {
            var entity = this.createMethod();
            this.Created?.Invoke(entity);
            
            return entity;
        }

        private void Destroy(TEntity entity)
        {
            this.destroyMethod(entity);
            this.Destroyed?.Invoke(entity);
        }

        private TEntity RecycleOrThrow()
        {
            if (this.recycleMode == RecycleMode.None)
            {
                throw new InvalidOperationException("Pool capacity reached and recycling is disabled.");
            }

            if (this.active.Count == 0)
            {
                throw new InvalidOperationException("Pool exhausted but no active items exist to recycle.");
            }

            var index = 0;

            switch (this.recycleMode)
            {
                case RecycleMode.None:
                    break;
                case RecycleMode.FirstInFirstOut:
                    index = 0;
                    break;
                case RecycleMode.FirstInLastOut:
                    index = this.active.Count - 1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var entity = this.active[index];
            this.active.RemoveAt(index);

            return entity;
        }
    }
}
