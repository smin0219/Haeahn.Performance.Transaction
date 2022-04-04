using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Model
{
    public class ErrorItem : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged 멤버
        public void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private string _code;
        public string Code
        {
            get { return _code; }
            set
            {
                if (value != _code)
                {
                    _code = value.Trim();
                    RaisePropertyChanged();
                }
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value.Trim();
                    RaisePropertyChanged();
                }
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (value != _description)
                {
                    _description = value.Trim();
                    RaisePropertyChanged();
                }
            }
        }
    }
}
