using System;
using System.Collections.Generic;

namespace TravisRFrench.ObjectPools.Runtime
{
    public class ObjectPool<TEntity> : IObjectPool<TEntity>
    {
        private readonly Func<TEntity> createMethod;
        private readonly Action<TEntity> destroyMethod;
        private readonly Stack<TEntity> entities;
        
        public int Count => this.entities.Count;
        public int Capacity { get; }

        public event Action<TEntity> Created;
        public event Action<TEntity> Retrieved;
        public event Action<TEntity> Returned;
        public event Action<TEntity> Destroyed;

        public ObjectPool(Func<TEntity> createMethod, Action<TEntity> destroyMethod, int capacity = 10)
        {
            this.createMethod = createMethod;
            this.destroyMethod = destroyMethod;
            this.Capacity = capacity;
            
            this.entities = new Stack<TEntity>(capacity);
        }
        
        public void Initialize()
        {
            for (var i = 0; i < this.Capacity; i++)
            {
                var entity = this.Create();
                this.entities.Push(entity);
            }
        }

        public TEntity Retrieve()
        {
            var entity = this.entities.Pop();
            this.Retrieved?.Invoke(entity);
            
            return entity;
        }

        public void Return(TEntity entity)
        {
            this.entities.Push(entity);
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
    }
}
