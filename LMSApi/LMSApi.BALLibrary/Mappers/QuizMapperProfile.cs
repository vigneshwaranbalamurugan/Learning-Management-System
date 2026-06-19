using AutoMapper;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Mappers
{
    public class QuizMapperProfile : Profile
    {
        public QuizMapperProfile()
        {
            // ─── Quiz ───────────────────────────────────────────────────────
            CreateMap<Quzzes, QuizResponse>()
                .ForMember(dest => dest.QuestionCount,
                    opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Count : 0))
                .ForMember(dest => dest.TotalMarks,
                    opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Sum(q => q.Mark) : 0));

            CreateMap<Quzzes, QuizDetailResponse>()
                .ForMember(dest => dest.Questions,
                    opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.TotalMarks,
                    opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Sum(q => q.Mark) : 0));

            CreateMap<CreateQuizRequest, Quzzes>();
            CreateMap<UpdateQuizRequest, Quzzes>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ─── Question ───────────────────────────────────────────────────
            CreateMap<QuizQuestions, QuizQuestionResponse>()
                .ForMember(dest => dest.Options,
                    opt => opt.MapFrom(src => src.Answers));

            CreateMap<CreateQuizQuestionRequest, QuizQuestions>()
                .ForMember(dest => dest.Answers, opt => opt.Ignore()); // handled manually in service

            // ─── Option ─────────────────────────────────────────────────────
            CreateMap<QuizOptions, QuizOptionResponse>();
            CreateMap<CreateQuizOptionRequest, QuizOptions>();

            // ─── Student View (hides IsCorrect) ─────────────────────────────
            CreateMap<Quzzes, QuizStudentDetailResponse>()
                .ForMember(dest => dest.Questions,
                    opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.TotalMarks,
                    opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Sum(q => q.Mark) : 0));

            CreateMap<QuizQuestions, QuizStudentQuestionResponse>()
                .ForMember(dest => dest.Options,
                    opt => opt.MapFrom(src => src.Answers));

            CreateMap<QuizOptions, QuizStudentOptionResponse>();

            // ─── Attempt ────────────────────────────────────────────────────
            CreateMap<QuizAttempts, QuizAttemptResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ObtainedScore, opt => opt.MapFrom(src => src.Score))
                .ForMember(dest => dest.TotalScore, opt => opt.MapFrom(src => src.Quiz != null && src.Quiz.Questions != null ? src.Quiz.Questions.Sum(q => q.Mark) : 0.0));

            CreateMap<QuizAttempts, QuizAttemptDetailResponse>()
                .ForMember(dest => dest.Answers,
                    opt => opt.MapFrom(src => src.Answers))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ObtainedScore, opt => opt.MapFrom(src => src.Score))
                .ForMember(dest => dest.TotalScore, opt => opt.MapFrom(src => src.Quiz != null && src.Quiz.Questions != null ? src.Quiz.Questions.Sum(q => q.Mark) : 0.0));

            // ─── Answer ─────────────────────────────────────────────────────
            CreateMap<QuizAnswers, QuizAnswerResponse>()
                .ForMember(dest => dest.QuestionText,
                    opt => opt.MapFrom(src => src.Question != null ? src.Question.QuestionText : string.Empty))
                .ForMember(dest => dest.SelectedOptionText,
                    opt => opt.MapFrom(src => src.SelectedOption != null ? src.SelectedOption.OptionText : string.Empty));
        }
    }
}
