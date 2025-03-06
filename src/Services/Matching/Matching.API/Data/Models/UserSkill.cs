using System.ComponentModel.DataAnnotations;

namespace Matching.API.Data.Models
{
    public class UserSkill
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid SportId { get; set; }

        [Required]
        [MaxLength(20)]
        public string SkillLevel { get; set; }  // beginner, intermediate, advanced
    }
}