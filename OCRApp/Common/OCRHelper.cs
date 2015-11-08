using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Media.Ocr;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using System.Collections.ObjectModel;

namespace OCRApp.Common
{
    public class OCRHelper
    {
        public  const string Warning = "NOTPOSSIBLE";

        public static async Task<String> ScanImageOCR(ObservableCollection<List<WriteableBitmap>> observbitmap,bool isProcessPages,int SelectedIndex, String selectedLanguage)
        {

            string testdata = string.Empty;
            string extractedText = "";
            OcrLanguage selectedLanguageValue;
            switch(selectedLanguage)
            {
                case "English": selectedLanguageValue = OcrLanguage.English;
                    break;
                case "ChineseSimplified": selectedLanguageValue = OcrLanguage.ChineseSimplified;
                    break;
                case "ChineseTraditional": selectedLanguageValue = OcrLanguage.ChineseTraditional;
                    break;
                case "Dutch": selectedLanguageValue = OcrLanguage.Dutch;
                    break;
                case "Danish": selectedLanguageValue = OcrLanguage.Danish;
                    break;
                case "Finnish": selectedLanguageValue = OcrLanguage.Finnish;
                    break;
                case "German": selectedLanguageValue = OcrLanguage.German;
                    break;
                case "Greek": selectedLanguageValue = OcrLanguage.Greek;
                    break;
                case "Hungarian": selectedLanguageValue = OcrLanguage.Hungarian;
                    break;
                case "Italian": selectedLanguageValue = OcrLanguage.Italian;
                    break;
                case "Japanese": selectedLanguageValue = OcrLanguage.Japanese;
                    break;
                case "Korean": selectedLanguageValue = OcrLanguage.Korean;
                    break;
                case "Norwegian": selectedLanguageValue = OcrLanguage.Norwegian;
                    break;
                case "Polish": selectedLanguageValue = OcrLanguage.Polish;
                    break;
                case "Portugese": selectedLanguageValue = OcrLanguage.Portuguese;
                    break;
                case "Russian": selectedLanguageValue = OcrLanguage.Russian;
                    break;
                case "Spanish": selectedLanguageValue = OcrLanguage.Spanish;
                    break;
                case "Swedish": selectedLanguageValue = OcrLanguage.Swedish;
                    break;
                case "Turkis": selectedLanguageValue = OcrLanguage.Turkish;
                    break;

                default: selectedLanguageValue = OcrLanguage.English;
                    break;
            }
            OcrEngine ocrEngine = new OcrEngine(selectedLanguageValue);

            //if (!isProcessPages)
            //{
            //    observbitmap = observbitmap[0]
            //}


            if(isProcessPages)
            { 
                for (int i = 0; i < observbitmap.Count; i++)
                {
                    List<WriteableBitmap> lstbitmap = observbitmap[i];
                    string OCRString = await GetString(lstbitmap, ocrEngine);
                    if (i > 0)
                    {
                        extractedText += Environment.NewLine;
                        extractedText += " === Page " + (i + 1) + " OCR Scanning Started === ";
                        extractedText += Environment.NewLine;
                        extractedText += Environment.NewLine;
                        extractedText += OCRString;
                    }
                    else
                    {
                        extractedText += OCRString;
                    }

                    Debug.WriteLine(String.Format("Image successfully processed in {0} language.", ocrEngine.Language.ToString()));
                }
            }
            else
            {
                string OCRString = await GetString(observbitmap[SelectedIndex], ocrEngine);
                extractedText += OCRString;
            }
            return extractedText;
            
        }

        private static async Task<string> GetString(List<WriteableBitmap> lstbitmap,OcrEngine ocrEngine)
        {
            string extractedText = "";
            foreach (WriteableBitmap bitmap in lstbitmap)
                {
                    if (bitmap.PixelHeight < 40 ||
                        bitmap.PixelHeight > 2600 ||
                        bitmap.PixelWidth < 40 ||
                        bitmap.PixelWidth > 2600)
                    {
                        extractedText = Environment.NewLine + "Image size is not supported." +
                                    Environment.NewLine +
                                    "Loaded image size is " + bitmap.PixelWidth + "x" + bitmap.PixelHeight + "." +
                                    Environment.NewLine +
                                    "Supported image dimensions are between 40 and 2600 pixels.";
                        //ImageText.Style = (Style)Application.Current.Resources["RedTextStyle"];
                        return extractedText;
                    }


                    try
                    {
                        // This main API call to extract text from image.
                        var ocrResult = await ocrEngine.RecognizeAsync((uint)bitmap.PixelHeight, (uint)bitmap.PixelWidth, bitmap.PixelBuffer.ToArray());

                        //OCR result does not contain any lines, no text was recognized. 
                        if (ocrResult.Lines != null)
                        {
                            // Iterate over recognized lines of text.
                            foreach (var line in ocrResult.Lines)
                            {
                                // Iterate over words in line.
                                foreach (var word in line.Words)
                                {
                                    var originalRect = new Rect(word.Left, word.Top, word.Width, word.Height);
                                    if (ocrEngine.Language != OcrLanguage.ChineseSimplified)
                                        extractedText += word.Text + " ";
                                    else
                                        extractedText += word.Text;
                                }
                                extractedText += Environment.NewLine;
                            }


                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {

                    }


                }

            return extractedText;
        }
    }
}
