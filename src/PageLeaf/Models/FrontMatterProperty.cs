using PageLeaf.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PageLeaf.Models
{
    /// <summary>
    /// フロントマターの1つのプロパティを表すクラスです。
    /// </summary>
    public class FrontMatterProperty : ViewModelBase
    {
        // キー名を変更できない予約語セット
        private static readonly HashSet<string> _fixedKeys = new HashSet<string>
        {
            "title",
            "tags",
            "created",
            "updated",
            "css",
            "syntax_highlight"
        };

        // 値を変更できない（システム管理）予約語セット
        private static readonly HashSet<string> _readOnlyValues = new HashSet<string>
        {
            "created",
            "updated"
        };

        private string _key = string.Empty;
        private object? _value;

        /// <summary>
        /// プロパティ名。
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged();
                    // キーが変わるとReadOnly状態なども変わる可能性があるため通知
                    OnPropertyChanged(nameof(IsKeyReadOnly));
                    OnPropertyChanged(nameof(IsValueReadOnly));
                    OnPropertyChanged(nameof(CanRemove));
                    OnPropertyChanged(nameof(IsTags));
                }
            }
        }

        /// <summary>
        /// プロパティの値。
        /// </summary>
        public object? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// タグのリスト（Valueがリスト形式の場合に使用）。
        /// </summary>
        public ObservableCollection<string>? Tags { get; set; }

        private string _newTagText = string.Empty;
        /// <summary>
        /// 新規追加用タグの入力テキスト。
        /// </summary>
        public string NewTagText
        {
            get => _newTagText;
            set
            {
                if (_newTagText != value)
                {
                    _newTagText = value;
                    OnPropertyChanged();
                }
            }
        }

        // View 制御用プロパティ

        /// <summary>
        /// キー（プロパティ名）が変更可能かどうか。
        /// </summary>
        public bool IsKeyReadOnly => _fixedKeys.Contains(Key);

        /// <summary>
        /// 値が変更可能かどうか。
        /// </summary>
        public bool IsValueReadOnly => _readOnlyValues.Contains(Key);

        /// <summary>
        /// このプロパティを削除できるかどうか。
        /// </summary>
        public bool CanRemove => !_fixedKeys.Contains(Key);

        /// <summary>
        /// タグ編集モードかどうか。
        /// </summary>
        public bool IsTags => Key == "tags";
    }
}
