using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Models
{
    public class CoachServicePurchase
    {
        public Guid Id { get; set; }
        public Guid UserId {  get; set; }   
    }
}
