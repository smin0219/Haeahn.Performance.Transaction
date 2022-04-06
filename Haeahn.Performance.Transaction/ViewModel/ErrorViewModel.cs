using Haeahn.Performance.Transaction.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Haeahn.Performance.Transaction.ViewModel
{
    internal class ErrorViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged 멤버
        public void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region 멤버변수

        #endregion

        #region 프로퍼티
        private ObservableCollection<ErrorItem> _itemCollection;
        public ObservableCollection<ErrorItem> ItemCollection
        {
            get { return _itemCollection; }
            set
            {
                if (value != _itemCollection)
                {
                    _itemCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region 커맨드
        public virtual DelegateCommand<Window> CloseCommand
        {
            get
            {
                return new DelegateCommand<Window>(x => x.Close());
            }
        }
        #endregion

        #region 생성자
        public ErrorViewModel(ObservableCollection<ErrorItem> itemCollection)
        {
            this._itemCollection = itemCollection;

        }

        #endregion
    }
}
