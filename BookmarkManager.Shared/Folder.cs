using System;
using System.Collections.Generic;

namespace BookmarkManager.Shared
{
    public class Folder
    {
        // Unique tracking key for each custom folder created in the database
        public int Id { get; set; }

        // The user-defined custom name string (e.g., "Manga", "Berserk Notes")
        public string Name { get; set; } = string.Empty;

        // Recursive Anchor: Links this folder directly to its master folder container.
        // If Null, this is a top-level primary workspace directory!
        public int? ParentFolderId { get; set; }

        // Connecting reference linking folders directly to a custom parent workspace
        public int WorkspaceId { get; set; }

        // The exact database account profile user ID string who created this custom directory node
        public int CreatedBy { get; set; }

        // Navigational helper collection tracking nested child folder nodes sitting directly inside this tree branch
        public List<Folder> SubFolders { get; set; } = new();

        // Navigational helper collection linking active bookmarks tied directly to this folder bucket
        public List<Bookmark> Bookmarks { get; set; } = new();
    }
}

