using BookmarkManager.Shared;   //this gives our server access to your 4 shared blueprints.
using Microsoft.EntityFrameworkCore;
using System.Text.Json; // 🚀 ADDED: Required for converting lists to text strings

namespace BookmarkManager.Server
{
    public class ApplicationDbContext : DbContext
    {
        // This constructor lets the server configure our connection details later
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) 
        { 
        }

        // These turn your 4 C# blueprint files into active tables inside the database.
        public DbSet<User> Users { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Folder> Folders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================================
            // 🚀 CRITICAL MULTI-USER SQLITE FIX: SERIALIZE THE INT LIST
            // =========================================================================
            // This automatically turns List<int> into a JSON string text block for SQLite, 
            // and translates it back into a live C# List when reading data rows!
            modelBuilder.Entity<Workspace>()
                .Property(w => w.AllowedUserIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null)!,
                    v => JsonSerializer.Deserialize<List<int>>(v ?? "[]", (JsonSerializerOptions?)null) ?? new List<int>()
                );




            // This links Workspaces to Bookmarks based on your documentation guidelines.
            // It ensures that if a Workspace folder is deleted, all bookmarks inside it
            // are safely cleaned up too.
            modelBuilder.Entity<Workspace>()
                .HasMany(w => w.Bookmarks)
                .WithOne()
                .HasForeignKey(b => b.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hierarchical Cascade Delete Rule: If a custom Folder layout node is scrubbed, 
            // automatically cascade and erase all attached nested SubFolders and Bookmarks inside it!
            modelBuilder.Entity<Folder>()
                .HasMany(f => f.SubFolders)
                .WithOne()
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Folder>()
                .HasMany(f => f.Bookmarks)
                .WithOne()
                .HasForeignKey(b => b.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
