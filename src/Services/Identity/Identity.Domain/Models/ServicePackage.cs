﻿namespace Identity.Domain.Models
{
    public class ServicePackage : Aggregate<int>
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int DurationDays { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private List<Guid> _subscribedUserIds = new();
        public IReadOnlyList<Guid> SubscribedUserIds => _subscribedUserIds.AsReadOnly();

        public static ServicePackage Create(
            string name,
            string description,
            decimal price,
            int durationDays)
        {
            if (price <= 0)
                throw new ArgumentException("Price must be positive");

            return new ServicePackage
            {
                Name = name,
                Description = description,
                Price = price,
                DurationDays = durationDays,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void SubscribeUser(Guid userId)
        {
            if (!_subscribedUserIds.Contains(userId))
            {
                _subscribedUserIds.Add(userId);
                AddDomainEvent(new ServicePackageSubscribedEvent(Id, userId));
            }
        }

        public void UpdateDetails(
       string name,
       string description,
       decimal price,
       int durationDays)
        {
            Name = name;
            Description = description;
            Price = price;
            DurationDays = durationDays;

            // Có thể thêm domain rules ở đây
            if (price <= 0)
                throw new ArgumentException("Price must be positive");
        }
    }
}