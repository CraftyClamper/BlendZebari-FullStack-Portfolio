using System;
using System.Collections.Generic;
using System.Text;

namespace BookmarkManager.Shared
{
    public class Comment
    {
        //the id belonging to the specific belonging comments
        public int Id { get; set; }

        //bookmarks connection with comments made
        public int BookmarkId { get; set; }

        //userid connected with comments
        public int UserId { get; set; }

        //comments made
        public string Text { get; set; } = string.Empty;

        //time of comments
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
