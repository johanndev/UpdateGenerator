namespace Entities
{
    public abstract class Entity { }
    public abstract class Entity<TEntity> : Entity
    {
        public abstract TEntity Update(TEntity other);
    }
}
