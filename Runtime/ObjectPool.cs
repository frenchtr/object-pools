using System;

namespace TravisRFrench.ObjectPools.Runtime
{
    public class ObjectPool<TEntity> : IObjectPool<TEntity>
    {
        protected Func<TEntity> CreateMethod { get; }
        protected Action<TEntity> DestroyMethod { get; }
        protected IStorage<TEntity> Storage { get; }
        protected bool WasSetupCalled { get; private set; }
        
        public int Count => this.Storage.Count;
        public int Capacity { get; }

        public event Action<TEntity> Created;
        public event Action<TEntity> Retrieved;
        public event Action<TEntity> Returned;
        public event Action<TEntity> Destroyed;

        public ObjectPool(Func<TEntity> createMethod, Action<TEntity> destroyMethod, IStorage<TEntity> storage = null, int capacity = 10)
        {
            this.CreateMethod = createMethod;
            this.DestroyMethod = destroyMethod;
            this.Storage = storage ?? new StackBasedStorage<TEntity>(capacity);
            this.Capacity = capacity;
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
            
            var entity = this.Storage.Retrieve();
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
