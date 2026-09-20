# 📂 Sub-Project: Full-Stack Authenticated BookmarkManager

This directory houses the core presentation host and structural architecture for the **BookmarkManager Suite**—a highly secure, multi-tenant knowledge vault system. The application bridges a modern reactive web dashboard ecosystem with lower-level browser APIs and real-time networking protocols to scrape, analyze, and synchronize web data instantly.

---

## 🛠️ Deep-Dive Subsystem Architecture

The subsystem maps operations across an isolated presentation block and an decoupled backend tier to enforce clean **Separation of Concerns (SoC)**:

### 1. ⚙️ Background Infrastructure Engine (`BookmarkManagerInfrastructure`)
The foundational business logic, database controllers, and communication systems are encapsulated within a dedicated infrastructure domain tier:
* **Hybrid Storage & Migration Matrix (`ApplicationDbContext.cs`):** Integrates a dual compilation pipeline switch. The framework seamlessly shifts storage operations from an isolated local **SQLite sandbox database** (`bookmarks.db`) for rapid local development, over to high-availability **PostgreSQL network clusters** for live multi-user staging deployments.
* **EF Core SQLite Custom Serialization:** Because SQLite lacks native array data-type storage primitives, the database context implements a custom `HasConversion` mapping interceptor. This layer automatically serializes primitive arrays (`List<int> AllowedUserIds`) into flat, queryable JSON blocks on write streams, translating them back into live C# tracking collections during database read operations.
* **Hierarchical Cascade Maintenance:** Custom foreign key metadata mapping patterns ensure strict database hygiene. Deleting a parent workspace layout or clearing a custom folder node triggers a cascade wipe, safely scrubbing all attached nested subfolders, commentary entries, and bookmark rows from disk.
* **Cryptographic Identity Validation Engine (`AuthService.cs`):** Manages secure sign-ins and new user provisioning. User credentials and 4-digit companion tool unlock codes are passed through an irreversible, high-speed **SHA256 hashing sequence**, writing clean hex digits to database cells rather than plane text strings.
* **Asynchronous Deep-Parse Web Crawler (`MetadataScraper.cs`):** When a new web destination is pushed down the pipeline, the scraper fires an asynchronous HTTP worker pool configured with a tailored browser `User-Agent`. It runs a three-tier parsing loop:
  1. *YouTube Sniffing:* Extracts raw video logs via specialized inner script query matching (`shortDescription`).
  2. *Universal Schema Processing:* Targets structured `application/ld+json` definitions.
  3. *Baseline Meta Ingestion:* Extracts data from `og:description` or generic web description properties as a fallback.
* **WebSocket Radio Towers (`SyncHub.cs` / `ChatHub.cs`):** Leverages full-duplex SignalR WebSockets. `ChatHub` segregates communication lines into isolated workspace streams (`WorkspaceChat_{id}`), while `SyncHub` monitors remote remote captures. When the browser extension saves a link, `SyncHub` broadcasts a silent background refresh command (`TriggerLayoutRefresh`) strictly to that user's group, forcing their active Blazor views to reload instantly without losing layout focus.

### 2. 🛡️ State-Protected Presentation Interfaces (`Components/Pages`)
The web interfaces are engineered using asynchronous server rendering pipelines, fortified with custom browser-isolated routing guards:
* **`Login.razor` & `Register.razor` (Token Verification Funnels):** To prevent malicious URL injection exploits (manually typing addresses to bypass login checks), these pages utilize a secure **One-Time Pass Protocol** backed by `ProtectedLocalStorage`. Components will completely self-abort and force a redirect unless an authentic button-click event seeds an active tracking token (`ButtonNavigationClearance`). This token is scrubbed from browser memory the millisecond the component runs.
* **Cookie Handshake Bridge:** Upon authentication, the server injects an encrypted browser-wide session token (`VaultSession`). This cookie runs on secure HTTPS channels with explicit `SameSite=None` flags, enabling independent client scripts (like the Chrome Extension) to authenticate active profiles instantly from any tab open across the browser.
* **`Sandbox.razor` (Interactive Hierarchical File Manager):** Renders a multi-tenant dashboard driven by recursive layout fragments (`RenderFolderNode` and `RenderDropdownOption`). This allows users to build infinitely nested folder directory sub-trees on screen while preserving precise data tracking. It features an adaptive, Steam-style boolean switch filtering mechanism that dynamically hides items marked as `Disliked` from active views without severing them from underlying tables.
* **`Dashboard.razor` (Real-Time Workspace Overview):** Displays a live, multi-user workspace map. Each user is represented by a unique avatar icon, and their current activity is tracked in real-time via SignalR WebSocket events. The dashboard updates instantly as users add, remove, or modify bookmarks and folders.

### 3. 🧩 Universal Schemas & Integration Contracts (`Shared` / `Extension`)
* **`BookmarkManager.Shared`:** A cross-cutting class library standardizing data payloads (`User`, `Workspace`, `Folder`, `Bookmark`, `Comment`) globally across all compilation domains.
* **`BookmarkManager.Extension`:** A **Manifest V3 Google Chrome Extension** that acts as the hardware input driver for the system, grabbing browser data via `chrome.tabs` and shipping verified capture objects down to the server API endpoints instantly.

---

## 🚀 Step-by-Step Local Deployment Sequence

1. Open your terminal window and verify your root path directory maps cleanly to your system setup:
   ```cmd
   cd C:\Users\Blend\source\repos\Portfolio
   ```
2. Open **`Portfolio.slnx`** inside Visual Studio 2026. Open the Package Manager Console (**Tools > NuGet Package Manager > Package Manager Console**) and build your local SQLite database architecture by running:
   ```powershell
   Update-Database -Project Portfolio.Server
   ```
   *(Note: If changes are made to models during deployment, generate a consolidation blueprint first via `Add-Migration ContextUpdate -Project Portfolio.Server` and then execute the database update).*
3. Set **`Portfolio.Server`** as your executable startup target project on the top toolbar menu.
4. Press **`Ctrl + F5`** to compile and run the application host environment.
5. Launch Google Chrome, navigate to `chrome://extensions/`, enable **Developer mode**, click **Load unpacked**, and select your physical `BookmarkManager.Extension` directory folder to mount the capture tool!
6. Press **`Ctrl + Shift + Y`** to open the extension popup and start capturing bookmarks directly from your browser.
