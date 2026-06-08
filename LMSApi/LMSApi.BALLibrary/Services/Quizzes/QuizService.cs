using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.IO;

namespace LMSApi.BALLibrary.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizAttemptRepository _attemptRepository;
        private readonly IQuizQuestionRepository _questionRepository;
        private readonly IQuizOptionRepository _optionRepository;
        private readonly IQuizAnswerRepository _answerRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentProgressService _progressService;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizService> _logger;

        public QuizService(
            IQuizRepository quizRepository,
            IQuizAttemptRepository attemptRepository,
            IQuizQuestionRepository questionRepository,
            IQuizOptionRepository optionRepository,
            IQuizAnswerRepository answerRepository,
            ICourseSectionRepository sectionRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IStudentProgressService progressService,
            IMapper mapper,
            ILogger<QuizService> logger)
        {
            _quizRepository = quizRepository;
            _attemptRepository = attemptRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _answerRepository = answerRepository;
            _sectionRepository = sectionRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _progressService = progressService;
            _mapper = mapper;
            _logger = logger;
        }

        // ─── Quiz CRUD ──────────────────────────────────────────────────────

        public async Task<IEnumerable<QuizResponse>> GetQuizzesBySectionAsync(int sectionId)
        {
            var quizzes = await _quizRepository.GetQuizzesBySectionAsync(sectionId);
            return _mapper.Map<IEnumerable<QuizResponse>>(quizzes);
        }

        public async Task<QuizDetailResponse> GetQuizByIdAsync(int id)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id);
            return _mapper.Map<QuizDetailResponse>(quiz);
        }

        public async Task<QuizResponse> CreateQuizAsync(CreateQuizRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Quiz title cannot be null or empty.", nameof(request.Title));

            // Auto-assign Order if not provided
            if (request.Order == 0)
            {
                var existing = await _quizRepository.GetQuizzesBySectionAsync(request.CourseSectionId);
                request.Order = existing.Any() ? existing.Max(q => q.Order) + 1 : 1;
            }

            var quiz = _mapper.Map<Quzzes>(request);
            
            var section = await _sectionRepository.GetByIdAsync(request.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            
            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                quiz.IsPublished = true;
            }
            else
            {
                quiz.IsPublished = false; // defaults to unpublished
            }

            await _quizRepository.AddAsync(quiz);

            _logger.LogInformation("Quiz Created: '{Title}' for SectionId={SectionId}", request.Title, request.CourseSectionId);

            return _mapper.Map<QuizResponse>(quiz);
        }

        public async Task<QuizResponse> UpdateQuizAsync(int id, UpdateQuizRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id);

            if (request.Title != null) quiz.Title = request.Title;
            if (request.Description != null) quiz.Description = request.Description;
            if (request.TimeLimit.HasValue) quiz.TimeLimit = request.TimeLimit.Value;
            if (request.PassingMarks.HasValue) quiz.PassingMarks = request.PassingMarks.Value;
            if (request.MaxAttempts.HasValue) quiz.MaxAttempts = request.MaxAttempts.Value;
            if (request.Order.HasValue) quiz.Order = request.Order.Value;
            if (request.DeadlineInDays.HasValue) quiz.DeadlineInDays = request.DeadlineInDays.Value;
            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            
            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                quiz.IsPublished = true;
            }
            
            if (quiz.IsPublished)
            {
                ValidateQuizMarks(quiz);
            }

            await _quizRepository.UpdateAsync(quiz);

            _logger.LogInformation("Quiz Updated: Id={Id}", id);

            return _mapper.Map<QuizResponse>(quiz);
        }

        public async Task DeleteQuizAsync(int id)
        {
            await _quizRepository.DeleteAsync(id);
            _logger.LogInformation("Quiz Deleted: Id={Id}", id);
        }

        public async Task<QuizResponse> PublishQuizAsync(int quizId, PublishQuizRequest request)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);

            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            
            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                throw new InvalidOperationException("Cannot manually publish or unpublish items in a Self-Paced course. Their publish status is bound to the Course.");
            }

            if (request.Publish)
            {
                if (quiz.Questions == null || !quiz.Questions.Any())
                {
                    throw new InvalidOperationException("Quiz cannot be published without at least one question.");
                }
                ValidateQuizMarks(quiz);
            }

            quiz.IsPublished = request.Publish;
            await _quizRepository.UpdateAsync(quiz);

            _logger.LogInformation("Quiz Published status updated: Id={Id}, IsPublished={IsPublished}", quizId, quiz.IsPublished);

            return _mapper.Map<QuizResponse>(quiz);
        }

        // ─── Question CRUD ──────────────────────────────────────────────────

        public async Task<QuizQuestionResponse> AddQuestionAsync(CreateQuizQuestionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.QuestionText))
                throw new ArgumentException("Question text cannot be null or empty.", nameof(request.QuestionText));

            if (request.Mark <= 0)
                throw new ArgumentException("Mark must be greater than zero.", nameof(request.Mark));

            ValidateQuestionOptions(request.QuestionType, request.Options);

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(request.QuizId);

            // Verify SortOrder uniqueness
            if (quiz.Questions.Any(q => q.SortOrder == request.SortOrder))
            {
                throw new ArgumentException($"A question with SortOrder '{request.SortOrder}' already exists in this quiz.");
            }

            // Auto-assign SortOrder if not provided
            var sortOrder = request.SortOrder;
            if (sortOrder == 0)
            {
                sortOrder = quiz.Questions.Any() ? quiz.Questions.Max(q => q.SortOrder) + 1 : 1;
            }

            var question = _mapper.Map<QuizQuestions>(request);
            question.SortOrder = sortOrder;
            question.Answers = request.Options.Select(o => _mapper.Map<QuizOptions>(o)).ToList();

            await _questionRepository.AddAsync(question);

            _logger.LogInformation("Question Added: QuizId={QuizId}, QuestionId={QuestionId}", request.QuizId, question.Id);

            // Reload quiz with questions to check publish constraints
            var reloadedQuiz = await _quizRepository.GetQuizWithQuestionsAsync(request.QuizId);
            if (reloadedQuiz.IsPublished)
            {
                ValidateQuizMarks(reloadedQuiz);
            }

            // Reload saved question with options
            var saved = await _questionRepository.GetQuestionWithAnswersAsync(question.Id)
                ?? throw new KeyNotFoundException($"Saved question with id '{question.Id}' not found.");

            return _mapper.Map<QuizQuestionResponse>(saved);
        }

        public async Task<QuizQuestionResponse> UpdateQuestionAsync(int id, UpdateQuizQuestionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var question = await _questionRepository.GetQuestionWithAnswersAsync(id)
                ?? throw new KeyNotFoundException($"Question with id '{id}' not found.");

            if (request.Mark.HasValue && request.Mark.Value <= 0)
                throw new ArgumentException("Mark must be greater than zero.");

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(question.QuizId);

            if (request.SortOrder.HasValue && request.SortOrder.Value != question.SortOrder)
            {
                if (quiz.Questions.Any(q => q.SortOrder == request.SortOrder.Value && q.Id != id))
                {
                    throw new ArgumentException($"A question with SortOrder '{request.SortOrder.Value}' already exists in this quiz.");
                }
            }

            if (request.QuestionText != null) question.QuestionText = request.QuestionText;
            if (request.QuestionType.HasValue) question.QuestionType = request.QuestionType.Value;
            if (request.Mark.HasValue) question.Mark = request.Mark.Value;
            if (request.Explanation != null) question.Explanation = request.Explanation;
            if (request.SortOrder.HasValue) question.SortOrder = request.SortOrder.Value;

            // If options provided, replace all existing options
            if (request.Options != null && request.Options.Count > 0)
            {
                ValidateQuestionOptions(request.QuestionType ?? question.QuestionType, request.Options);
                await _optionRepository.DeleteRangeAsync(question.Answers);
                question.Answers = request.Options.Select(o => _mapper.Map<QuizOptions>(o)).ToList();
            }

            await _questionRepository.UpdateAsync(question);

            _logger.LogInformation("Question Updated: Id={Id}", id);

            // Check published constraints
            var reloadedQuiz = await _quizRepository.GetQuizWithQuestionsAsync(question.QuizId);
            if (reloadedQuiz.IsPublished)
            {
                ValidateQuizMarks(reloadedQuiz);
            }

            return _mapper.Map<QuizQuestionResponse>(question);
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            var quizId = question.QuizId;

            await _questionRepository.DeleteAsync(id);

            _logger.LogInformation("Question Deleted: Id={Id}", id);

            // Check published constraints
            var reloadedQuiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (reloadedQuiz.IsPublished)
            {
                try
                {
                    ValidateQuizMarks(reloadedQuiz);
                }
                catch (Exception)
                {
                    // If deleting the question violates PassingMarks <= TotalMarks on a published quiz, we roll back or auto-unpublish
                    reloadedQuiz.IsPublished = false;
                    await _quizRepository.UpdateAsync(reloadedQuiz);
                    _logger.LogWarning("Quiz unpublished due to PassingMarks violating TotalMarks after question deletion: QuizId={QuizId}", quizId);
                }
            }
        }

        public async Task<BulkUploadResult> BulkUploadQuestionsAsync(int quizId, Stream excelStream)
        {
            var result = new BulkUploadResult();
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null) throw new KeyNotFoundException($"Quiz with id '{quizId}' not found.");

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    result.Errors.Add("No worksheets found in the Excel file.");
                    return result;
                }

                var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                int rowIndex = 2; // Real Excel row number for error reporting

                int maxSortOrder = quiz.Questions.Any() ? quiz.Questions.Max(q => q.SortOrder) : 0;

                foreach (var row in rows)
                {
                    try
                    {
                        string questionText = row.Cell(1).GetString().Trim();
                        if (string.IsNullOrEmpty(questionText)) continue; // Skip empty rows silently

                        string questionTypeStr = row.Cell(2).GetString().Trim();
                        if (!Enum.TryParse<QuestionType>(questionTypeStr, true, out var qType))
                        {
                            result.Errors.Add($"Row {rowIndex}: Invalid QuestionType '{questionTypeStr}'. Must be MultipleChoice, TrueFalse, etc.");
                            rowIndex++;
                            continue;
                        }

                        if (!row.Cell(3).TryGetValue<int>(out int mark) || mark <= 0)
                        {
                            result.Errors.Add($"Row {rowIndex}: Mark must be a valid positive integer.");
                            rowIndex++;
                            continue;
                        }

                        string explanation = row.Cell(4).GetString().Trim();

                        maxSortOrder++;

                        var question = new QuizQuestions
                        {
                            QuizId = quizId,
                            QuestionText = questionText,
                            QuestionType = qType,
                            Mark = mark,
                            Explanation = explanation,
                            SortOrder = maxSortOrder,
                            Answers = new List<QuizOptions>()
                        };

                        // Extract up to 4 options (Cols: 5/6, 7/8, 9/10, 11/12)
                        var optionRequestsForValidation = new List<CreateQuizOptionRequest>();
                        for (int i = 0; i < 4; i++)
                        {
                            int textCol = 5 + (i * 2);
                            int isCorrectCol = 6 + (i * 2);

                            string optText = row.Cell(textCol).GetString().Trim();
                            if (!string.IsNullOrEmpty(optText))
                            {
                                // ClosedXML might fail to parse 'GetBoolean()' if it's text. Try parsing manually if needed.
                                bool isCorrect = false;
                                if (row.Cell(isCorrectCol).DataType == XLDataType.Boolean)
                                    isCorrect = row.Cell(isCorrectCol).GetBoolean();
                                else
                                    bool.TryParse(row.Cell(isCorrectCol).GetString(), out isCorrect);

                                question.Answers.Add(new QuizOptions
                                {
                                    OptionText = optText,
                                    IsCorrect = isCorrect
                                });

                                optionRequestsForValidation.Add(new CreateQuizOptionRequest
                                {
                                    OptionText = optText,
                                    IsCorrect = isCorrect
                                });
                            }
                        }

                        ValidateQuestionOptions(qType, optionRequestsForValidation);

                        await _questionRepository.AddAsync(question);
                        result.TotalImported++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {rowIndex}: {ex.Message}");
                    }
                    
                    rowIndex++;
                }

                if (result.TotalImported > 0)
                {
                    _logger.LogInformation("Bulk uploaded {Count} questions to Quiz {QuizId}.", result.TotalImported, quizId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Excel file for quiz {QuizId}.", quizId);
                result.Errors.Add("Failed to parse Excel file. Ensure it is a valid .xlsx file.");
            }

            return result;
        }

        // ─── Student Quiz-Taking ────────────────────────────────────────────

        public async Task<QuizStudentDetailResponse> GetQuizForStudentAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (!quiz.IsPublished)
            {
                throw new InvalidOperationException("This quiz is not available.");
            }
            return _mapper.Map<QuizStudentDetailResponse>(quiz);
        }

        public async Task<StartAttemptResponse> StartAttemptAsync(int quizId, int userId)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);

            if (!quiz.IsPublished)
            {
                throw new InvalidOperationException("Cannot start an attempt on an unpublished quiz.");
            }

            // Verify Enrollment
            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, section.CourseId);

            var isEnrolled = enrollment != null && 
                (enrollment.EnrollmentStatus == EnrollmentStatus.Active || enrollment.EnrollmentStatus == EnrollmentStatus.Completed);

            if (!isEnrolled)
            {
                throw new UnauthorizedAccessException("Student must be enrolled in the course to start this quiz.");
            }

            // Verify availability dates
            var now = DateTime.UtcNow;
            if (quiz.DeadlineInDays > 0)
            {
                var deadline = enrollment!.EnrolledAt.AddDays(quiz.DeadlineInDays);
                if (now > deadline)
                {
                    throw new InvalidOperationException("This quiz is no longer available.");
                }
            }

            // Verify max attempts using PG function
            var remaining = await _attemptRepository.GetRemainingAttemptsAsync(quizId, userId);

            if (remaining <= 0)
            {
                throw new InvalidOperationException($"Maximum number of attempts ({quiz.MaxAttempts}) has been reached for this quiz.");
            }

            // Create in-progress attempt
            var attempt = new QuizAttempts
            {
                QuizId = quizId,
                UserId = userId,
                Status = AttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                Score = 0.0,
                IsPassed = false
            };

            await _attemptRepository.AddAsync(attempt);

            _logger.LogInformation("Quiz Started: QuizId={QuizId}, UserId={UserId}, AttemptId={AttemptId}", quizId, userId, attempt.Id);

            return new StartAttemptResponse
            {
                AttemptId = attempt.Id,
                QuizId = quizId,
                UserId = userId,
                StartedAt = attempt.StartedAt,
                TimeLimit = quiz.TimeLimit
            };
        }

        public async Task<QuizAttemptResponse> SubmitQuizAsync(int userId, SubmitQuizRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(request.QuizId);

            // Find in-progress attempt
            var attempt = await _attemptRepository.GetInProgressAttemptAsync(request.QuizId, userId)
                ?? throw new InvalidOperationException("No active in-progress attempt found for this quiz.");

            // Verify active status / time limit
            var timeElapsed = DateTime.UtcNow - attempt.StartedAt;
            if (quiz.TimeLimit > TimeSpan.Zero && timeElapsed > quiz.TimeLimit.Add(TimeSpan.FromSeconds(30)))
            {
                attempt.Status = AttemptStatus.Expired;
                attempt.CompletedAt = DateTime.UtcNow;
                await _attemptRepository.UpdateAsync(attempt);
                throw new InvalidOperationException("This quiz attempt has expired because the time limit was exceeded.");
            }

            var now = DateTime.UtcNow;
            
            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, section.CourseId);
            
            if (enrollment != null && quiz.DeadlineInDays > 0)
            {
                var deadline = enrollment.EnrolledAt.AddDays(quiz.DeadlineInDays);
                if (now > deadline)
                {
                    attempt.Status = AttemptStatus.Expired;
                    attempt.CompletedAt = DateTime.UtcNow;
                    await _attemptRepository.UpdateAsync(attempt);
                    throw new InvalidOperationException("This quiz is no longer available.");
                }
            }

            // Save answers
            var answers = new List<QuizAnswers>();
            foreach (var answer in request.Answers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId)
                    ?? throw new ArgumentException($"Question with id '{answer.QuestionId}' does not belong to quiz '{request.QuizId}'.");

                var selectedOption = question.Answers.FirstOrDefault(o => o.Id == answer.SelectedOptionId)
                    ?? throw new ArgumentException($"Option with id '{answer.SelectedOptionId}' does not belong to question '{answer.QuestionId}'.");

                answers.Add(new QuizAnswers
                {
                    AttemptId = attempt.Id,
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId,
                    IsCorrect = selectedOption.IsCorrect
                });
            }

            await _answerRepository.AddRangeAsync(answers);

            attempt.Status = AttemptStatus.Submitted;
            attempt.CompletedAt = DateTime.UtcNow;
            await _attemptRepository.UpdateAsync(attempt);

            // Run PostgreSQL function to calculate score
            var score = await _attemptRepository.CalculateScoreAsync(attempt.Id);
            attempt.Score = score;
            await _attemptRepository.UpdateAsync(attempt); // save score first so pass status can check it

            // Run PostgreSQL function to calculate pass status
            var isPassed = await _attemptRepository.CalculatePassStatusAsync(attempt.Id);
            attempt.IsPassed = isPassed;
            await _attemptRepository.UpdateAsync(attempt);

            _logger.LogInformation("Quiz Submitted: QuizId={QuizId}, UserId={UserId}, Score={Score}, Passed={Passed}",
                request.QuizId, userId, attempt.Score, attempt.IsPassed);

            // Recalculate Course progress
             section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            if (section != null)
            {
                await _progressService.RecalculateCourseProgressAsync(userId, section.CourseId);
            }

            return _mapper.Map<QuizAttemptResponse>(attempt);
        }

        public async Task<IEnumerable<QuizAttemptResponse>> GetUserAttemptsAsync(int quizId, int userId)
        {
            var attempts = await _attemptRepository.GetAttemptsByQuizAndUserAsync(quizId, userId);
            return _mapper.Map<IEnumerable<QuizAttemptResponse>>(attempts);
        }

        public async Task<QuizAttemptDetailResponse> GetAttemptDetailAsync(int attemptId)
        {
            var attempt = await _attemptRepository.GetAttemptWithAnswersAsync(attemptId);
            return _mapper.Map<QuizAttemptDetailResponse>(attempt);
        }

        public async Task<GetRemainingAttemptsResponse> GetRemainingAttemptsAsync(int quizId, int userId)
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            var remaining = await _attemptRepository.GetRemainingAttemptsAsync(quizId, userId);

            return new GetRemainingAttemptsResponse
            {
                QuizId = quizId,
                RemainingAttempts = remaining,
                MaxAttempts = quiz.MaxAttempts
            };
        }

        // ─── Private Helper validations ──────────────────────────────────────

        private void ValidateQuizMarks(Quzzes quiz)
        {
            var totalMarks = quiz.Questions.Sum(q => q.Mark);
            if (quiz.PassingMarks > totalMarks)
            {
                throw new InvalidOperationException($"Passing marks ({quiz.PassingMarks}) cannot be greater than the quiz total marks ({totalMarks}).");
            }
        }

        private void ValidateQuestionOptions(QuestionType type, List<CreateQuizOptionRequest> options)
        {
            if (options == null || options.Count < 2)
            {
                throw new ArgumentException("A question must have at least 2 options.");
            }

            foreach (var opt in options)
            {
                if (string.IsNullOrWhiteSpace(opt.OptionText))
                {
                    throw new ArgumentException("Option text cannot be empty.");
                }
            }

            var correctCount = options.Count(o => o.IsCorrect);

            if (type == QuestionType.MultipleChoice)
            {
                if (correctCount != 1)
                {
                    throw new ArgumentException("Multiple choice questions must have exactly one correct option.");
                }
            }
            else if (type == QuestionType.TrueFalse)
            {
                if (options.Count != 2)
                {
                    throw new ArgumentException("True/False questions must have exactly two options.");
                }
                if (correctCount != 1)
                {
                    throw new ArgumentException("True/False questions must have exactly one correct option.");
                }
            }
        }
    }
}