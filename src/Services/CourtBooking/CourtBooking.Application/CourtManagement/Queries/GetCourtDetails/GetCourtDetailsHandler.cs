//using MediatR;
//using CourtBooking.Application.Data;
//using CourtBooking.Application.DTOs;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;

//public class GetCourtDetailsHandler(IApplicationDbContext _context)
//    : IQueryHandler<GetCourtDetailsQuery, GetCourtDetailsResult>
//{
//    public async Task<GetCourtDetailsResult> Handle(GetCourtDetailsQuery query, CancellationToken cancellationToken)
//    {
//        var court = await _context.Courts
//            //.Include(c => c.OperatingHours)
//            .Join(
//                _context.Sports,
//                court => court.SportId,
//                sport => sport.Id,
//                (court, sport) => new { Court = court, Sport = sport }
//            )
//            .FirstOrDefaultAsync(x => x.Court.Id == CourtId.Of(query.CourtId), cancellationToken);

//        if (court == null)
//        {
//            throw new KeyNotFoundException("Court not found");
//        }

//        //var courtDto = new CourtDTO(
//        //    Id: court.Court.Id.Value,
//        //    CourtName: court.Court.CourtName.Value,
//        //    Description: court.Court.Description,
//        //    Sport: new SportDTO(
//        //        Name: court.Sport.Name,
//        //        Description: court.Sport.Description
//        //    ),
//        //    //OwnerId: court.Court.OwnerId.Value,

//        //);
//        var courtDto = new CourtDTO(
//            //Id: court.Court.Id.Value,
//            CourtName: court.Court.CourtName.Value,
//            Description: court.Court.Description

            
//            //OwnerId: court.Court.O,
//            //Facilities: JsonSerializer.Deserialize<Dictionary<string, string>>(court.Court.Facilities)
//        );

//        return new GetCourtDetailsResult(courtDto);
//    }
//}
