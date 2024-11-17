

public enum ContentType //depricated 
{
    Text = 0,               // Single block of text (RTF?)
    Image = 1,              // Image URL
    Video = 2,              // Video URL
    Layout = 3,             // Subcontainer that specifies layout of children
    Sound = 4,              // Sound URL
    Property = 5,            // TrackedObject property as string (only useful for containers that are attached to a trackedObject)
    WebUrl = 6              // URL to a webpage
}

// public abstract class ContentItem
// {
//     public virtual string ListElementLabelName()
//     {
//         return "Content Item";
//     }
//     public ContentType contentType;
// }

