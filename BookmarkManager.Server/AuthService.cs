using Microsoft.EntityFrameworkCore;
using BookmarkManager.Shared;
using System.Security.Cryptography;
using System.Text;

namespace BookmarkManager.Server
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection pulls our bookmarks.db connection profile straight into this service
        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. REGISTRATION ENGINE: Creates a brand-new user with a hashed password
        public async Task<bool> RegisterUserAsync(string username, string password, string extensionPin)
        {
            // Check if the username is already taken in our database tables
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
            {
                return false; // Registration fails: Username exists!
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = HashText(password),
                ExtensionPinHash = HashText(extensionPin) // Hashes your local extension lock PIN too!
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return true;
        }

        // 2. LOGIN ENGINE: Validates username and password credentials
        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null) return null; // User not found

            // Verify if the incoming password hash matches our stored database string
            string incomingHash = HashText(password);
            if (user.PasswordHash != incomingHash) return null; // Wrong password

            return user; // Login successful! Returns the user account object
        }

        // 3. CRYPTOGRAPHY UTILITY: Securely turns plain text strings into irreversible hashes
        public string HashText(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            
            var builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2")); // Converts bytes into standard hex text digits
            }
            return builder.ToString();
        }

    }
}