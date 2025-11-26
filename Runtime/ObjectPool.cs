using System;

namespace TravisRFrench.ObjectPools.Runtime
{
    public class ObjectPool<TEntity> : IObjectPool<TEntity>
    {
        private readonly Func<TEntity> createMethod;
        private readonly Action<TEntity> destroyMethod;
        private readonly IStorage<TEntity> storage;
        private bool wasSetupCalled;
        
        public int Count => this.storage.Count;
        public int Capacity { get; }

        public event Action<TEntity> Created;
        public event Action<TEntity> Retrieved;
        public event Action<TEntity> Returned;
        public event Action<TEntity> Destroyed;

        public ObjectPool(Func<TEntity> createMethod, Action<TEntity> destroyMethod, IStorage<TEntity> storage = null, int capacity = 10)
        {
            this.createMethod = createMethod;
            this.destroyMethod = destroyMethod;
            this.storage = storage ?? new StackBasedStorage<TEntity>(capacity);
            this.Capacity = capacity;
        }
        
        public void Setup()
        {
            for (var i = 0; i < this.Capacity; i++)
            {
                var entity = this.Create();
                this.storage.Return(entity);
            }
            
            this.wasSetupCalled = true;
        }

        public void Teardown()
        {
            for (var i = 0; i < this.Capacity; i++)
            {
                var next = this.storage.Retrieve();
                this.Destroy(next);
            }
            
            this.storage.Clear();
        }

        public TEntity Retrieve()
        {
            this.SetupIfNotAlreadyCalled();
            
            var entity = this.storage.Retrieve();
            this.Retrieved?.Invoke(entity);
            
            return entity;
        }

        public void Return(TEntity entity)
        {
            this.storage.Return(entity);
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

        private void SetupIfNotAlreadyCalled()
        {
            if (this.wasSetupCalled)
            {
                return;
            }
            
            this.Setup();
        }
    }
}
