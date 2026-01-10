
using System.ComponentModel;
using System.Text;

namespace PageLeaf.Models
{
    public class MarkdownDocument : INotifyPropertyChanged
    {
        private string _content = "";
        private string? _filePath = null;
        private bool _isDirty = false;
        private System.Collections.Generic.Dictionary<string, object> _frontMatter = new System.Collections.Generic.Dictionary<string, object>();

        /// <summary>
        /// 本文（フロントマターを除いた部分）。
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// フロントマター。
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> FrontMatter
        {
            get => _frontMatter;
            set
            {
                if (_frontMatter != value)
                {
                    _frontMatter = value;
                    OnPropertyChanged(nameof(FrontMatter));
                    IsDirty = true;
                }
            }
        }

        public string? FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public Encoding? Encoding { get; set; }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
