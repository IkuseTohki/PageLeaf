
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
                    // フロントマターが更新されると派生プロパティも変わるため通知
                    OnPropertyChanged(nameof(SuggestedCss));
                    OnPropertyChanged(nameof(PreferredSyntaxHighlight));
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// フロントマターで指定された推奨 CSS ファイル名を取得します。指定がない場合は null です。
        /// </summary>
        public string? SuggestedCss => FrontMatter.TryGetValue("css", out var v) ? v?.ToString() : null;

        /// <summary>
        /// フロントマターで指定された優先シンタックスハイライトテーマ名を取得します。指定がない場合は null です。
        /// </summary>
        public string? PreferredSyntaxHighlight => FrontMatter.TryGetValue("syntax_highlight", out var v) ? v?.ToString() : null;

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
