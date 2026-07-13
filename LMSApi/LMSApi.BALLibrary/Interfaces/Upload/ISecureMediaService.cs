using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ISecureMediaService
    {
        /// <summary>
        /// Validates user enrollment and returns a short-lived SAS URL for a private blob.
        /// Returns null for public blobs (profile images, thumbnails).
        /// </summary>
        Task<string> GetSecureUrlAsync(string blobPath, int? userId, int courseId);
    }
}
