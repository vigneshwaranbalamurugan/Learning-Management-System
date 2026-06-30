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

        public async Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(string? userQuery, string? activityType, int page, int pageSize)
        {
            var logs = await _activityLogsRepository.GetFilteredLogsAsync(userQuery, activityType, page, pageSize);
            return _mapper.Map<IEnumerable<ActivityLogResponse>>(logs);
        }

        public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(string? userQuery, string? tableName, string? action, int page, int pageSize)
        {
            var logs = await _auditLogsRepository.GetFilteredLogsAsync(userQuery, tableName, action, page, pageSize);
            return _mapper.Map<IEnumerable<AuditLogResponse>>(logs);
        }

        public async Task<int> GetActivityLogsCountAsync(string? userQuery, string? activityType)
        {
            return await _activityLogsRepository.GetFilteredLogsCountAsync(userQuery, activityType);
        }

        public async Task<int> GetAuditLogsCountAsync(string? userQuery, string? tableName, string? action)
        {
            return await _auditLogsRepository.GetFilteredLogsCountAsync(userQuery, tableName, action);
        }
    }
}
