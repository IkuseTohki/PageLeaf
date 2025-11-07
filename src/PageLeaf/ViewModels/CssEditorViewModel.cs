namespace PageLeaf.ViewModels
{
    public class CssEditorViewModel : ViewModelBase
    {
        private string? _bodyTextColor;
        private string? _bodyBackgroundColor;

        public string? BodyTextColor
        {
            get => _bodyTextColor;
            set
            {
                if (_bodyTextColor != value)
                {
                    _bodyTextColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? BodyBackgroundColor
        {
            get => _bodyBackgroundColor;
            set
            {
                if (_bodyBackgroundColor != value)
                {
                    _bodyBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
