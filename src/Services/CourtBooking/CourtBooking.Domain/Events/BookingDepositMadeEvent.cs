using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.Events
{
    public record BookingDepositMadeEvent(Guid BookingId, decimal DepositAmount, decimal RemainingBalance) : IDomainEvent;
}