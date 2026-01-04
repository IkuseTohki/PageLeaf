
namespace PageLeaf.Models
{
    public class ApplicationSettings
    {
        public string SelectedCss { get; set; } = "";
        public string CodeBlockTheme { get; set; } = "github.css";
        public bool UseCustomCodeBlockStyle { get; set; } = false;

        // Default constructor for deserialization
        public ApplicationSettings() { }
    }
}
