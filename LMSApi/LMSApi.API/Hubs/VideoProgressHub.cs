using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LMSApi.API.Hubs
{
    /// <summary>
    /// Real-time SignalR hub for video lesson progress tracking.
    ///
    /// Client workflow:
    ///   1. Connect to /hubs/video-progress (with JWT bearer token).
    ///   2. On lesson open, call <see cref="GetResumePosition"/> to get the last watched second.
    ///   3. Every ~30 s during playback, call <see cref="UpdateProgress"/> with the current second.
    ///   4. The hub persists the position and broadcasts a <c>ProgressUpdated</c> event back to the caller.
    ///   5. When the watch percentage reaches 98% the server also emits a <c>LessonCompleted</c> event.
    /// </summary>
    [Authorize]
    public class VideoProgressHub : Hub
    {
        private readonly IStudentProgressService _progressService;
        private readonly ILogger<VideoProgressHub> _logger;

        public VideoProgressHub(
            IStudentProgressService progressService,
            ILogger<VideoProgressHub> logger)
        {
            _progressService = progressService;
            _logger = logger;
        }

        /// <summary>
        /// Called by the client just before starting video playback.
        /// Returns the last watched second so the player can seek to the correct position.
        /// Returns null when the student has never started this lesson.
        /// </summary>
        /// <param name="lessonId">ID of the video lesson being opened.</param>
        public async Task GetResumePosition(int lessonId)
        {
            var userId = Context.User!.GetUserId();
            try
            {
                var progress = await _progressService.GetLessonProgressAsync(userId, lessonId);
                await Clients.Caller.SendAsync("ResumePosition", new
                {
                    LessonId = lessonId,
                    LastWatchedSecond = progress?.LastWatchedSecond ?? 0,
                    WatchPercentage = progress?.WatchPercentage ?? 0m,
                    IsCompleted = progress?.IsCompleted ?? false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching resume position for User {UserId}, Lesson {LessonId}", userId, lessonId);
                await Clients.Caller.SendAsync("Error", new { Message = "Failed to fetch resume position." });
            }
        }

        /// <summary>
        /// Called by the video player periodically (e.g. every 30 s) to save the current playback position.
        /// The backend calculates the watch percentage from the lesson's stored duration.
        /// Emits <c>ProgressUpdated</c> to the caller on success.
        /// Emits <c>LessonCompleted</c> when the 98% threshold is crossed for the first time.
        /// </summary>
        /// <param name="request">Contains the current playback second.</param>
        public async Task UpdateProgress(UpdateVideoProgressRequest request)
        {
            var userId = Context.User!.GetUserId();
            try
            {
                var progress = await _progressService.UpdateVideoProgressAsync(
                    userId, request.LessonId, request.LastWatchedSecond);

                await Clients.Caller.SendAsync("ProgressUpdated", progress);

                if (progress.IsCompleted)
                {
                    await Clients.Caller.SendAsync("LessonCompleted", new
                    {
                        progress.LessonId,
                        progress.CompletedAt,
                        progress.WatchPercentage
                    });
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Lesson not found for User {UserId}", userId);
                await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for User {UserId}, Lesson {LessonId}", userId, request.LessonId);
                await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating video progress for User {UserId}, Lesson {LessonId}", userId, request.LessonId);
                await Clients.Caller.SendAsync("Error", new { Message = "Failed to save progress." });
            }
        }
    }
}
