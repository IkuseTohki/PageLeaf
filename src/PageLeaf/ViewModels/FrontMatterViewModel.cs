using PageLeaf.Models;
using PageLeaf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// フロントマター編集パネルのViewModelクラスです。
    /// </summary>
    public class FrontMatterViewModel : ViewModelBase
    {
        private readonly IEditorService _editorService;
        private ObservableCollection<FrontMatterProperty> _properties = new ObservableCollection<FrontMatterProperty>();
        private bool _isExpanded = true;
        private bool _isInternalChange; // 内部変更中フラグ

        /// <summary>
        /// プロパティのリスト。
        /// </summary>
        public ObservableCollection<FrontMatterProperty> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// パネルが展開されているかどうか。
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddPropertyCommand { get; }
        public ICommand RemovePropertyCommand { get; }
        public ICommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }

        public FrontMatterViewModel(IEditorService editorService)
        {
            _editorService = editorService;
            AddPropertyCommand = new Utilities.DelegateCommand(ExecuteAddProperty);
            RemovePropertyCommand = new Utilities.DelegateCommand(ExecuteRemoveProperty);
            AddTagCommand = new Utilities.DelegateCommand(ExecuteAddTag);
            RemoveTagCommand = new Utilities.DelegateCommand(ExecuteRemoveTag);

            _editorService.PropertyChanged += EditorService_PropertyChanged;

            if (_editorService.CurrentDocument != null)
            {
                _editorService.CurrentDocument.PropertyChanged += Document_PropertyChanged;
            }

            SyncFromDocument();
        }

        private void EditorService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IEditorService.CurrentDocument))
            {
                if (_editorService.CurrentDocument != null)
                {
                    _editorService.CurrentDocument.PropertyChanged += Document_PropertyChanged;
                }
                SyncFromDocument();
            }
        }

        private void Document_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 外部（保存処理など）からの変更時のみ同期する
            if (!_isInternalChange && e.PropertyName == nameof(MarkdownDocument.FrontMatter))
            {
                SyncFromDocument();
            }
        }

        /// <summary>
        /// ドキュメントのフロントマターからViewModelの状態を同期します。
        /// </summary>
        public void SyncFromDocument()
        {
            var doc = _editorService.CurrentDocument;
            if (doc == null) return;

            // 内部変更中の場合は、UIのフォーカスを維持するためコレクションを再生成しない
            if (_isInternalChange) return;

            var newProps = new List<FrontMatterProperty>();
            foreach (var kvp in doc.FrontMatter)
            {
                var prop = new FrontMatterProperty { Key = kvp.Key, Value = kvp.Value };

                // tags の特別な処理
                if (kvp.Key == "tags" && kvp.Value is System.Collections.IEnumerable list)
                {
                    prop.Tags = new ObservableCollection<string>(list.Cast<object>().Select(o => o.ToString() ?? string.Empty));
                    prop.Tags.CollectionChanged += (s, e) => SyncToDocument();
                }

                prop.PropertyChanged += Property_Changed;
                newProps.Add(prop);
            }

            Properties = new ObservableCollection<FrontMatterProperty>(newProps);
        }

        private void Property_Changed(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // NewTagText の変更はフロントマターの内容ではないため同期しない
            if (e.PropertyName == nameof(FrontMatterProperty.NewTagText)) return;

            SyncToDocument();
        }

        /// <summary>
        /// ViewModelの状態をドキュメントのフロントマターに反映します。
        /// </summary>
        private void SyncToDocument()
        {
            var doc = _editorService.CurrentDocument;
            if (doc == null) return;

            _isInternalChange = true;
            try
            {
                var dict = new Dictionary<string, object>();
                foreach (var prop in Properties)
                {
                    if (string.IsNullOrWhiteSpace(prop.Key)) continue;

                    if (prop.Key == "tags" && prop.Tags != null)
                    {
                        dict[prop.Key] = prop.Tags.ToList();
                    }
                    else
                    {
                        dict[prop.Key] = prop.Value ?? string.Empty;
                    }
                }
                doc.FrontMatter = dict;
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        private void ExecuteAddProperty(object? parameter)
        {
            var prop = new FrontMatterProperty { Key = "new_property", Value = "" };
            prop.PropertyChanged += Property_Changed;
            Properties.Add(prop);
            SyncToDocument();
        }

        private void ExecuteRemoveProperty(object? parameter)
        {
            if (parameter is FrontMatterProperty prop)
            {
                prop.PropertyChanged -= Property_Changed;
                Properties.Remove(prop);
                SyncToDocument();
            }
        }

        private void ExecuteAddTag(object? parameter)
        {
            if (parameter is FrontMatterProperty prop)
            {
                var tagText = prop.NewTagText;
                if (!string.IsNullOrWhiteSpace(tagText))
                {
                    prop.Tags ??= new ObservableCollection<string>();
                    var newTag = tagText.Trim();
                    if (!prop.Tags.Contains(newTag))
                    {
                        prop.Tags.Add(newTag);
                    }
                    prop.NewTagText = string.Empty; // 追加後にクリア
                }
            }
        }

        private void ExecuteRemoveTag(object? parameter)
        {
            if (parameter is object[] args && args.Length == 2 && args[0] is FrontMatterProperty prop && args[1] is string tagText)
            {
                if (prop.Tags != null)
                {
                    prop.Tags.Remove(tagText);
                }
            }
        }
    }
}
