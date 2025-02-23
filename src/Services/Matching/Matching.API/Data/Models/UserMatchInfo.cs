using System.ComponentModel.DataAnnotations;

namespace Matching.API.Data.Models
{
    public class UserMatchInfo
    {
        [Key]
        public Guid UserId { get; set; }

        public string SelfIntroduction { get; set; }
    }
}