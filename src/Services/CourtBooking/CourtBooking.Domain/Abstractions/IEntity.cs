namespace CourtBooking.Domain.Abstractions;

public interface IEntity<TId> : IEntity
{
    TId Id { get; }
}

public interface IEntity
{
    DateTime CreatedAt { get; }
    DateTime? LastModified { get; }
    void SetCreatedAt(DateTime createdAt);
    void SetLastModified(DateTime? lastModified);
}
