using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.ValueObjects
{
    public sealed record MembershipStatus
    {
        public static readonly MembershipStatus Regular = new("regular");
        public static readonly MembershipStatus Vip = new("vip");

        private MembershipStatus(string value) => Value = value;

        public string Value { get; }
    }
}
