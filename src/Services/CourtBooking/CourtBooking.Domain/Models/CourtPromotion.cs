using CourtBooking.Domain.ValueObjects;
using System;

namespace CourtBooking.Domain.Models
{
    public class CourtPromotion : Entity<CourtPromotionId>
    {
        public CourtId CourtId { get; private set; }
        public string Description { get; private set; }
        public string DiscountType { get; private set; }
        public decimal DiscountValue { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidTo { get; private set; }

        protected CourtPromotion()
        { } // For EF Core

        public static CourtPromotion Create(CourtId courtId, string description,
            string discountType, decimal discountValue,
            DateTime validFrom, DateTime validTo)
        {
            return new CourtPromotion
            {
                Id = CourtPromotionId.Of(Guid.NewGuid()),
                CourtId = courtId,
                Description = description,
                DiscountType = discountType,
                DiscountValue = discountValue,
                ValidFrom = validFrom,
                ValidTo = validTo,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(string description, string discountType, decimal discountValue, DateTime validFrom, DateTime validTo)
        {
            Description = description;
            DiscountType = discountType;
            DiscountValue = discountValue;
            ValidFrom = validFrom;
            ValidTo = validTo;
            SetLastModified(DateTime.UtcNow);
        }
    }
}