using System.Xml.Linq;

//this .shared is used to set rules, setting the options that the blazor has available for it to work with.

namespace BookmarkManager.Shared
{
    //Behavior Category for opinions
    public enum BookmarkCategory
    {
        Neutral,
        Liked,
        Disliked
    }

    public class Bookmark
    {
        //Unique ID for every bookmark in the database
        public int Id { get; set; }


        //The web address and title of the pages
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;


        //AI that summarizes the bookmarked websites for content awareness
        public string AiSummary {  get; set; } = string.Empty;


        //This connects bookmarks to the opinion category by allowing you to select like or dislike for the bookmark.
        //Default is Neutral
        public BookmarkCategory Category {  get; set; } = BookmarkCategory.Neutral;

        //bookmark's connection with the workspace
        public int WorkspaceId { get; set; }

        // Foreign Key link connecting this bookmark directly to its dynamic folder layout node row
        public int? FolderId { get; set; }


        //This is the status over if you've seen or not seen it, true = viewed, false = not viewed
        public bool IsViewed { get; set; }


        // A list of saved commentary/notes for this specific bookmark
        // This allows for future implementation of side-panels or notes per title
        //comments connection with bookmarks
        public List<Comment> Comments { get; set; } = new();


        //ID of user who saved the bookmark, and when the user made the save
        //user connection with bookmark
        public int CreatedBy { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
