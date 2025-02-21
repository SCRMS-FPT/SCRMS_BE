using BuildingBlocks.Exceptions;

namespace Coach.API.Exceptions
{
    public class CoachNotFoundException : NotFoundException
    {
        public CoachNotFoundException(Guid Id) : base("Coach", Id)
        {
        }
    }
}