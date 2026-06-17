using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ECommerceApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        public AuthService(ApplicationDbContext context) => _context = context;

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email)) return false;

            _context.Users.Add(new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Phone = model.Phone,
                Role = "Customer"
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByIdAsync(int id) =>
            await _context.Users.FindAsync(id);

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Register a Google user — skips password hashing; uses a sentinel value
        public async Task<bool> RegisterGoogleUserAsync(RegisterViewModel model)
        {
            // If user already exists (returning Google user), just return true
            if (await _context.Users.AnyAsync(u => u.Email == model.Email)) return true;

            _context.Users.Add(new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Customer"
            });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}