using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;

namespace OCRApp.ViewModel
{
    public class TextRecognitionViewModel : BaseViewModel
    {

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
                if(value != _scannedText)
                {
                    _scannedText = value;
                    NotifyPropertyChanged("ScannedText");
                }
            }
        }

        #region Constructor

        public TextRecognitionViewModel()
        {
            //Dummy text to save. 
            ScannedText = "Sample text to save. Actual scanned text will be shown here";
        }

        #endregion

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
            switch(fileType)
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
                await FileIO.WriteTextAsync(file, ScannedText);
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
        

    }
}
