
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CourtBooking.Infrastructure.Data.Extensions
{
    internal class InitialData
    {
        public static IEnumerable<Sport> Sports =>
            new List<Sport>
            {
                Sport.Create(SportId.Of(new Guid("58c49479-ec65-4de2-86e7-033c546291aa")), "Tennis", "Tennis is a racket sport that can be played individually against a single opponent (singles) or between two teams of two players each (doubles)."),
                Sport.Create(SportId.Of(new Guid("189dc8dc-990f-48e0-a37b-e6f2b60b9d7d")), "Badminton", "Badminton is a racquet sport played using racquets to hit a shuttlecock across a net."),
            };

        public static IEnumerable<Court> Courts =>
            new List<Court>
            {
                //Court.Create(CourtId.Of(new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61")), "Court 1", OwnerId.Of(new Guid("58c49479-ec65-4de2-86e7-033c546291aa")), SportId.Of(new Guid("58c49479-ec65-4de2-86e7-033c546291aa")),
                //    new CourtName("Court 1"), new Location("123 Main St", "City", "District", "Commune"), "Court 1 Description",
                //    new List<CourtOperatingHour>
                //    {
                //         CourtOperatingHour
                //        .Create(CourtOperatingHourId.Of(new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914")),
                //             CourtId.Of(new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61")),
                //              new TimeSpan(8, 0, 0), 
                //              new TimeSpan(22, 0, 0),
                //             );
                //    };

                // seed 1 court
                //Court.Create(CourtId.Of(new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61")), new CourtName("Court 1"), SportId.Of(new Guid("58c49479-ec65-4de2-86e7-033c546291aa")),
                //    new Location("123 Main St", "City", "District", "Commune"), "Court 1 Description", "Facilities", 100, OwnerId.Of(new Guid("58c49479-ec65-4de2-86e7-033c546291aa")),
                //    new List<CourtOperatingHour>
                //    {
                //        CourtOperatingHour
                //            .Create(CourtOperatingHourId.Of(new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914")),
                //                CourtId.Of(new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61")),
                //                new TimeSpan(8, 0, 0),
                //                new TimeSpan(22, 0, 0))
                //    }
                //)

            };
    }
}
