
namespace PageLeaf.Models
{
    public enum FolderTreePosition
    {
        Left,
        Right
    }

    public class ApplicationSettings
    {
        public FolderTreePosition FolderTreePosition { get; set; }
        public string SelectedCss { get; set; } = "";
        public string LastOpenedFolder { get; set; } = "";
    }
}
