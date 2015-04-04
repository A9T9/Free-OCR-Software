using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace OCRApp.ViewModel
{
    public class Images : BaseViewModel
    {

        public Images()
        {
            _lstImageDisplay = new ObservableCollection<List<ImageSource>>();
            _lstWriteImages = new ObservableCollection<List<WriteableBitmap>>();
        }

        private ObservableCollection<List<ImageSource>> _lstImageDisplay;
        public ObservableCollection<List<ImageSource>> ListImageDisplay
        {
            get
            {
                return _lstImageDisplay;
            }
            set
            {
                if (value != null)
                {
                    _lstImageDisplay = value;
                    NotifyPropertyChanged("ListImageDisplay");
                }
            }

        }

        private ObservableCollection<List<WriteableBitmap>> _lstWriteImages;
        public ObservableCollection<List<WriteableBitmap>> ListWriteImages
        {
            get
            {
                return _lstWriteImages;
            }
            set
            {
                if (value != null)
                {
                    _lstWriteImages = value;
                    NotifyPropertyChanged("ListWriteImages");
                }
            }

        }

        
    }
}
