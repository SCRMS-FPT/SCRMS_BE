using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CourtBooking.Infrastructure.Data.Extensions;

public static class DatabaseExtentions
{
    // Court owner ID from Identity service
    private static readonly Guid CourtOwnerUserId = new Guid("8e445865-a24d-4543-a6c6-9443d048cdb9");

    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var isDatabaseCreated = await context.Database.CanConnectAsync();
        if (!isDatabaseCreated)
        {
            await context.Database.MigrateAsync();
        }
        await SeedAsync(context, logger);
    }

    private static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        // Check if SportCenter exists before proceeding with seeding
        if (!await context.SportCenters.AnyAsync())
        {
            logger.LogInformation("Bắt đầu quá trình seed dữ liệu");

            // Seed in the required order
            var sportCenter = await SeedSportCenterAsync(context, logger);
            var sports = await SeedSportsAsync(context, logger);
            var courts = await SeedCourtsAsync(context, logger, sportCenter, sports);
            await SeedCourtSchedulesAsync(context, logger, courts);
            await SeedCourtPromotionsAsync(context, logger, courts);

            logger.LogInformation("Quá trình seed dữ liệu hoàn tất thành công");
        }
        else
        {
            logger.LogInformation("Dữ liệu đã được seed trước đó, không cần thực hiện lại");
        }
    }

    private static async Task<SportCenter> SeedSportCenterAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Seed dữ liệu trung tâm thể thao...");

        var ownerId = OwnerId.Of(CourtOwnerUserId);
        var sportCenterId = SportCenterId.Of(Guid.NewGuid());
        var location = new Location("123 Nguyễn Huệ", "Thành phố Hồ Chí Minh", "Quận 1", "Phường Bến Nghé");
        var geoLocation = new GeoLocation(10.7769, 106.7009); // Tọa độ tại TPHCM
        var images = new SportCenterImages(
            "https://assets.simpleviewinc.com/simpleview/image/upload/c_limit,q_75,w_1200/v1/crm/chicagonorthwest/CMYK_IMG_1182_D7D4CA25-5056-BF65-D61EF582949EFB12-d7d4c4075056bf6_d7d4d2d0-5056-bf65-d6d24c67bdaa666f.jpg",
            new List<string> {
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRul3f9Irv5tUe7fQTovkT5ZIvPt3ecYnTQsQ&s",
                "https://img.thegioithethao.vn/thumbs/san-cau-long/ha-noi/hoang-mai/san-net-sport-center/net-sport-centernet-sport-center-1_thumb_150.webp",
                "https://pix10.agoda.net/hotelImages/138/1387741/1387741_17020118140050711361.jpg?ca=6&ce=1&s=414x232"
            }
        );

        var sportCenter = SportCenter.Create(
            sportCenterId,
            ownerId,
            "Trung Tâm Thể Thao Victory",
            "0901234567",
            location,
            geoLocation,
            images,
            "Trung tâm thể thao cao cấp với cơ sở vật chất hiện đại và đội ngũ nhân viên chuyên nghiệp. Tọa lạc tại trung tâm Quận 1, thuận tiện di chuyển từ mọi quận huyện."
        );
        sportCenter.SetCreatedAt(DateTime.UtcNow);
        sportCenter.SetLastModified(DateTime.UtcNow);
        await context.SportCenters.AddAsync(sportCenter);
        await context.SaveChangesAsync();

        logger.LogInformation("Đã tạo trung tâm thể thao với ID: {ID}", sportCenter.Id.Value);

        return sportCenter;
    }

    private static async Task<List<Sport>> SeedSportsAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Seed dữ liệu môn thể thao...");

        var sports = new List<Sport>
        {
            Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Tennis",
                "Tennis là môn thể thao dùng vợt đánh bóng, có thể chơi đơn (1 người chơi mỗi bên) hoặc đánh đôi (2 người chơi mỗi bên).",
                "🎾"
            ),
            Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Cầu lông",
                "Cầu lông là môn thể thao dùng vợt đánh quả cầu qua lại trên lưới được căng ngang giữa sân.",
                "🏸"
            ),
            Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Bóng rổ",
                "Bóng rổ là môn thể thao đồng đội trong đó hai đội, thường là 5 người chơi mỗi đội, thi đấu trên sân hình chữ nhật.",
                "🏀"
            ),
            Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Bóng đá",
                "Bóng đá là môn thể thao đồng đội được chơi giữa hai đội, mỗi đội có 11 cầu thủ thi đấu với một quả bóng tròn.",
                "⚽"
            )
        };

        await context.Sports.AddRangeAsync(sports);
        await context.SaveChangesAsync();

        logger.LogInformation("Đã tạo {Count} môn thể thao", sports.Count);

        return sports;
    }

    private static async Task<List<Court>> SeedCourtsAsync(
        ApplicationDbContext context,
        ILogger logger,
        SportCenter sportCenter,
        List<Sport> sports)
    {
        logger.LogInformation("Seed dữ liệu sân thể thao...");

        var courts = new List<Court>();

        // Sân Tennis
        var tennisSport = sports.Find(s => s.Name == "Tennis");
        for (int i = 1; i <= 2; i++)
        {
            var tennisFacilities = new List<FacilityDTO>
            {
                new FacilityDTO { Name = "Vòi nước uống", Description = "Vòi nước uống miễn phí cho người chơi" },
                new FacilityDTO { Name = "Ghế nghỉ", Description = "Ghế nghỉ cho vận động viên giữa hiệp đấu" },
                new FacilityDTO { Name = "Lưới chất lượng cao", Description = "Lưới tennis tiêu chuẩn quốc tế" },
                new FacilityDTO { Name = "Khu vực khán giả", Description = "Khu vực dành cho 30 người xem" }
            };

            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                new CourtName($"Sân Tennis {i}"),
                sportCenter.Id,
                tennisSport.Id,
                TimeSpan.FromHours(1), // Thời lượng 1 giờ mỗi suất
                "Sân tennis chuyên nghiệp với bề mặt cao cấp và hệ thống chiếu sáng hiện đại.",
                JsonSerializer.Serialize(tennisFacilities),
                CourtType.Outdoor,
                80, // 80% đặt cọc
                24, // Hủy trước 24 giờ
                50  // Hoàn trả 50% khi hủy
            );

            courts.Add(court);
            sportCenter.AddCourt(court);
        }

        // Sân Cầu lông
        var badmintonSport = sports.Find(s => s.Name == "Cầu lông");
        for (int i = 1; i <= 3; i++)
        {
            var badmintonFacilities = new List<FacilityDTO>
            {
                new FacilityDTO { Name = "Phòng thay đồ", Description = "Phòng thay đồ riêng biệt cho nam và nữ" },
                new FacilityDTO { Name = "Điều hòa", Description = "Hệ thống điều hòa hiện đại giữ nhiệt độ lý tưởng" },
                new FacilityDTO { Name = "Hệ thống âm thanh", Description = "Hệ thống âm thanh cho giải đấu" },
                new FacilityDTO { Name = "Khu vực nghỉ ngơi", Description = "Khu vực nghỉ ngơi dành cho người chơi" }
            };

            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                new CourtName($"Sân Cầu Lông {i}"),
                sportCenter.Id,
                badmintonSport.Id,
                TimeSpan.FromMinutes(90), // Thời lượng 90 phút mỗi suất
                "Sân cầu lông trong nhà với hệ thống chiếu sáng và thông gió tốt.",
                JsonSerializer.Serialize(badmintonFacilities),
                CourtType.Indoor,
                50, // 50% đặt cọc
                12, // Hủy trước 12 giờ
                75  // Hoàn trả 75% khi hủy
            );

            courts.Add(court);
            sportCenter.AddCourt(court);
        }

        // Sân Bóng rổ
        var basketballSport = sports.Find(s => s.Name == "Bóng rổ");
        var basketballFacilities = new List<FacilityDTO>
        {
            new FacilityDTO { Name = "Bảng điểm điện tử", Description = "Bảng điểm điện tử hiển thị thời gian và điểm số" },
            new FacilityDTO { Name = "Ghế khán giả", Description = "Có 50 ghế dành cho khán giả" },
            new FacilityDTO { Name = "Phòng y tế", Description = "Phòng y tế sơ cứu tại chỗ" },
            new FacilityDTO { Name = "Tủ đựng đồ", Description = "Tủ đựng đồ có khóa an toàn cho người chơi" }
        };

        var basketballCourt = Court.Create(
            CourtId.Of(Guid.NewGuid()),
            new CourtName("Sân Bóng Rổ"),
            sportCenter.Id,
            basketballSport.Id,
            TimeSpan.FromHours(2), // Thời lượng 2 giờ mỗi suất
            "Sân bóng rổ tiêu chuẩn với vòng rổ chuyên nghiệp và sàn gỗ cao cấp.",
            JsonSerializer.Serialize(basketballFacilities),
            CourtType.Indoor,
            30, // 30% đặt cọc
            48, // Hủy trước 48 giờ
            100 // Hoàn trả 100% khi hủy
        );

        courts.Add(basketballCourt);
        sportCenter.AddCourt(basketballCourt);

        // Sân Bóng đá
        var soccerSport = sports.Find(s => s.Name == "Bóng đá");
        var soccerFacilities = new List<FacilityDTO>
        {
            new FacilityDTO { Name = "Phòng thay đồ", Description = "Phòng thay đồ riêng biệt cho hai đội" },
            new FacilityDTO { Name = "Ghế huấn luyện viên", Description = "Khu vực dành cho HLV và đội ngũ kỹ thuật" },
            new FacilityDTO { Name = "Hệ thống chiếu sáng", Description = "Hệ thống đèn LED chiếu sáng toàn sân" },
            new FacilityDTO { Name = "Khu vực khán đài", Description = "Khán đài có mái che dành cho 100 người" }
        };

        var soccerCourt = Court.Create(
            CourtId.Of(Guid.NewGuid()),
            new CourtName("Sân Bóng Đá"),
            sportCenter.Id,
            soccerSport.Id,
            TimeSpan.FromHours(2), // Thời lượng 2 giờ mỗi suất
            "Sân bóng đá cỏ nhân tạo chất lượng cao với kích thước tiêu chuẩn.",
            JsonSerializer.Serialize(soccerFacilities),
            CourtType.Outdoor,
            40, // 40% đặt cọc
            24, // Hủy trước 24 giờ
            50  // Hoàn trả 50% khi hủy
        );

        courts.Add(soccerCourt);
        sportCenter.AddCourt(soccerCourt);

        await context.Courts.AddRangeAsync(courts);
        await context.SaveChangesAsync();

        logger.LogInformation("Đã tạo {Count} sân thể thao", courts.Count);

        return courts;
    }

    private static async Task SeedCourtSchedulesAsync(
        ApplicationDbContext context,
        ILogger logger,
        List<Court> courts)
    {
        logger.LogInformation("Seed dữ liệu lịch sân...");

        var schedules = new List<CourtSchedule>();

        foreach (var court in courts)
        {
            // Lịch ngày thường (Thứ 2 đến Thứ 6)

            var dayOfWeekValue = new DayOfWeekValue(new[] { 1, 2, 3, 4, 5 });

            // Buổi sáng
            var morningSchedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                court.Id,
                dayOfWeekValue,
                new TimeSpan(6, 0, 0),  // 6 giờ sáng
                new TimeSpan(12, 0, 0), // 12 giờ trưa
                court.CourtType == CourtType.Indoor ? 120000m : 100000m // Sân trong nhà đắt hơn
            );
            morningSchedule.SetCreatedAt(DateTime.UtcNow);
            morningSchedule.SetLastModified(DateTime.UtcNow);
            schedules.Add(morningSchedule);

            // Buổi chiều
            var afternoonSchedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                court.Id,
                dayOfWeekValue,
                new TimeSpan(12, 0, 0), // 12 giờ trưa
                new TimeSpan(18, 0, 0), // 6 giờ chiều
                court.CourtType == CourtType.Indoor ? 150000m : 130000m // Giá cao hơn vào giờ cao điểm
            );
            afternoonSchedule.SetCreatedAt(DateTime.UtcNow);
            afternoonSchedule.SetLastModified(DateTime.UtcNow);
            schedules.Add(afternoonSchedule);

            // Buổi tối
            var eveningSchedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                court.Id,
                dayOfWeekValue,
                new TimeSpan(18, 0, 0), // 6 giờ chiều
                new TimeSpan(22, 0, 0), // 10 giờ tối
                court.CourtType == CourtType.Indoor ? 180000m : 160000m // Giá cao nhất vào buổi tối
            );
            eveningSchedule.SetCreatedAt(DateTime.UtcNow);
            eveningSchedule.SetLastModified(DateTime.UtcNow);
            schedules.Add(eveningSchedule);

            // Lịch cuối tuần (Thứ 7 và Chủ nhật)
            var weekendDayOfWeekValue = new DayOfWeekValue(new[] { 6, 7 }); // 0=Chủ nhật, 6=Thứ 7

            // Cả ngày với giá cao hơn
            var weekendSchedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                court.Id,
                weekendDayOfWeekValue,
                new TimeSpan(6, 0, 0),  // 6 giờ sáng
                new TimeSpan(22, 0, 0), // 10 giờ tối
                court.CourtType == CourtType.Indoor ? 200000m : 180000m // Giá cao nhất vào cuối tuần
            );
            weekendSchedule.SetCreatedAt(DateTime.UtcNow);
            weekendSchedule.SetLastModified(DateTime.UtcNow);
            schedules.Add(weekendSchedule);
        }

        await context.CourtSchedules.AddRangeAsync(schedules);
        await context.SaveChangesAsync();

        logger.LogInformation("Đã tạo {Count} lịch sân", schedules.Count);
    }

    private static async Task SeedCourtPromotionsAsync(
        ApplicationDbContext context,
        ILogger logger,
        List<Court> courts)
    {
        logger.LogInformation("Seed dữ liệu khuyến mãi sân...");

        var now = DateTime.UtcNow;
        var promotions = new List<CourtPromotion>();

        // Khuyến mãi sân Tennis
        var tennisCourts = courts.FindAll(c => c.CourtName.Value.Contains("Tennis"));
        foreach (var court in tennisCourts)
        {
            var promotion = CourtPromotion.Create(
                court.Id,
                "Ưu đãi cuối tuần - Đặt sớm để tiết kiệm!",
                "Percentage", // Loại giảm giá
                15, // Giảm 15%
                now,
                now.AddMonths(3) // Có hiệu lực trong 3 tháng
            );
            promotions.Add(promotion);
        }

        // Khuyến mãi sân Cầu lông
        var badmintonCourts = courts.FindAll(c => c.CourtName.Value.Contains("Cầu Lông"));
        foreach (var court in badmintonCourts)
        {
            var promotion = CourtPromotion.Create(
                court.Id,
                "Ưu đãi buổi sáng - Giảm 20% cho đặt sân buổi sáng!",
                "Percentage", // Loại giảm giá
                20, // Giảm 20%
                now,
                now.AddMonths(2) // Có hiệu lực trong 2 tháng
            );
            promotions.Add(promotion);
        }

        // Khuyến mãi sân Bóng rổ
        var basketballCourt = courts.Find(c => c.CourtName.Value == "Sân Bóng Rổ");
        var basketballPromotion = CourtPromotion.Create(
            basketballCourt.Id,
            "Gói đội bóng - Đặt 2 giờ liên tiếp được giảm 10%",
            "Percentage", // Loại giảm giá
            10, // Giảm 10%
            now,
            now.AddMonths(6) // Có hiệu lực trong 6 tháng
        );
        promotions.Add(basketballPromotion);

        // Khuyến mãi sân Bóng đá
        var soccerCourt = courts.Find(c => c.CourtName.Value == "Sân Bóng Đá");
        var soccerPromotion = CourtPromotion.Create(
            soccerCourt.Id,
            "Ưu đãi giải đấu - Giảm 25% cho đặt sân thường xuyên",
            "Percentage", // Loại giảm giá
            25, // Giảm 25%
            now,
            now.AddMonths(12) // Có hiệu lực trong 1 năm
        );
        promotions.Add(soccerPromotion);

        await context.CourtPromotions.AddRangeAsync(promotions);
        await context.SaveChangesAsync();

        logger.LogInformation("Đã tạo {Count} khuyến mãi sân", promotions.Count);
    }
}