using System;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;

namespace LMSApi.BALLibrary.Services
{
    public class OwnershipService : IOwnershipService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResourceRepository _resourceRepository;
        private readonly ICourseBatchRepository _batchRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IAssignmentRepository _assignmentRepository;

        public OwnershipService(
            ICourseRepository courseRepository,
            ICourseSectionRepository sectionRepository,
            ILessonRepository lessonRepository,
            ILessonResourceRepository resourceRepository,
            ICourseBatchRepository batchRepository,
            IQuizRepository quizRepository,
            IAssignmentRepository assignmentRepository)
        {
            _courseRepository = courseRepository;
            _sectionRepository = sectionRepository;
            _lessonRepository = lessonRepository;
            _resourceRepository = resourceRepository;
            _batchRepository = batchRepository;
            _quizRepository = quizRepository;
            _assignmentRepository = assignmentRepository;
        }

        public async Task EnforceCourseOwnershipAsync(int courseId, int userId, bool isAdmin, string message)
        {
            if (isAdmin) return;

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course.InstructorId != userId)
            {
                throw new UnauthorizedAccessException(message);
            }
        }

        public async Task EnforceSectionOwnershipAsync(int sectionId, int userId, bool isAdmin, string message)
        {
            if (isAdmin) return;

            var section = await _sectionRepository.GetByIdAsync(sectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.InstructorId != userId)
            {
                throw new UnauthorizedAccessException(message);
            }
        }

        public async Task EnforceLessonOwnershipAsync(int lessonId, int userId, bool isAdmin, string message)
        {
            if (isAdmin) return;

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.InstructorId != userId)
            {
                throw new UnauthorizedAccessException(message);
            }
        }

        public async Task EnforceResourceOwnershipAsync(int resourceId, int userId, bool isAdmin, string message)
        {
            if (isAdmin) return;

            var resource = await _resourceRepository.GetByIdAsync(resourceId);
            var lesson = await _lessonRepository.GetByIdAsync(resource.LessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.InstructorId != userId)
            {
                throw new UnauthorizedAccessException(message);
            }
        }

        public async Task EnforceBatchOwnershipAsync(int batchId, int userId, bool isAdmin, string message)
        {
            if (isAdmin) return;

            var batch = await _batchRepository.GetByIdAsync(batchId);
            var course = await _courseRepository.GetByIdAsync(batch.CourseId);
            if (course.InstructorId != userId)
            {
                throw new UnauthorizedAccessException(message);
            }
        }

        public async Task EnforceQuizOwnershipAsync(int quizId, int userId, bool isAdmin, string message)
        {
            if (isAdmin) return;

            var quiz = await _quizRepository.GetByIdAsync(quizId);
            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.InstructorId != userId)
            {
                throw new UnauthorizedAccessException(message);
            }
        }

        public async Task EnforceAssignmentOwnershipAsync(int assignmentId, int userId, bool isAdmin, string message)
        {
            if (isAdmin) return;

            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.InstructorId != userId)
            {
                throw new UnauthorizedAccessException(message);
            }
        }
    }
}
