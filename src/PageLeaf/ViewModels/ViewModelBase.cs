using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// プロパティ変更通知をサポートする ViewModel の基本クラスです。
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティが変更されたときに発生します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChanged イベントを発行します。
        /// </summary>
        /// <param name="propertyName">変更されたプロパティの名前。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
