
using System;
using System.ComponentModel;
using System.Text;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PageLeaf.Utilities;

namespace PageLeaf.Models.Markdown
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

        /// <summary>
        /// 生のテキストからフロントマターと本文を解析し、ドキュメントの状態を構築します。
        /// </summary>
        /// <param name="rawText">解析対象のテキスト。</param>
        public void Load(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
            {
                FrontMatter = new System.Collections.Generic.Dictionary<string, object>();
                Content = string.Empty;
                IsDirty = false;
                return;
            }

            // フロントマターの解析
            var fm = new System.Collections.Generic.Dictionary<string, object>();
            var body = rawText;

            if (rawText.TrimStart().StartsWith("---"))
            {
                using (var reader = new StringReader(rawText.TrimStart()))
                {
                    var line = reader.ReadLine(); // First ---
                    if (line?.TrimEnd() == "---")
                    {
                        var yamlContent = new StringBuilder();
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.TrimEnd() == "---") break;
                            yamlContent.AppendLine(line);
                        }

                        if (yamlContent.Length > 0)
                        {
                            try
                            {
                                var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(NullNamingConvention.Instance)
                                    .Build();
                                fm = deserializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(yamlContent.ToString())
                                     ?? new System.Collections.Generic.Dictionary<string, object>();
                            }
                            catch { /* Ignore invalid YAML */ }
                        }

                        // 本文の抽出 (--- 直後の改行を考慮)
                        var firstDash = rawText.IndexOf("---");
                        var secondDash = rawText.IndexOf("---", firstDash + 3);
                        if (secondDash != -1)
                        {
                            var endOfDash = secondDash + 3;
                            if (rawText.Length > endOfDash)
                            {
                                if (rawText.Substring(endOfDash).StartsWith("\r\n")) endOfDash += 2;
                                else if (rawText.Substring(endOfDash).StartsWith("\n")) endOfDash += 1;
                            }
                            body = rawText.Substring(endOfDash);
                        }
                    }
                }
            }

            // プロパティへのセット（セッターで IsDirty = true になるのを防ぐため、バッキングフィールドへのセットまたは最後に IsDirty をリセット）
            _frontMatter = fm;
            _content = body;

            OnPropertyChanged(nameof(FrontMatter));
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(SuggestedCss));
            OnPropertyChanged(nameof(PreferredSyntaxHighlight));

            IsDirty = false;
        }

        /// <summary>
        /// フロントマターと本文を結合し、保存に適した形式の文字列を生成します。
        /// </summary>
        /// <returns>結合されたテキスト。</returns>
        public string ToFullString()
        {
            if (FrontMatter == null || FrontMatter.Count == 0)
            {
                return Content ?? string.Empty;
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(FrontMatter);
            var nl = Environment.NewLine;

            return "---" + nl + yaml + "---" + nl + (Content ?? string.Empty);
        }

        /// <summary>
        /// ドキュメント内の脚注番号を出現順に整理します。
        /// </summary>
        public void RenumberFootnotes()
        {
            if (string.IsNullOrEmpty(Content)) return;
            Content = MarkdownFootnoteHelper.Renumber(Content);
        }

        /// <summary>
        /// フロントマターの更新日時 (updated) を現在の日時で更新します。
        /// </summary>
        public void UpdateTimestamp()
        {
            if (FrontMatter != null && FrontMatter.Count > 0)
            {
                // 参照を新しくすることで変更通知を飛ばす
                var newFrontMatter = new System.Collections.Generic.Dictionary<string, object>(FrontMatter);
                newFrontMatter["updated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FrontMatter = newFrontMatter;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
