using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.Data.Repositories
{
    public class SportRepository : ISportRepository
    {
        private readonly IApplicationDbContext _context;

        public SportRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddSportAsync(Sport sport, CancellationToken cancellationToken)
        {
            await _context.Sports.AddAsync(sport, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Sport> GetSportByIdAsync(SportId sportId, CancellationToken cancellationToken)
        {
            return await _context.Sports.FindAsync(new object[] { sportId }, cancellationToken);
        }

        public async Task UpdateSportAsync(Sport sport, CancellationToken cancellationToken)
        {
            _context.Sports.Update(sport);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteSportAsync(SportId sportId, CancellationToken cancellationToken)
        {
            var sport = await _context.Sports.FindAsync(new object[] { sportId }, cancellationToken);
            if (sport != null)
            {
                _context.Sports.Remove(sport);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<Sport>> GetAllSportsAsync(CancellationToken cancellationToken)
        {
            return await _context.Sports.ToListAsync(cancellationToken);
        }

        public async Task<bool> IsSportInUseAsync(SportId sportId, CancellationToken cancellationToken)
        {
            return await _context.Courts.AnyAsync(c => c.SportId == sportId, cancellationToken);
        }

        public async Task<Sport> GetByIdAsync(SportId sportId, CancellationToken cancellationToken)
        {
            return await _context.Sports
                .FirstOrDefaultAsync(s => s.Id == sportId, cancellationToken);
        }

        public async Task<List<Sport>> GetSportsByIdsAsync(List<SportId> sportIds, CancellationToken cancellationToken)
        {
            var guidIds = sportIds.Select(id => id).ToList(); // Trích xuất danh sách Guid từ SportId
            return await _context.Sports
                .Where(s => guidIds.Contains(s.Id)) // So sánh với Guid
                .ToListAsync(cancellationToken);
        }
        public async Task<Sport> GetByName(string name, CancellationToken cancellationToken)
        {
            return await _context.Sports.FirstOrDefaultAsync(p => EF.Functions.ILike(p.Name, name), cancellationToken);
        }
    }
}