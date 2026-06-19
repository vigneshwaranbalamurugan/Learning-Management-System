using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs.Logs;

namespace LMSApi.BALLibrary.Services
{
    public class AdminLogService : IAdminLogService
    {
        private readonly IActivityLogsRepository _activityLogsRepository;
        private readonly IAuditLogsRepository _auditLogsRepository;
        private readonly IMapper _mapper;

        public AdminLogService(
            IActivityLogsRepository activityLogsRepository,
            IAuditLogsRepository auditLogsRepository,
            IMapper mapper)
        {
            _activityLogsRepository = activityLogsRepository;
            _auditLogsRepository = auditLogsRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(int? userId, string? activityType, int page, int pageSize)
        {
            var logs = await _activityLogsRepository.GetFilteredLogsAsync(userId, activityType, page, pageSize);
            return _mapper.Map<IEnumerable<ActivityLogResponse>>(logs);
        }

        public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(int? userId, string? tableName, string? action, int page, int pageSize)
        {
            var logs = await _auditLogsRepository.GetFilteredLogsAsync(userId, tableName, action, page, pageSize);
            return _mapper.Map<IEnumerable<AuditLogResponse>>(logs);
        }
    }
}
