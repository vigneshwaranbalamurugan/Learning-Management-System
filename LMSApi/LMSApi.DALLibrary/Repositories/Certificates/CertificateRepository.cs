using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly LMSDbContext _context;

        public CertificateRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<Certificates?> GetByGuidAsync(Guid certificateId)
        {
            return await _context.Certificates
                .Include(c => c.Course)
                    .ThenInclude(course => course.Instructor)
                        .ThenInclude(i => i.UserProfile)
                .Include(c => c.Course)
                    .ThenInclude(course => course.Category)
                .Include(c => c.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(c => c.Template)
                .FirstOrDefaultAsync(c => c.CertificateId == certificateId);
        }

        public async Task<Certificates?> GetByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.Certificates
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);
        }

        public async Task<IEnumerable<Certificates>> GetCertificatesByUserAsync(int userId)
        {
            return await _context.Certificates
                .Include(c => c.Course)
                    .ThenInclude(course => course.Instructor)
                        .ThenInclude(i => i.UserProfile)
                .Include(c => c.Course)
                    .ThenInclude(course => course.Category)
                .Include(c => c.User)
                    .ThenInclude(u => u.UserProfile)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Certificates> Certificates, int TotalCount)> GetCertificatesByUserPagedAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _context.Certificates
                .Include(c => c.Course)
                    .ThenInclude(course => course.Instructor)
                        .ThenInclude(i => i.UserProfile)
                .Include(c => c.Course)
                    .ThenInclude(course => course.Category)
                .Include(c => c.User)
                    .ThenInclude(u => u.UserProfile)
                .Where(c => c.UserId == userId);

            var totalCount = await query.CountAsync();

            var certificates = await query
                .OrderByDescending(c => c.IssuedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (certificates, totalCount);
        }

        public async Task<CertificateTemplates?> GetActiveTemplateAsync()
        {
            return await _context.CertificateTemplates
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<CertificateTemplates?> GetTemplateByIdAsync(int id)
        {
            return await _context.CertificateTemplates
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<CertificateTemplates>> GetAllTemplatesAsync()
        {
            return await _context.CertificateTemplates
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddCertificateAsync(Certificates certificate)
        {
            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCertificateAsync(Certificates certificate)
        {
            _context.Certificates.Update(certificate);
            await _context.SaveChangesAsync();
        }

        public async Task AddTemplateAsync(CertificateTemplates template)
        {
            await _context.CertificateTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTemplateAsync(CertificateTemplates template)
        {
            _context.CertificateTemplates.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAllTemplatesAsync()
        {
            var activeTemplates = await _context.CertificateTemplates.Where(t => t.IsActive).ToListAsync();
            foreach (var t in activeTemplates)
            {
                t.IsActive = false;
            }
            await _context.SaveChangesAsync();
        }
    }
}
