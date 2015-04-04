using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRApp.Common
{
    internal class FileExtensions
    {
        public static readonly string[] Document = new string[] { ".doc", ".xls", ".ppt", ".docx", ".xlsx", ".pptx", ".pdf", ".txt", ".rtf" };
        public static readonly string[] Image = new string[] { ".jpg", ".png", ".bmp", ".gif", ".tif" };
        public static readonly string[] Music = new string[] { ".mp3", ".wma", ".m4a", ".aac" };
    }
}
