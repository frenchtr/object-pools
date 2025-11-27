using System;
using System.Collections.Generic;

namespace TravisRFrench.ObjectPools.Runtime
{
    public class ObjectPool<TEntity> : IObjectPool<TEntity>
    {
        private readonly Queue<TEntity> active;
        
        protected Func<TEntity> CreateMethod { get; }
        protected Action<TEntity> DestroyMethod { get; }
        protected IStorage<TEntity> Storage { get; }
        protected bool WasSetupCalled { get; private set; }
        protected bool ShouldRecycle { get; }
        
        public int Count => this.Storage.Count;
        public int Capacity { get; }

        public event Action<TEntity> Created;
        public event Action<TEntity> Retrieved;
        public event Action<TEntity> Returned;
        public event Action<TEntity> Destroyed;

        public ObjectPool(Func<TEntity> createMethod, Action<TEntity> destroyMethod, IStorage<TEntity> storage = null, int capacity = 10, bool shouldRecycle = true)
        {
            this.CreateMethod = createMethod;
            this.DestroyMethod = destroyMethod;
            this.Storage = storage ?? new StackBasedStorage<TEntity>(capacity);
            this.Capacity = capacity;
            this.ShouldRecycle = shouldRecycle;
            
            this.active = new Queue<TEntity>();
        }
        
        public virtual void Setup()
        {
            for (var i = 0; i < this.Capacity; i++)
            {
                var entity = this.Create();
                this.Storage.Return(entity);
            }
            
            this.WasSetupCalled = true;
        }

        public virtual void Teardown()
        {
            for (var i = 0; i < this.Capacity; i++)
            {
                var next = this.Storage.Retrieve();
                this.Destroy(next);
            }
            
            this.Storage.Clear();
        }

        public virtual TEntity Retrieve()
        {
            this.SetupIfNotAlreadyCalled();

            TEntity entity;
            
            if (this.Storage.Count == 0)
            {
                if (this.ShouldRecycle)
                {
                    entity = this.active.Dequeue();
                }
                else
                {
                    return default;
                }
            }
            else
            {
                entity = this.Storage.Retrieve();
            }
            
            this.active.Enqueue(entity);
            this.Retrieved?.Invoke(entity);
            
            return entity;
        }

        public virtual void Return(TEntity entity)
        {
            this.Storage.Return(entity);
            this.Returned?.Invoke(entity);
        }

        protected virtual TEntity Create()
        {
            var entity = this.CreateMethod();
            this.Created?.Invoke(entity);
            
            return entity;
        }

        protected virtual void Destroy(TEntity entity)
        {
            this.DestroyMethod(entity);
            this.Destroyed?.Invoke(entity);
        }

        private void SetupIfNotAlreadyCalled()
        {
            if (this.WasSetupCalled)
            {
                return;
            }
            
            this.Setup();
        }
    }
}
