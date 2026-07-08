using AutoMapper;
using ClosedXML.Excel;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.DALLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class QuizQuestionService : IQuizQuestionService
    {

        private readonly IQuizRepository _quizRepository;
        private readonly IQuizQuestionRepository _questionRepository;
        private readonly IQuizOptionRepository _optionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizQuestionService> _logger;

        public QuizQuestionService(
            IQuizRepository quizRepository,
            IQuizQuestionRepository questionRepository,
            IQuizOptionRepository optionRepository,
            IMapper mapper,
            ILogger<QuizQuestionService> logger
            )
        {
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _mapper = mapper;
            _logger = logger;
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

            // Verify QuestionText uniqueness
            if (quiz.Questions.Any(q => string.Equals(q.QuestionText.Trim(), request.QuestionText.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"A question with this text already exists in this quiz.");
            }

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
            if (reloadedQuiz.Status == PublishStatus.Published)
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

            if (request.QuestionText != null)
            {
                if (string.IsNullOrWhiteSpace(request.QuestionText))
                    throw new ArgumentException("Question text cannot be null or empty.", nameof(request.QuestionText));

                if (quiz.Questions.Any(q => q.Id != id && string.Equals(q.QuestionText.Trim(), request.QuestionText.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"A question with this text already exists in this quiz.");
                }
                question.QuestionText = request.QuestionText;
            }
            if (request.QuestionType.HasValue) question.QuestionType = request.QuestionType.Value;
            if (request.Mark.HasValue) question.Mark = request.Mark.Value;
            if (request.Explanation != null) question.Explanation = request.Explanation;
            if (request.SortOrder.HasValue) question.SortOrder = request.SortOrder.Value;

            // If options provided, replace/update all existing options in-place to avoid breaking FK constraints with QuizAnswers
            if (request.Options != null && request.Options.Count > 0)
            {
                ValidateQuestionOptions(request.QuestionType ?? question.QuestionType, request.Options);

                var existingOptions = question.Answers.OrderBy(a => a.Id).ToList();
                var newOptions = request.Options;

                int minCount = Math.Min(existingOptions.Count, newOptions.Count);
                for (int i = 0; i < minCount; i++)
                {
                    existingOptions[i].OptionText = newOptions[i].OptionText;
                    existingOptions[i].IsCorrect = newOptions[i].IsCorrect;
                }

                if (newOptions.Count > existingOptions.Count)
                {
                    // Add new options
                    for (int i = existingOptions.Count; i < newOptions.Count; i++)
                    {
                        var opt = _mapper.Map<QuizOptions>(newOptions[i]);
                        opt.QuestionId = question.Id;
                        question.Answers.Add(opt);
                    }
                }
                else if (existingOptions.Count > newOptions.Count)
                {
                    // Delete extra options
                    var optionsToDelete = existingOptions.Skip(newOptions.Count).ToList();
                    await _optionRepository.DeleteRangeAsync(optionsToDelete);
                    foreach (var opt in optionsToDelete)
                    {
                        question.Answers.Remove(opt);
                    }
                }
            }

            await _questionRepository.UpdateAsync(question);

            _logger.LogInformation("Question Updated: Id={Id}", id);

            // Check published constraints
            var reloadedQuiz = await _quizRepository.GetQuizWithQuestionsAsync(question.QuizId);
            if (reloadedQuiz.Status == PublishStatus.Published)
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
            if (reloadedQuiz.Status == PublishStatus.Published)
            {
                try
                {
                    ValidateQuizMarks(reloadedQuiz);
                }
                catch (Exception)
                {
                    // If deleting the question violates some rule, we could roll back or auto-unpublish
                    reloadedQuiz.Status = PublishStatus.Draft;
                    await _quizRepository.UpdateAsync(reloadedQuiz);
                    _logger.LogWarning("Quiz unpublished due to validation failure after question deletion: QuizId={QuizId}", quizId);
                }
            }
        }

        public async Task ReorderQuestionsAsync(int quizId, BulkReorderQuestionsRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                throw new ArgumentException("Reorder items cannot be empty.");

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId)
                ?? throw new KeyNotFoundException($"Quiz with id '{quizId}' not found.");

            // Phase 1: Shift all affected questions to a very high temporary sort order
            // to avoid unique constraint collisions during the update sequence.
            int tempBase = 500000;
            foreach (var (item, idx) in request.Items.Select((it, i) => (it, i)))
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == item.QuestionId)
                    ?? throw new KeyNotFoundException($"Question with id '{item.QuestionId}' not found in quiz '{quizId}'.");
                question.SortOrder = tempBase + idx;
                await _questionRepository.UpdateAsync(question);
            }

            // Phase 2: Assign the real final sort orders
            foreach (var item in request.Items)
            {
                var question = quiz.Questions.First(q => q.Id == item.QuestionId);
                question.SortOrder = item.SortOrder;
                await _questionRepository.UpdateAsync(question);
            }

            _logger.LogInformation("Questions reordered for QuizId={QuizId}, Count={Count}", quizId, request.Items.Count);
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

                var existingQuestionTexts = new HashSet<string>(quiz.Questions.Select(q => q.QuestionText.Trim()), StringComparer.OrdinalIgnoreCase);

                foreach (var row in rows)
                {
                    try
                    {
                        string questionText = row.Cell(1).GetString().Trim();
                        if (string.IsNullOrEmpty(questionText)) continue; // Skip empty rows silently

                        if (!existingQuestionTexts.Add(questionText))
                        {
                            result.Errors.Add($"Row {rowIndex}: A question with text '{questionText}' already exists in this quiz.");
                            rowIndex++;
                            continue;
                        }

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

        // ─── Private Helper validations ──────────────────────────────────────

        private void ValidateQuizMarks(Quzzes quiz)
        {
            if (quiz.PassingPercentage < 0 || quiz.PassingPercentage > 100)
            {
                throw new InvalidOperationException($"Passing percentage ({quiz.PassingPercentage}) must be between 0 and 100.");
            }
        }    
     private void ValidateQuestionOptions(QuestionType type, List<CreateQuizOptionRequest> options)
        {
            if (options == null || options.Count < 2)
            {
                throw new ArgumentException("A question must have at least 2 options.");
            }

            var uniqueOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var opt in options)
            {
                if (string.IsNullOrWhiteSpace(opt.OptionText))
                {
                    throw new ArgumentException("Option text cannot be empty.");
                }

                if (!uniqueOptions.Add(opt.OptionText.Trim()))
                {
                    throw new ArgumentException($"Duplicate option '{opt.OptionText.Trim()}' is not allowed in the same question.");
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