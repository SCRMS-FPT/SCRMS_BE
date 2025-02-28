namespace Identity.Domain.Models;

public class ServicePackage : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int DurationDays { get; private set; }
    public string AssociatedRole { get; private set; } = null!;
    public string Status { get; private set; } = "active";
    public List<ServicePackagePromotion> Promotions { get; set; } = new List<ServicePackagePromotion>();
    public DateTime CreatedAt { get; private set; }

    public static ServicePackage Create(
        string name,
        string description,
        decimal price,
        int durationDays,
        string associatedRole,
        string status = "active") // Thêm tham số status với giá trị mặc định
    {
        if (price <= 0)
            throw new ArgumentException("Price must be positive");

        if (string.IsNullOrWhiteSpace(associatedRole))
            throw new ArgumentException("Associated role is required");

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required");

        return new ServicePackage
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            DurationDays = durationDays,
            AssociatedRole = associatedRole,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string name,
        string description,
        decimal price,
        int durationDays,
        string associatedRole,
        string status) // Thêm status vào phương thức UpdateDetails
    {
        if (price <= 0)
            throw new ArgumentException("Price must be positive");

        if (string.IsNullOrWhiteSpace(associatedRole))
            throw new ArgumentException("Associated role is required");

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required");

        Name = name;
        Description = description;
        Price = price;
        DurationDays = durationDays;
        AssociatedRole = associatedRole;
        Status = status;
    }
}