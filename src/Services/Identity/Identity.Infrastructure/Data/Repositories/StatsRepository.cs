using Identity.Application.Dtos;
using Identity.Application.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Identity.Domain.Models;

namespace Identity.Infrastructure.Data.Repositories
{
    public class StatsRepository : IStatsRepository
    {
        private readonly IdentityDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public StatsRepository(IdentityDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var usersQuery = _dbContext.Users.AsQueryable();
            var users = await usersQuery.ToListAsync();
            int totalCourtOwners = 0;
            int totalCoaches = 0;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("CourtOwner")) totalCourtOwners++;
                if (roles.Contains("Coach")) totalCoaches++;
            }

            return new DashboardStatsDto
            {
                TotalUsers = users.Count,
                TotalCourtOwners = totalCourtOwners,
                TotalCoaches = totalCoaches,
            };
        }
    }
}