using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRApp.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        # region INotifyPropertyChnaged Mambers

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string PropertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));


        }
        #endregion


    }
}
