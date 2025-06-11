using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.SentMessages)
                .Include(u => u.ReceivedMessages)
                .Include(u => u.Notifications)
                .Include(u => u.ReviewsGiven)
                .Include(u => u.ReviewsReceived)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.SentMessages)
                .Include(u => u.ReceivedMessages)
                .Include(u => u.Notifications)
                .Include(u => u.ReviewsGiven)
                .Include(u => u.ReviewsReceived)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<ApplicationUser> CreateAsync(ApplicationUser user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}

