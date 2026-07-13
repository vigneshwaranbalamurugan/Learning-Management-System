using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class DiscussionService : IDiscussionService
    {
        private readonly IDiscussionRepository _discussionRepository;
        private readonly IDiscussionReplyRepository _replyRepository;
        private readonly IDiscussionLikeRepository _likeRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;

        public DiscussionService(
            IDiscussionRepository discussionRepository,
            IDiscussionReplyRepository replyRepository,
            IDiscussionLikeRepository likeRepository,
            ILessonRepository lessonRepository,
            IMapper mapper)
        {
            _discussionRepository = discussionRepository;
            _replyRepository = replyRepository;
            _likeRepository = likeRepository;
            _lessonRepository = lessonRepository;
            _mapper = mapper;
        }

        public async Task<DiscussionResponse> CreateDiscussionAsync(int userId, CreateDiscussionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var courseId = await _lessonRepository.GetCourseIdByLessonIdAsync(request.LessonId);
            if (courseId == null) throw new KeyNotFoundException("Lesson not found.");

            var discussion = new Discussions
            {
                LessonId = request.LessonId,
                CourseId = courseId.Value,
                UserId = userId,
                Title = request.Title,
                Content = request.Content,
                IsPinned = false,
                IsLocked = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _discussionRepository.AddAsync(discussion);

            var created = await _discussionRepository.GetByIdWithDetailsAsync(discussion.Id);
            var response = _mapper.Map<DiscussionResponse>(created);
            response.ReplyCount = 0;
            response.LikeCount = 0;
            return response;
        }

        public async Task<DiscussionResponse> UpdateDiscussionAsync(int userId, int discussionId, UpdateDiscussionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var discussion = await _discussionRepository.GetByIdAsync(discussionId);
            if (discussion == null || discussion.UserId != userId)
                throw new KeyNotFoundException("Discussion not found or unauthorized.");

            if (!string.IsNullOrWhiteSpace(request.Title)) discussion.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Content)) discussion.Content = request.Content;
            discussion.UpdatedAt = DateTime.UtcNow;

            await _discussionRepository.UpdateAsync(discussion);

            var updated = await _discussionRepository.GetByIdWithDetailsAsync(discussionId);
            var response = _mapper.Map<DiscussionResponse>(updated);
            response.ReplyCount = (await _replyRepository.GetByDiscussionAsync(discussionId)).Count();
            response.LikeCount = await _likeRepository.GetLikeCountAsync(discussionId);
            return response;
        }

        public async Task DeleteDiscussionAsync(int userId, int discussionId)
        {
            var discussion = await _discussionRepository.GetByIdAsync(discussionId);
            if (discussion == null || discussion.UserId != userId)
                throw new KeyNotFoundException("Discussion not found or unauthorized.");

            await _discussionRepository.DeleteAsync(discussionId);
        }

        public async Task<IEnumerable<DiscussionResponse>> GetLessonDiscussionsAsync(int lessonId, int userId)
        {
            var discussions = await _discussionRepository.GetByLessonAsync(lessonId);

            var responses = new List<DiscussionResponse>();
            foreach (var d in discussions)
            {
                var response = _mapper.Map<DiscussionResponse>(d);
                response.ReplyCount = (await _replyRepository.GetByDiscussionAsync(d.Id)).Count();
                response.LikeCount = await _likeRepository.GetLikeCountAsync(d.Id);
                response.IsLikedByUser = await _likeRepository.GetByDiscussionAndUserAsync(d.Id, userId) != null;
                responses.Add(response);
            }
            return responses;
        }

        public async Task<DiscussionDetailResponse> GetDiscussionDetailAsync(int discussionId, int userId)
        {
            var discussion = await _discussionRepository.GetByIdWithDetailsAsync(discussionId);
            if (discussion == null) throw new KeyNotFoundException("Discussion not found.");

            var replies = await _replyRepository.GetByDiscussionAsync(discussionId);

            var response = _mapper.Map<DiscussionDetailResponse>(discussion);
            response.Replies = _mapper.Map<List<ReplyResponse>>(replies);

            if (discussion.IsDeleted)
            {
                response.Title = "[deleted]";
                response.Content = "[deleted]";
                response.UserName = "[deleted]";
                response.UserEmail = "[deleted]";
            }

            foreach (var r in response.Replies)
            {
                var original = replies.First(x => x.Id == r.Id);
                if (original.IsDeleted)
                {
                    r.ReplyText = "[deleted]";
                    r.UserName = "[deleted]";
                    r.UserEmail = "[deleted]";
                }
            }

            response.ReplyCount = response.Replies.Count;
            response.LikeCount = await _likeRepository.GetLikeCountAsync(discussionId);
            response.IsLikedByUser = await _likeRepository.GetByDiscussionAndUserAsync(discussionId, userId) != null;
            return response;
        }

        public async Task<ReplyResponse> AddReplyAsync(int userId, int discussionId, CreateReplyRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var discussion = await _discussionRepository.GetByIdAsync(discussionId);
            if (discussion == null) throw new KeyNotFoundException("Discussion not found.");
            if (discussion.IsLocked) throw new InvalidOperationException("This discussion is locked.");

            var reply = new DiscussionReplies
            {
                DiscussionId = discussionId,
                UserId = userId,
                ReplyText = request.ReplyText,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _replyRepository.AddAsync(reply);

            var replies = await _replyRepository.GetByDiscussionAsync(discussionId);
            var created = replies.FirstOrDefault(r => r.Id == reply.Id);
            return _mapper.Map<ReplyResponse>(created);
        }

        public async Task<ReplyResponse> UpdateReplyAsync(int userId, int replyId, UpdateReplyRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var reply = await _replyRepository.GetByIdAsync(replyId);
            if (reply == null || reply.UserId != userId)
                throw new KeyNotFoundException("Reply not found or unauthorized.");

            reply.ReplyText = request.ReplyText;
            reply.UpdatedAt = DateTime.UtcNow;

            await _replyRepository.UpdateAsync(reply);

            var replies = await _replyRepository.GetByDiscussionAsync(reply.DiscussionId);
            var updated = replies.FirstOrDefault(r => r.Id == reply.Id);
            return _mapper.Map<ReplyResponse>(updated);
        }

        public async Task DeleteReplyAsync(int userId, int replyId)
        {
            var reply = await _replyRepository.GetByIdAsync(replyId);
            if (reply == null || reply.UserId != userId)
                throw new KeyNotFoundException("Reply not found or unauthorized.");

            await _replyRepository.DeleteAsync(replyId);
        }

        public async Task<int> ToggleLikeAsync(int userId, int discussionId)
        {
            var discussion = await _discussionRepository.GetByIdAsync(discussionId);
            if (discussion == null) throw new KeyNotFoundException("Discussion not found.");

            var existingLike = await _likeRepository.GetByDiscussionAndUserAsync(discussionId, userId);
            if (existingLike != null)
            {
                await _likeRepository.DeleteAsync(existingLike.Id);
            }
            else
            {
                await _likeRepository.AddAsync(new DiscussionLikes
                {
                    DiscussionId = discussionId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return await _likeRepository.GetLikeCountAsync(discussionId);
        }
    }
}
