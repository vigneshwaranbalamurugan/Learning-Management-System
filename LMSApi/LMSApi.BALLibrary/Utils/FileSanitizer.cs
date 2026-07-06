using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace LMSApi.BALLibrary.Utils
{
    public static class FileSanitizer
    {
        /// <summary>
        /// Removes all EXIF, ICC, and XMP metadata from the image stream.
        /// Returns a new MemoryStream containing the sanitized image.
        /// If the image format is not recognized or an error occurs, it returns the original stream copied.
        /// </summary>
        public static async Task<MemoryStream> SanitizeImageAsync(Stream inputStream)
        {
            var outputStream = new MemoryStream();

            try
            {
                inputStream.Position = 0;
                
                // ImageSharp automatically parses metadata
                using var image = await Image.LoadAsync(inputStream);
                
                // Strip metadata
                image.Metadata.ExifProfile = null;
                image.Metadata.IccProfile = null;
                image.Metadata.XmpProfile = null;
                
                // Save the cleaned image to the output stream
                // It will preserve the original format if we use the format from LoadAsync, 
                // but ImageSharp doesn't expose the original format directly on the image object without returning it in LoadAsync.
                // We'll use the overloaded LoadAsync that returns the format.
                inputStream.Position = 0;
                var (img, format) = await Image.LoadWithFormatAsync(inputStream);
                using (img)
                {
                    img.Metadata.ExifProfile = null;
                    img.Metadata.IccProfile = null;
                    img.Metadata.XmpProfile = null;
                    await img.SaveAsync(outputStream, format);
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException("The uploaded image could not be processed for privacy sanitization. Please ensure it is a valid image format (JPG, PNG) and try again.");
            }

            outputStream.Position = 0;
            return outputStream;
        }

        /// <summary>
        /// Removes Document Information (Title, Author, Subject, etc.) from a PDF.
        /// </summary>
        public static async Task<MemoryStream> SanitizePdfAsync(Stream inputStream)
        {
            var outputStream = new MemoryStream();
            
            try
            {
                inputStream.Position = 0;
                
                // Load PDF Document
                using var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);
                
                // Clear Metadata
                document.Info.Author = string.Empty;
                document.Info.Title = string.Empty;
                document.Info.Subject = string.Empty;
                document.Info.Keywords = string.Empty;
                document.Info.Creator = string.Empty;
                
                // Save sanitized PDF
                document.Save(outputStream, false);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("The uploaded PDF could not be processed for privacy sanitization. Please ensure it is not password-protected or corrupted and try again.");
            }

            outputStream.Position = 0;
            return outputStream;
        }
    }
}
