using System;

namespace TravisRFrench.ObjectPools.Runtime
{
    public interface IObjectPool<TEntity>
    {
        int Count { get; }
        int Capacity { get; }

        event Action<TEntity> Created;
        event Action<TEntity> Retrieved;
        event Action<TEntity> Returned;
        event Action<TEntity> Destroyed;
        
        void Setup();
        void Teardown();
        TEntity Retrieve();
        void Return(TEntity entity);
    }
}
