using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Models
{
    public class ServicePackagePromotion
    {
        public Guid Id { get; set; } // UUID, PRIMARY KEY
        public Guid ServicePackageId { get; set; } // UUID, NOT NULL, khóa ngoại
        public string? Description { get; set; } // TEXT, nullable
        public string DiscountType { get; set; } = null!; // VARCHAR(50), NOT NULL (percentage, fixed_amount)
        public decimal DiscountValue { get; set; } // DECIMAL, NOT NULL
        public DateTime ValidFrom { get; set; } // DATE, NOT NULL
        public DateTime ValidTo { get; set; } // DATE, NOT NULL
        public DateTime CreatedAt { get; set; } // TIMESTAMP, DEFAULT NOW()
        public DateTime UpdatedAt { get; set; } // TIMESTAMP, DEFAULT NOW()

        // Thuộc tính navigation để liên kết với bảng ServicePackage
        public ServicePackage ServicePackage { get; set; } = null!;
    }
}