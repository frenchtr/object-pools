namespace TravisRFrench.ObjectPools.Runtime
{
    public interface IStorage<TEntity>
    {
        int Count { get; }
        
        TEntity Retrieve();
        void Return(TEntity entity);
        void Clear();
    }
}
