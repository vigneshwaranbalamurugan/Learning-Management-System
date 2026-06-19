using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IOwnershipService
    {
        Task EnforceCourseOwnershipAsync(int courseId, int userId, bool isAdmin, string message = "You do not have permission to modify this course.");
        Task EnforceSectionOwnershipAsync(int sectionId, int userId, bool isAdmin, string message = "You do not have permission to modify this section.");
        Task EnforceLessonOwnershipAsync(int lessonId, int userId, bool isAdmin, string message = "You do not have permission to modify this lesson.");
        Task EnforceResourceOwnershipAsync(int resourceId, int userId, bool isAdmin, string message = "You do not have permission to modify this resource.");
        Task EnforceBatchOwnershipAsync(int batchId, int userId, bool isAdmin, string message = "You do not have permission to modify this batch.");
        Task EnforceQuizOwnershipAsync(int quizId, int userId, bool isAdmin, string message = "You do not have permission to modify this quiz.");
        Task EnforceAssignmentOwnershipAsync(int assignmentId, int userId, bool isAdmin, string message = "You do not have permission to modify this assignment.");
    }
}
