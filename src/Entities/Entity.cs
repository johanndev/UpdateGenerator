namespace Entities
{
    public abstract class Entity { }
    public abstract class Entity<TEntity> : Entity
    {
        public abstract void Update(TEntity other);
    }
}
