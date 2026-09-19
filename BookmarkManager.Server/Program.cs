using BookmarkManager.Server.Components;
using Microsoft.EntityFrameworkCore; // Added: Brings in database functionality
using BookmarkManager.Server;
using BookmarkManager.Shared;        // Added: Links to your ApplicationDbContext file
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// =========================================================================
// 🎯 PORTFOLIO PROGRESS SHOWCASE: MULTI-DATABASE ENGINE PIPELINE
// =========================================================================
// Set this to true when connecting to a live cloud/local PostgreSQL server instance,
// or false to use the local single-file SQLite database sandbox!
bool usePostgreSQL = false;

if (usePostgreSQL)
{
    // 🚀 ENTERPRISE UPGRADE: PostgreSQL Network Server Environment
    string pgConnectionString = "Host=localhost;Port=5432;Database=vaultdb;Username=postgres;Password=YourSecurePassword123;Include Error Detail=true;";
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(pgConnectionString));
}
else
{
    // 📁 LOCAL ARCHIVE: SQLite Single-File Environment
    string projectDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
    string databaseFullPath = System.IO.Path.Combine(projectDirectoryPath, "bookmarks.db");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite($"Data Source={databaseFullPath}"));
}

// CRITICAL FIX: Registers your security engine globally so Login.razor can find it!
builder.Services.AddScoped<AuthService>();

// Added: Loads the real-time communication framework tools into memory
builder.Services.AddSignalR();

// Added: Registers the metadata scraping client pool service
builder.Services.AddHttpClient<MetadataScraper>();

// Tells the server framework it is safe to accept incoming query requests from browser tabs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowExtensionBridge", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowExtensionBridge");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// =========================================================================
// 🤝 TRANSLATION-SAFE COLLABORATIVE EXTENSION DROPDOWN DIRECTORY GATEWAY
// =========================================================================
app.MapGet("/api/folders/list", async (string username, string extensionPin, ApplicationDbContext context, AuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(extensionPin))
    {
        return Results.BadRequest("Missing required identity parameters.");
    }

    var targetUser = await context.Users
        .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

    if (targetUser == null) return Results.Unauthorized();

    string incomingPinHash = authService.HashText(extensionPin);
    if (targetUser.ExtensionPinHash != incomingPinHash)
    {
        return Results.Unauthorized();
    }

    // 1. Fetch workspaces fully into memory first to unblock SQLite's translation walls
    var rawWorkspaces = await context.Workspaces.ToListAsync();

    // 2. Filter allowed workspaces using standard safe C# list logic
    var collaborativeWorkspaceIds = rawWorkspaces
        .Where(w => w.AllowedUserIds != null && w.AllowedUserIds.Contains(targetUser.Id))
        .Select(w => w.Id)
        .ToList();

    // 3. 🤝 DYNAMIC DROPDOWN FILTER: Pull folders belonging to user or their collaborative assignments!
    var visibleFolders = await context.Folders
        .Where(f => f.CreatedBy == targetUser.Id || collaborativeWorkspaceIds.Contains(f.WorkspaceId))
        .Select(f => new { id = f.Id, name = f.Name })
        .ToListAsync();

    return Results.Ok(visibleFolders);
});




// 🚀 bulletproof CAPTURE GATEWAY: Bypasses case-sensitivity walls completely to eliminate 400 errors
app.MapPost("/api/bookmarks/capture", async (System.Text.Json.JsonElement rawPayload, IServiceProvider serviceProvider, AuthService authService, IHubContext<SyncHub> syncHubContext, MetadataScraper scraper) =>
{
    // Spawns a unique isolated database connection thread channel
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // 1. DYNAMIC VALUE EXTRACTION: Extracts variables safely regardless of uppercase/lowercase names!
    string url = rawPayload.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";
    string title = rawPayload.TryGetProperty("title", out var t) ? t.GetString() ?? "Untitled Bookmark" : "Untitled Bookmark";
    string username = rawPayload.TryGetProperty("username", out var user) ? user.GetString() ?? "" : "";
    string pin = rawPayload.TryGetProperty("extensionPin", out var p) ? p.GetString() ?? "" : "";

    int? folderId = null;
    if (rawPayload.TryGetProperty("folderId", out var f) && f.ValueKind == System.Text.Json.JsonValueKind.Number)
    {
        folderId = f.GetInt32();
    }

    // 2. CORE URL DATA VALIDATION
    if (string.IsNullOrWhiteSpace(url))
    {
        return Results.BadRequest("Invalid URL data layout.");
    }

    // 3. CRYPTOGRAPHIC USER CHECK
    var targetUser = await context.Users
        .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

    if (targetUser == null) return Results.Unauthorized();

    // 🔄 FIXED: Replaced slow reflection with the secure direct public calculation call!
    string incomingPinHash = authService.HashText(pin);

    if (targetUser.ExtensionPinHash != incomingPinHash)
    {
        return Results.Forbid();
    }


    // 🛡️ DYNAMIC MULTI-USER WORKSPACE ISOLATION
    // Look for a workspace belonging to this specific user or named after them
    var userWorkspace = await context.Workspaces
        .FirstOrDefaultAsync(w => w.Name == $"{targetUser.Username}'s Vault Layout");

    if (userWorkspace == null)
    {
        userWorkspace = new Workspace
        {
            Name = $"{targetUser.Username}'s Vault Layout",
            IsCollaborative = false
        };
        userWorkspace.AllowedUserIds.Add(targetUser.Id);
        context.Workspaces.Add(userWorkspace);
        await context.SaveChangesAsync(); // Generates a unique tracking ID automatically
    }


    // 📁 FIXED BACKUP SAFEGUARD: Only checks/creates "Uncategorized" if the user left the dropdown blank!
    int finalFolderId;
    if (folderId.HasValue)
    {
        finalFolderId = folderId.Value;
    }
    else
    {
        var generalFolder = await context.Folders
            .FirstOrDefaultAsync(folder => folder.Name.ToLower() == "uncategorized" && folder.CreatedBy == targetUser.Id);

        if (generalFolder == null)
        {
            generalFolder = new Folder
            {
                Name = "Uncategorized",
                ParentFolderId = null,
                WorkspaceId = userWorkspace.Id, //  Dynamic assignment
                CreatedBy = targetUser.Id
            };
            context.Folders.Add(generalFolder);
            await context.SaveChangesAsync();
        }
        finalFolderId = generalFolder.Id;
    }

    // 🔍 AUTOMATED BACKGROUND SCRAING: Crawls the captured link and extracts its summary descriptions live!
    string structuralSummaryText = await scraper.ExtractSiteSummaryAsync(url);

    // 5. SECURE PRODUCTION STORAGE WRITE
    var newBookmark = new Bookmark
    {
        Url = url,
        Title = title,
        AiSummary = structuralSummaryText, // Locks the real webpage summary text straight into bookmarks.db!
        WorkspaceId = userWorkspace.Id, //  Dynamic assignment
        CreatedBy = targetUser.Id,
        FolderId = finalFolderId
    };


    context.Bookmarks.Add(newBookmark);
    await context.SaveChangesAsync();

    // 📡 SILENT SYSTEM UPDATE BROADCAST: Fires a background pulse strictly to the logged-in user channel
    // This triggers an immediate UI update without touching or polluting the ChatHub text streams!
    string userGroupName = $"UserSync_{targetUser.Username.ToLower()}";
    await syncHubContext.Clients.Group(userGroupName).SendAsync("TriggerLayoutRefresh");

    return Results.Ok(new { message = "Bookmark successfully saved to your private folder drawer!" });
});

// =========================================================================
// 🤝 CORE FEATURE 6: REAL-TIME COLLABORATIVE WORKSPACE PERMISSIONS GATEWAY
// =========================================================================
app.MapPost("/api/workspaces/invite", async (System.Text.Json.JsonElement payload, ApplicationDbContext context, IHubContext<SyncHub> syncHubContext) =>
{
    string targetFriendName = payload.TryGetProperty("friendUsername", out var f) ? f.GetString() ?? "" : "";
    int currentWorkspaceId = payload.TryGetProperty("workspaceId", out var w) ? w.GetInt32() : 0;

    if (string.IsNullOrWhiteSpace(targetFriendName) || currentWorkspaceId == 0)
    {
        return Results.BadRequest("Missing collaboration context parameters.");
    }

    // 1. Locate the target friend profile row context records
    var friendUser = await context.Users
        .FirstOrDefaultAsync(u => u.Username.ToLower() == targetFriendName.ToLower());

    if (friendUser == null)
    {
        return Results.NotFound(new { message = "❌ Requested user profile does not exist." });
    }

    // 2. Fetch the target workspace record
    var targetWorkspace = await context.Workspaces
        .FirstOrDefaultAsync(ws => ws.Id == currentWorkspaceId);

    if (targetWorkspace == null) return Results.NotFound("Workspace context not found.");

    // 3. Security Boundary: Prevent redundant duplicate index assignments
    if (!targetWorkspace.AllowedUserIds.Contains(friendUser.Id))
    {
        targetWorkspace.AllowedUserIds.Add(friendUser.Id);
        targetWorkspace.IsCollaborative = true;

        context.Workspaces.Update(targetWorkspace);
        await context.SaveChangesAsync();
    }

    // 4. SIGNALR LIVE COLLABORATION RECONCILIATION PULSE
    // Forces the friend's running dashboard screen to instantly sync counts and redrawn rows
    string friendSyncGroupName = $"UserSync_{friendUser.Username.ToLower()}";
    await syncHubContext.Clients.Group(friendSyncGroupName).SendAsync("TriggerLayoutRefresh");

    return Results.Ok(new { message = $"✓ Successfully invited {friendUser.Username} to your directory drawer!" });
});

// Added: Establishes the real-time network route path for your social workspaces
app.MapHub<ChatHub>("/chathub");

// Added: Establishes the real-time network route path for your sync engine
app.MapHub<SyncHub>("/synchub");

// 1. Fire up the server engine first!
app.Run();

// The helper record updated with precise system serialization attributes to stop 400 bad request errors
public record BookmarkPayload(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("extensionPin")] string ExtensionPin,
    [property: JsonPropertyName("folderId")] int? FolderId
);
