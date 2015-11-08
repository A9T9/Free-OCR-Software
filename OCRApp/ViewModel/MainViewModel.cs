using OCRApp.Common;
using OCRApp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using Windows.Storage.Provider;
using Windows.UI;

namespace OCRApp.ViewModel
{
    public class MainViewModel : BaseViewModel
    {

        private bool _isProcessPages = false;
        public bool ProcessPages
        {
            get
            {
                return _isProcessPages;
            }
            set
            {
                if (value != null)
                {
                    _isProcessPages = value;
                    NotifyPropertyChanged("ProcessPages");
                }
            }
        }

        private Visibility _ProgressVisibility = Visibility.Collapsed;
        public Visibility ProgressVisibility
        {
            get
            {
                return _ProgressVisibility;
            }
            set
            {
                if (value != null)
                {
                    _ProgressVisibility = value;
                    NotifyPropertyChanged("ProgressVisibility");
                }
            }
        }

        private Visibility _ProgressOCRVisibility = Visibility.Collapsed;
        public Visibility ProgressOCRVisibility
        {
            get
            {
                return _ProgressOCRVisibility;
            }
            set
            {
                if (value != null)
                {
                    _ProgressOCRVisibility = value;
                    NotifyPropertyChanged("ProgressOCRVisibility");
                }
            }
        }

        private String _selectedLanguage;
        public String SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                if (value != null)
                {
                    _selectedLanguage = value;
                    NotifyPropertyChanged("SelectedLanguage");
                }
            }

        }

        public ObservableCollection<String> SelectedLangues
        {
            get;
            set;
        }

        private bool _isOpen = true;
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                if (value != null)
                {
                    _isOpen = value;
                    NotifyPropertyChanged("IsOpen");
                }
            }
        }

        private int _SelectedIndex = -1;
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                if (value != null)
                {
                    _SelectedIndex = value;
                    NotifyPropertyChanged("SelectedIndex");
                }
            }
        }

        private String _OcrString;
        public String OcrString
        {
            get
            {
                return _OcrString;
            }
            set
            {
                if (value != null)
                {
                    _OcrString = value;
                    NotifyPropertyChanged("OcrString");
                }
            }

        }


        private SolidColorBrush _textblockForeground = new SolidColorBrush(Colors.Black);
        public SolidColorBrush TextblockForeground
        {
            get
            {
                return _textblockForeground;
            }
            set
            {
                if (value != null)
                {
                    _textblockForeground = value;
                    NotifyPropertyChanged("TextblockForeground");
                }
            }

        }


        private ImageSource _ImageDisplay;
        public ImageSource ImageDisplay
        {
            get 
            {
                return _ImageDisplay;
            }
            set 
            {
                if (value != null)
                {
                    _ImageDisplay = value;
                    NotifyPropertyChanged("ImageDisplay");
                }
            }

        }

        private Images _ImageCollection;
        public Images ImageCollection
        {
            get
            {
                return _ImageCollection;
            }
            set
            {
                if (value != null)
                {
                    _ImageCollection = value;
                    NotifyPropertyChanged("ImageCollection");
                }
            }

        }

        //private ObservableCollection<List<ImageSource>> _lstImageDisplay;
        //public ObservableCollection<List<ImageSource>> ListImageDisplay
        //{
        //    get
        //    {
        //        return _lstImageDisplay;
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            _lstImageDisplay = value;
        //            NotifyPropertyChanged("ListImageDisplay");
        //        }
        //    }

        //}

        #region TextRecognistion
        public enum FILETYPE
        {
            TEXTFILE,
            WORDDOCUMENT
        }


        /// <summary>
        /// Text scanned using OCR
        /// </summary>
        private string _scannedText;
        public string ScannedText
        {
            get
            {
                return _scannedText;
            }
            set
            {
                if (value != _scannedText)
                {
                    _scannedText = value;
                    NotifyPropertyChanged("ScannedText");
                }
            }
        }

        #region Commands
        BaseCommand _SaveAsTextFileCommand;
        public BaseCommand SaveAsTextFileCommand
        {
            get
            {
                if (_SaveAsTextFileCommand == null)
                    _SaveAsTextFileCommand = new BaseCommand(ExecuteSaveAsTextFileCommand);
                return _SaveAsTextFileCommand;
            }
        }

        BaseCommand _SaveAsWordDocFileCommand;
        public BaseCommand SaveAsWordDocFileCommand
        {
            get
            {
                if (_SaveAsWordDocFileCommand == null)
                    _SaveAsWordDocFileCommand = new BaseCommand(ExecuteSaveAsWordDocFileCommand);
                return _SaveAsWordDocFileCommand;
            }
        }


        #endregion

        private async void ExecuteSaveAsTextFileCommand(object obj)
        {
            OpenFileSavePicker(FILETYPE.TEXTFILE);
        }

        private async void ExecuteSaveAsWordDocFileCommand(object obj)
        {
            OpenFileSavePicker(FILETYPE.WORDDOCUMENT);
        }

        private async void OpenFileSavePicker(FILETYPE fileType)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // Dropdown of file types the user can save the file as
            switch (fileType)
            {
                case FILETYPE.TEXTFILE: savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                    break;
                case FILETYPE.WORDDOCUMENT: savePicker.FileTypeChoices.Add("Word Document", new List<string>() { ".doc" });
                    break;
            }

            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                await FileIO.WriteTextAsync(file, OcrString);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                MessageDialog msgDialog;
                if (status == FileUpdateStatus.Complete)
                {
                    msgDialog = new MessageDialog("File " + file.Name + " was saved.", "File Saved.");
                }
                else
                {
                    msgDialog = new MessageDialog("File " + file.Name + " couldn't be saved.", "File Saved.");
                }
                await msgDialog.ShowAsync();
            }
            else
            {
                //"Operation cancelled.";
            }
        }

        #endregion

        #region Commands
        BaseCommand _OpenImageCommand;
        public BaseCommand OpenImageCommand
        {
            get
            {
                if (_OpenImageCommand == null)
                    _OpenImageCommand = new BaseCommand(ExecuteImageCommand);
                return _OpenImageCommand;
            }
        }

        BaseCommand _OpenPdfCommand;
        public BaseCommand OpenPdfCommand
        {
            get
            {
                if (_OpenPdfCommand == null)
                    _OpenPdfCommand = new BaseCommand(ExecutePdfCommand);
                return _OpenPdfCommand;
            }
        }

        BaseCommand _BeginOcrCommand;
        public BaseCommand BeginOcrCommand
        {
            get
            {
                if (_BeginOcrCommand == null)
                    _BeginOcrCommand = new BaseCommand(ExecuteBeginOcrCommand);
                return _BeginOcrCommand;
            }
        }

        BaseCommand _openHelpCommand;
        public BaseCommand OpenHelpCommand
        {
            get
            {
                if (_openHelpCommand == null)
                    _openHelpCommand = new BaseCommand(ExecuteOpenHelpCommand);
                return _openHelpCommand;
            }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            SelectedLangues = new ObservableCollection<string>();
            SelectedLangues.Add("ChineseSimplified");
            SelectedLangues.Add("ChineseTraditional");
            SelectedLangues.Add("Dutch");
            SelectedLangues.Add("Danish");
            SelectedLangues.Add("English");
            SelectedLangues.Add("Finnish");
            SelectedLangues.Add("French");
            SelectedLangues.Add("German");
            SelectedLangues.Add("Greek");
            SelectedLangues.Add("Hungarian");
            SelectedLangues.Add("Italian");
            SelectedLangues.Add("Japanese");
            SelectedLangues.Add("Korean");
            SelectedLangues.Add("Norwegian");
            SelectedLangues.Add("Polish");
            SelectedLangues.Add("Portugese");
            SelectedLangues.Add("Russian");
            SelectedLangues.Add("Spanish");
            SelectedLangues.Add("Swedish");

            //Dummy text to save. 
            OcrString = "--- Scanned text will be shown here ---";
            SelectedLanguage = "English";
            LoadDefaultImageForOcr();
        }

        #endregion

        #region Methods

        private async void ExecuteBeginOcrCommand(object obj)
        {
            if(SelectedIndex >= 0)
            {
                ProgressOCRVisibility = Visibility.Visible;
                OcrString = string.Empty;
                OcrString = await OCRHelper.ScanImageOCR(ImageCollection.ListWriteImages,ProcessPages,SelectedIndex, SelectedLanguage);
                if (OcrString.Contains("Supported image dimensions are between 40 and 2600 pixels."))
                {
                    TextblockForeground.Color = Colors.Red;
                }
                else
                {
                    TextblockForeground.Color = Colors.Black;
                }
               
                ProgressOCRVisibility = Visibility.Collapsed;
            }
        }

        private async void ExecutePdfCommand(object obj)
        {
            ProgressVisibility = Visibility.Visible;
            ImageCollection = new Images();

            List<string> fileTypes = new List<string>();
            fileTypes.Add(".pdf");
            ImageCollection = await FilePickerHelper.GetFileListWithBitmapImages(fileTypes);
            if (ImageCollection.ListImageDisplay.Count > 0 && ImageCollection.ListImageDisplay[0].Count > 0)
            {
                ImageDisplay = ImageCollection.ListImageDisplay[0].FirstOrDefault();//List<ImageSource>
                IsOpen = true;
            }
            else
            {

            }
            ProgressVisibility = Visibility.Collapsed;
        }

        private async void ExecuteImageCommand(object obj)
        {
            ProgressVisibility = Visibility.Visible;
            ImageCollection = new Images();
            List<string> fileTypes = new List<string>();
            fileTypes.Add(".jpg");
            fileTypes.Add(".jpeg");
            fileTypes.Add(".png");
            ImageCollection = await FilePickerHelper.GetFileListWithBitmapImages(fileTypes);
            if (ImageCollection.ListImageDisplay.Count > 0 && ImageCollection.ListImageDisplay[0].Count > 0)
            {
                ImageDisplay = ImageCollection.ListImageDisplay[0].FirstOrDefault();
                IsOpen = true;
            }
            else
            {

            }
            ProgressVisibility = Visibility.Collapsed;
           
        }

        //Load default Image
        private async void LoadDefaultImageForOcr()
        {
            ProgressVisibility = Visibility.Visible;
            ImageCollection = new Images();
            ImageCollection = await FilePickerHelper.GetFileListWithBitmapImages(null, true, new List<string>() { "ms-appx:///Assets/introtext.jpg" });
            if (ImageCollection.ListImageDisplay.Count > 0 && ImageCollection.ListImageDisplay[0].Count > 0)
            {
                ImageDisplay = ImageCollection.ListImageDisplay[0].FirstOrDefault();
                IsOpen = true;
            }
            else
            {

            }
            ProgressVisibility = Visibility.Collapsed;

        }

        private async void ExecuteOpenHelpCommand(object obj)
        {
            var content = Window.Current.Content;
            var frame = content as Frame;

            if (frame != null)
            {
                frame.Navigate(typeof(HelpPage));
            }
            Window.Current.Activate();
        }


        #endregion

    }
}
