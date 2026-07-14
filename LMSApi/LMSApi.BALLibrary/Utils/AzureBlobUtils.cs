using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Utils
{
    public static class AzureBlobUtils
    {
        private static BlobServiceClient CreateClient(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlob:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("AzureBlob configuration is missing. Set AzureBlob:ConnectionString.");
            }

            return new BlobServiceClient(connectionString);
        }

        private static async Task<BlobContainerClient> GetContainerClientAsync(IConfiguration configuration, bool isPublic)
        {
            var blobServiceClient = CreateClient(configuration);
            var containerName = isPublic
                ? (configuration["AzureBlob:PublicContainerName"] ?? "lms-public")
                : (configuration["AzureBlob:ContainerName"] ?? "lms-media");

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            // Always use PublicAccessType.None — "Allow Blob anonymous access" is disabled
            // on modern Azure Storage Accounts. Serve all blobs via SAS URLs instead.
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            return containerClient;
        }

        private static string GetFolderConfig(IConfiguration configuration, string folderKey, string defaultFolder)
        {
            return configuration[$"Cloudinary:{folderKey}"] ?? defaultFolder;
        }

        private static double GetVideoDuration(Stream stream, string fileName)
        {
            try
            {
                // Write stream to a temp file with the correct extension because TagLibSharp needs it to determine format
                var extension = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension)) extension = ".mp4"; // fallback
                var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + extension);
                using (var fileStream = File.Create(tempFile))
                {
                    stream.Position = 0;
                    stream.CopyTo(fileStream);
                }

                try
                {
                    using (var tagFile = TagLib.File.Create(tempFile))
                    {
                        return tagFile.Properties.Duration.TotalSeconds;
                    }
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }
            catch
            {
                return 0;
            }
        }

        public static async Task<string> UploadProfileImageAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "Folder", "lms/profile-pictures");
            var blobPath = $"{folder}/{publicId}{Path.GetExtension(fileName)}";

            using var sanitizedStream = await FileSanitizer.SanitizeImageAsync(fileStream);

            var containerClient = await GetContainerClientAsync(configuration, true);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(sanitizedStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) }
            });

            // Return blob path; serve via GeneratePublicSasUrl when displaying to users.
            return blobPath;
        }

        public static async Task<string> UploadCourseThumbnailAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "CourseThumbnailFolder", "lms/course-thumbnails");
            var blobPath = $"{folder}/{publicId}{Path.GetExtension(fileName)}";

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            using var sanitizedStream = await FileSanitizer.SanitizeImageAsync(memoryStream);

            var containerClient = await GetContainerClientAsync(configuration, true);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(sanitizedStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) }
            });

            // Return blob path; serve via GeneratePublicSasUrl when displaying to users.
            return blobPath;
        }

        public static async Task<(string Url, double DurationSeconds)> UploadCourseIntroVideoAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "CourseVideoFolder", "lms/course-videos");
            var blobPath = $"{folder}/{publicId}{Path.GetExtension(fileName)}";

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var duration = GetVideoDuration(memoryStream, fileName);
            memoryStream.Position = 0;

            var containerClient = await GetContainerClientAsync(configuration, false);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(memoryStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) }
            });

            return (blobPath, duration);
        }

        public static async Task<(string Url, double DurationSeconds)> UploadLessonVideoAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "LessonVideoFolder", "lms/lesson-videos");
            var blobPath = $"{folder}/{publicId}{Path.GetExtension(fileName)}";

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var duration = GetVideoDuration(memoryStream, fileName);
            memoryStream.Position = 0;

            var containerClient = await GetContainerClientAsync(configuration, false);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(memoryStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) }
            });

            return (blobPath, duration);
        }

        public static async Task<string> UploadLessonPdfAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "LessonPdfFolder", "lms/lesson-pdfs");
            var extension = Path.GetExtension(fileName);
            var blobPath = $"{folder}/{publicId}{(string.IsNullOrWhiteSpace(extension) ? "" : extension)}";

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            using var sanitizedStream = await FileSanitizer.SanitizePdfAsync(memoryStream);

            var containerClient = await GetContainerClientAsync(configuration, false);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(sanitizedStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" }
            });

            return blobPath;
        }

        public static async Task<string> UploadAssignmentAttachmentAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "AssignmentAttachmentFolder", "lms/assignment-attachments");
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";
            var blobPath = $"{folder}/{publicId}{(string.IsNullOrWhiteSpace(extension) ? "" : extension)}";

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            MemoryStream sanitizedStream;
            if (extension == ".pdf")
            {
                sanitizedStream = await FileSanitizer.SanitizePdfAsync(memoryStream);
            }
            else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp")
            {
                sanitizedStream = await FileSanitizer.SanitizeImageAsync(memoryStream);
            }
            else
            {
                sanitizedStream = new MemoryStream(memoryStream.ToArray());
            }

            var containerClient = await GetContainerClientAsync(configuration, false);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(sanitizedStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) }
            });

            return blobPath;
        }

        public static async Task<string> UploadCertificateTemplateBackgroundAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "CertificateTemplateFolder", "lms/certificate-templates");
            var blobPath = $"{folder}/{publicId}{Path.GetExtension(fileName)}";

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            using var sanitizedStream = await FileSanitizer.SanitizeImageAsync(memoryStream);

            var containerClient = await GetContainerClientAsync(configuration, true);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(sanitizedStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) }
            });

            // Return blob path; serve via GeneratePublicSasUrl when displaying to users.
            return blobPath;
        }

        public static async Task<string> UploadCertificatePdfAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
        {
            var folder = GetFolderConfig(configuration, "CertificateFolder", "lms/certificates");
            var blobPath = $"{folder}/{publicId}{Path.GetExtension(fileName)}";

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var containerClient = await GetContainerClientAsync(configuration, false);
            var blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(memoryStream, new BlobUploadOptions {
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" }
            });

            return blobPath;
        }

        public static async Task DeleteBlobAsync(IConfiguration configuration, string blobPath, bool isPublic)
        {
            // Backward compatibility: If it's a cloudinary URL, don't delete from Azure
            if (blobPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;

            try
            {
                var containerClient = await GetContainerClientAsync(configuration, isPublic);
                var blobClient = containerClient.GetBlobClient(blobPath);
                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Ignore delete errors
            }
        }

        public static string GenerateSasUrl(IConfiguration configuration, string blobPath, int expiryMinutes)
        {
            // Backward compatibility: pass-through for pre-existing absolute URLs
            if (blobPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return blobPath;

            var containerName = configuration["AzureBlob:ContainerName"] ?? "lms-media";
            return GenerateSasUrlInternal(configuration, containerName, blobPath, expiryMinutes);
        }

        /// <summary>
        /// Generates a time-limited SAS URL for blobs stored in the public container
        /// (profile images, course thumbnails, certificate template backgrounds).
        /// Use this instead of the raw blob URI since anonymous public access is disabled.
        /// </summary>
        public static string GeneratePublicSasUrl(IConfiguration configuration, string blobPath, int expiryMinutes = 60)
        {
            // Backward compatibility: pass-through for pre-existing absolute URLs
            if (blobPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return blobPath;

            var containerName = configuration["AzureBlob:PublicContainerName"] ?? "lms-public";
            return GenerateSasUrlInternal(configuration, containerName, blobPath, expiryMinutes);
        }

        private static string GenerateSasUrlInternal(IConfiguration configuration, string containerName, string blobPath, int expiryMinutes)
        {
            var blobServiceClient = CreateClient(configuration);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("BlobClient cannot generate SasUri. Ensure the connection string contains an AccountKey.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".zip" => "application/zip",
                ".txt" => "text/plain",
                _ => "application/octet-stream",
            };
        }
    }
}
