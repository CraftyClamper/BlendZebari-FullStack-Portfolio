using System;
using System.Collections.Generic;
using System.Text;

namespace BookmarkManager.Shared
{
    public class Workspace
    {
        //ID belonging to each Folder bookmarks workspace
        public int Id { get; set; }

        //Name of Folder bookmark
        //the bookmark folder that belongs to the workspace
        public string Name { get; set; } = string.Empty;

        //If you wish to allow more than one user access to Folder
        public bool IsCollaborative { get; set; }


        //Stores IDs of friends who have access to this workspace
        //the user access to the workspaces where the bookmarks are placed
        public List<int> AllowedUserIds { get; set; } = new();

        //Navigational property: linking bookmarks directly to this workspace
        //ensures cleaner API data retreival

        public List<Bookmark> Bookmarks { get; set; } = new();
    }
}
