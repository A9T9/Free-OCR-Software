using OCRApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace OCRApp.Common
{
    public class FilePickerHelper
    {

        public enum RENDEROPTIONS
        {
            NORMAL,
            ZOOM,
            PORTION
        }

        static uint PDF_PAGE_INDEX = 0; //first page
        static uint ZOOM_FACTOR = 1; //300% zoom
        static Rect PDF_PORTION_RECT = new Rect(100, 100, 300, 400); //portion of a page

        public static async Task<Images> GetFileListWithBitmapImages(List<string> fileTypes, bool loadFilesFromLocalFolder = false, List<string> fileListToLoad = null)
        {
            Images imagecollection = new Images();
            //ObservableCollection<List<ImageSource>> ListImageDisplay = new ObservableCollection<List<ImageSource>>();
            
            IReadOnlyList<StorageFile> files = null;
            if (!loadFilesFromLocalFolder) //Open file picker.
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                foreach (string fileType in fileTypes)
                {
                    openPicker.FileTypeFilter.Add(fileType);
                }
                files = await openPicker.PickMultipleFilesAsync();
            }
            else     //Load files from local folder
            {
                if (fileListToLoad != null)
                {
                    foreach (string filePath in fileListToLoad)
                    {
                        var uri = new System.Uri(filePath);// ("ms-appx:///images/logo.png");
                        var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                        List<StorageFile> filesLocal = new List<StorageFile>();
                        filesLocal.Add(file);
                        files = filesLocal;
                    }
                }
            }
            if (files != null && files.Count > 0)
            {
                StringBuilder output = new StringBuilder("Picked files:\n");
                // Application now has read/write access to the picked file(s)
                foreach (StorageFile file in files)
                {
                   

                    output.Append(file.Name + "\n");
                    bool fastThumbnail = false;
                    ThumbnailOptions thumbnailOptions = ThumbnailOptions.ResizeThumbnail;
                    if (fastThumbnail)
                    {
                        thumbnailOptions |= ThumbnailOptions.ReturnOnlyIfCached;
                    }

                    const uint size = 500;
                    using (StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, size))
                    {
                        if (thumbnail != null)
                        {
                            List<ImageSource> bitmapImageList = null;

                            ImageProperties imgProp = await file.Properties.GetImagePropertiesAsync();

                            if (file.FileType.ToLower() == ".pdf")
                            {
                                DocumentProperties docProp = await file.Properties.GetDocumentPropertiesAsync();

                                Images image = await LoadPDFBitmapImage(file, RENDEROPTIONS.NORMAL);

                                imagecollection.ListImageDisplay.Add(image.ListImageDisplay[0]);
                                imagecollection.ListWriteImages.Add(image.ListWriteImages[0]);
                                //image.ListWriteImages.Add()

                               
                            }
                            else
                            {
                                bitmapImageList = new List<ImageSource>();
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.SetSource(thumbnail);
                                bitmapImageList.Add(bitmapImage);

                                using (var imgStream = await file.OpenAsync(FileAccessMode.Read))
                                {
                                    List<WriteableBitmap> lstwriteable = new List<WriteableBitmap>();
                                    WriteableBitmap bitmap = new WriteableBitmap((int)imgProp.Width, (int)imgProp.Height);
                                    bitmap.SetSource(imgStream);
                                    lstwriteable.Add(bitmap);
                                    imagecollection.ListWriteImages.Add(lstwriteable);
                                }

                                imagecollection.ListImageDisplay.Add(bitmapImageList);
                            }

                           
                            //ListImageDisplay.Add(bitmapImageList);

                            

                            
                        }
                    }
                }

            }
            else
            {

            }
            return imagecollection;
        }


        /// <summary>
        /// This renders PDF bitmap Image with render options 
        /// Rendering a pdf page requires following 3 steps
        ///     1. PdfDocument.LoadFromFileAsync(pdfFile) which returns pdfDocument
        ///     2. pdfDocument.GetPage(pageIndex) 
        ///     3. pdfPage.RenderToStreamAsync(stream) or pdfPage.RenderToStreamAsync(stream,pdfPageRenderOptions)
        /// </summary>
        public static async Task<Images> LoadPDFBitmapImage(StorageFile pdfFile, RENDEROPTIONS renderOptions)
        {
            Images image = new Images();
            
            List<ImageSource> bitmapImageListForAllPages = new List<ImageSource>();
            List<WriteableBitmap> lstwriteable = new List<WriteableBitmap>();
           
            try
            {
                //Load Pdf File
                PdfDocument _pdfDocument = await PdfDocument.LoadFromFileAsync(pdfFile); ;

                if (_pdfDocument != null && _pdfDocument.PageCount > 0)
                {
                    for (uint i = 0; i < _pdfDocument.PageCount; i++)
                    { 
                        //Get Pdf page
                        var pdfPage = _pdfDocument.GetPage(i);

                       

                        if (pdfPage != null)
                        {
                            // next, generate a bitmap of the page
                            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;

                            StorageFile jpgFile = await tempFolder.CreateFileAsync(Guid.NewGuid().ToString() + ".png", CreationCollisionOption.ReplaceExisting);

                            if (jpgFile != null)
                            {
                                IRandomAccessStream randomStream = await jpgFile.OpenAsync(FileAccessMode.ReadWrite);

                                PdfPageRenderOptions pdfPageRenderOptions = new PdfPageRenderOptions();
                                switch (renderOptions)
                                {
                                    case RENDEROPTIONS.NORMAL:
                                        //Render Pdf page with default options
                                        await pdfPage.RenderToStreamAsync(randomStream);
                                        break;
                                    case RENDEROPTIONS.ZOOM:
                                        //set PDFPageRenderOptions.DestinationWidth or DestinationHeight with expected zoom value
                                        Size pdfPageSize = pdfPage.Size;
                                        pdfPageRenderOptions.DestinationHeight = (uint)pdfPageSize.Height * ZOOM_FACTOR;
                                        //Render pdf page at a zoom level by passing pdfpageRenderOptions with DestinationLength set to the zoomed in length 
                                        await pdfPage.RenderToStreamAsync(randomStream, pdfPageRenderOptions);
                                        break;
                                    case RENDEROPTIONS.PORTION:
                                        //Set PDFPageRenderOptions.SourceRect to render portion of a page
                                        pdfPageRenderOptions.SourceRect = PDF_PORTION_RECT;
                                        //Render portion of a page
                                        await pdfPage.RenderToStreamAsync(randomStream, pdfPageRenderOptions);
                                        break;
                                }

                                Size size = pdfPage.Size;
                                WriteableBitmap writebitmap = new WriteableBitmap((int)size.Width, (int)size.Height);
                                writebitmap.SetSource(randomStream);
                                lstwriteable.Add(writebitmap);

                             
                                //return src;

                                await randomStream.FlushAsync();

                                randomStream.Dispose();
                                pdfPage.Dispose();

                                // Display the image in the UI.
                                BitmapImage src = new BitmapImage();
                                //src.SetSource(randomStream);
                                src.SetSource(await jpgFile.OpenAsync(FileAccessMode.Read));
                                //jpgFile = await jpgFile.GetScaledImageAsThumbnailAsync(ThumbnailMode.DocumentsView, ZOOM_FACTOR, ThumbnailOptions.ResizeThumbnail);
                                bitmapImageListForAllPages.Add(src);

                               

                                
                            }
                        }
                    
                    }
                }
            }
            catch (Exception err)
            {

            }


            image.ListImageDisplay.Add(bitmapImageListForAllPages);
            image.ListWriteImages.Add(lstwriteable);
            return image;
        }
    }
}
