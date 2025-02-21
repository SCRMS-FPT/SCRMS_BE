namespace CourtBooking.Domain.Abstractions
{
    public abstract class Entity<T> : IEntity<T>
    {
        public T Id { get; protected set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public void SetCreatedAt(DateTime createdAt)
        {
            CreatedAt = createdAt;
        }

        public void SetLastModified(DateTime? lastModified)
        {
            LastModified = lastModified;
        }
    }
}
