using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Birth
{
    public class BirthRopository : IBirthRopository
    {
        private readonly AppDatabase _context;
        private readonly ILogger<BirthRopository> _logger;
        public BirthRopository (AppDatabase database, ILogger<BirthRopository> logger)
        {
            _context = database;
            _logger = logger;
        }
        public async Task<List<User>> GetUsersHaveBirthdayToday()
        {
            var today = DateTime.Today;

            return await _context.Users
                .Where(x =>
                    x.DateOfBirth != null &&
                    x.DateOfBirth.Value.Day == today.Day &&
                    x.DateOfBirth.Value.Month == today.Month &&
                    x.LastBirthdayEmailYear != today.Year)
                .ToListAsync();
        }
        public async Task UpdateLastBirthdayEmailYear(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return;
            }

            user.LastBirthdayEmailYear = DateTime.Now.Year;
            await _context.SaveChangesAsync();
        }
    }
}
