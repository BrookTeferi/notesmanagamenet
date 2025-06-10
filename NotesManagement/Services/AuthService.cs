using NotesManagement.Models;
using NotesManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace NotesManagement.Services
{
    public class AuthService
    {
        private readonly UserDbContext _context;

        public AuthService(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User?> Register(string username, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return null;

            Console.WriteLine(username + " " + password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
              //  PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10, saltRevision: BCrypt.Net.SaltRevision.TravisSalt)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> ValidateUser(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return null;
            }

            Console.WriteLine($"Stored hash: {user.PasswordHash}");
            Console.WriteLine($"Input password: {password}");

            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            Console.WriteLine($"Password valid: {isValid}");

            return isValid ? user : null;
        }
        public async Task AddRefreshToken(string refreshToken, DateTime expiryDate, Guid userId)
        {
            var token = new RefreshToken
            {
                Token = refreshToken,
                Expires = expiryDate,
                UserId = userId
            };
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetValidRefreshToken(string refreshToken)
        {
            return await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken &&
                                          t.Expires > DateTime.UtcNow &&
                                          !t.IsRevoked);
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                _context.RefreshTokens.Update(token);
                await _context.SaveChangesAsync();
            }
        }
    }
}