using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Extensions
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public int ExpiryHours { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
