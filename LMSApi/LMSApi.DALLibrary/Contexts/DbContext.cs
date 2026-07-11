using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using LMSApi.ModelLibrary.Enums;
using LMSApi.DALLibrary.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMSApi.DALLibrary.Contexts
{
    public class LMSDbContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<UserProfiles> UserProfiles { get; set; }

        // Course module
        public DbSet<CourseCategories> CourseCategories { get; set; }
        public DbSet<CourseLanguages> CourseLanguages { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<Lessons> Lessons { get; set; }
        public DbSet<LessonResources> LessonResources { get; set; }
        public DbSet<StudentProgress> StudentProgresses { get; set; }
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<Reviews> Reviews { get; set; }

        // Hybrid Learning module
        public DbSet<CourseBatch> CourseBatches { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<Payments> Payments { get; set; }

        // Platform Charges & Payouts
        public DbSet<PlatformFeeConfig> PlatformFeeConfigs { get; set; }
        public DbSet<InstructorPayoutAccount> InstructorPayoutAccounts { get; set; }
        public DbSet<InstructorPayout> InstructorPayouts { get; set; }

        // Razorpay Route Step-Split Onboarding Tables
        public DbSet<InstructorLinkedAccount> InstructorLinkedAccounts { get; set; }
        public DbSet<InstructorStakeholder> InstructorStakeholders { get; set; }
        public DbSet<InstructorPayoutProduct> InstructorPayoutProducts { get; set; }

        // Quiz module
        public DbSet<Quzzes> Quizzes { get; set; }
        public DbSet<QuizQuestions> QuizQuestions { get; set; }
        public DbSet<QuizOptions> QuizOptions { get; set; }
        public DbSet<QuizAttempts> QuizAttempts { get; set; }
        public DbSet<QuizAnswers> QuizAnswers { get; set; }

        // Assignment module
        public DbSet<Assignments> Assignments { get; set; }
        public DbSet<AssignmentSubmissions> AssignmentSubmissions { get; set; }

        // Certificate module
        public DbSet<Certificates> Certificates { get; set; }
        public DbSet<CertificateTemplates> CertificateTemplates { get; set; }

        // Notification module
        public DbSet<Notifications> Notifications { get; set; }

        // Logs module
        public DbSet<ActivityLogs> ActivityLogs { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<WebhookEventLog> WebhookEventLogs { get; set; }

        // Discussions module
        public DbSet<Discussions> Discussions { get; set; }
        public DbSet<DiscussionReplies> DiscussionReplies { get; set; }
        public DbSet<DiscussionLikes> DiscussionLikes { get; set; }

        private readonly ICurrentUserProvider? _currentUserProvider;
        private readonly ILogger<LMSDbContext>? _logger;

        public LMSDbContext(
            DbContextOptions<LMSDbContext> dbContextOptions,
            ICurrentUserProvider? currentUserProvider = null,
            ILogger<LMSDbContext>? logger = null) : base(dbContextOptions)
        {
            _currentUserProvider = currentUserProvider;
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Courses>().HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Users>()
                  .HasIndex(u => u.Email);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LMSDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            ApplyAuditTimestamps();
            var pendingActivities = new List<PendingActivity>();
            var auditEntries = OnBeforeSaveChanges(pendingActivities);
            var result = base.SaveChanges();
            OnAfterSaveChanges(auditEntries, pendingActivities).GetAwaiter().GetResult();
            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            var pendingActivities = new List<PendingActivity>();
            var auditEntries = OnBeforeSaveChanges(pendingActivities);
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries, pendingActivities);
            return result;
        }

        private void ApplyAuditTimestamps()
        {
            var utcNow = DateTime.UtcNow;
            foreach (EntityEntry entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    setAuditTimestamps(entry.Entity, utcNow, create: true);
                }
                else if (entry.State == EntityState.Modified)
                {
                    setAuditTimestamps(entry.Entity, utcNow, create: false);
                }
            }
        }

        private void setAuditTimestamps(object entity, DateTime utcNow, bool create)
        {
            switch (entity)
            {
                case Users user:
                    if (create) user.CreatedAt = utcNow;
                    user.UpdatedAt = utcNow;
                    break;
                case UserRoles role:
                    if (create) role.CreatedAt = utcNow;
                    role.UpdatedAt = utcNow;
                    break;
                case UserProfiles profile:
                    if (create) profile.CreatedAt = utcNow;
                    profile.UpdatedAt = utcNow;
                    break;
                case CourseCategories category:
                    if (create) category.CreatedAt = utcNow;
                    category.UpdatedAt = utcNow;
                    break;
                case Courses course:
                    if (create) course.CreatedAt = utcNow;
                    course.UpdatedAt = utcNow;
                    break;
                case CourseSection section:
                    if (create) section.CreatedAt = utcNow;
                    section.UpdatedAt = utcNow;
                    break;
                case CourseBatch batch:
                    if (create) batch.CreatedAt = utcNow;
                    batch.UpdatedAt = utcNow;
                    break;
                case StudentProgress progress:
                    progress.LastAccessed = utcNow;
                    break;
                case Assignments assignment:
                    if (create) assignment.CreatedAt = utcNow;
                    break;
                case WishList wishlist:
                    if (create) wishlist.AddedAt = utcNow;
                    wishlist.UpdatedAt = utcNow;
                    break;
                case Reviews review:
                    if (create) review.CreatedAt = utcNow;
                    review.UpdatedAt = utcNow;
                    break;
                case PlatformFeeConfig feeConfig:
                    if (create) feeConfig.CreatedAt = utcNow;
                    break;
                case InstructorPayoutAccount payoutAccount:
                    if (create) payoutAccount.CreatedAt = utcNow;
                    payoutAccount.UpdatedAt = utcNow;
                    break;
                case InstructorPayout instructorPayout:
                    if (create) instructorPayout.CreatedAt = utcNow;
                    instructorPayout.UpdatedAt = utcNow;
                    break;
                case InstructorLinkedAccount la:
                    if (create) la.CreatedAt = utcNow;
                    la.UpdatedAt = utcNow;
                    break;
                case InstructorStakeholder sh:
                    if (create) sh.CreatedAt = utcNow;
                    sh.UpdatedAt = utcNow;
                    break;
                case InstructorPayoutProduct pp:
                    if (create) pp.CreatedAt = utcNow;
                    pp.UpdatedAt = utcNow;
                    break;
                case CertificateTemplates certTemplate:
                    if (create) certTemplate.CreatedAt = utcNow;
                    certTemplate.UpdatedAt = utcNow;
                    break;
                case Certificates certificate:
                    if (create) certificate.IssuedAt = utcNow;
                    break;
                case Notifications notification:
                    if (create) notification.CreatedAt = utcNow;
                    break;
            }
        }

        private int GetCurrentUserId()
        {
            return _currentUserProvider?.GetCurrentUserId() ?? 1; // Default System User/Admin
        }

        private List<AuditEntry> OnBeforeSaveChanges(List<PendingActivity> pendingActivities)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            var userId = GetCurrentUserId();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLogs || entry.Entity is ActivityLogs || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    UserId = userId
                };
                auditEntries.Add(auditEntry);

                var action = ActionType.Insert;
                switch (entry.State)
                {
                    case EntityState.Added:
                        action = ActionType.Insert;
                        auditEntry.Action = ActionType.Insert;
                        foreach (var property in entry.Properties)
                        {
                            if (property.Metadata.IsPrimaryKey())
                            {
                                auditEntry.TemporaryProperties.Add(property);
                                continue;
                            }
                            auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue ?? null!;
                        }
                        break;

                    case EntityState.Deleted:
                        action = ActionType.Delete;
                        auditEntry.Action = ActionType.Delete;
                        foreach (var property in entry.Properties)
                        {
                            if (property.Metadata.IsPrimaryKey())
                            {
                                auditEntry.RecordId = (int)(property.CurrentValue ?? 0);
                                continue;
                            }
                            auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue ?? null!;
                        }
                        break;

                    case EntityState.Modified:
                        action = ActionType.Update;
                        auditEntry.Action = ActionType.Update;
                        foreach (var property in entry.Properties)
                        {
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue ?? null!;
                                auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue ?? null!;
                            }
                            if (property.Metadata.IsPrimaryKey())
                            {
                                auditEntry.RecordId = (int)(property.CurrentValue ?? 0);
                            }
                        }
                        break;
                }

                // Map high-level activities before state changes are committed
                var entity = entry.Entity;
                switch (entity)
                {
                    case Users user:
                        if (action == ActionType.Insert)
                        {
                            pendingActivities.Add(new PendingActivity
                            {
                                ActivityType = ActivityType.UserRegister,
                                DescriptionFactory = () => $"User registered successfully: {user.Email}",
                                Entity = user
                            });
                        }
                        else if (action == ActionType.Update)
                        {
                            var lastLoginProp = entry.Property("LastLoginAt");
                            if (lastLoginProp.IsModified)
                            {
                                pendingActivities.Add(new PendingActivity
                                {
                                    UserId = user.Id,
                                    ActivityType = ActivityType.UserLogin,
                                    DescriptionFactory = () => $"User logged in successfully: {user.Email}"
                                });
                            }
                        }
                        break;

                    case Enrollments enrollment:
                        if (action == ActionType.Insert)
                        {
                            pendingActivities.Add(new PendingActivity
                            {
                                UserId = enrollment.UserId,
                                ActivityType = ActivityType.CourseEnrollment,
                                DescriptionFactory = () => $"Student enrolled in Batch ID: {enrollment.BatchId}"
                            });
                        }
                        break;

                    case QuizAttempts attempt:
                        if (action == ActionType.Insert)
                        {
                            pendingActivities.Add(new PendingActivity
                            {
                                UserId = attempt.UserId,
                                ActivityType = ActivityType.QuizAttemptStarted,
                                DescriptionFactory = () => $"Started Quiz Attempt ID: {attempt.Id} for Quiz ID: {attempt.QuizId}",
                                Entity = attempt
                            });
                        }
                        else if (action == ActionType.Update)
                        {
                            var statusProp = entry.Property("Status");
                            if (statusProp.IsModified && attempt.Status == AttemptStatus.Submitted)
                            {
                                pendingActivities.Add(new PendingActivity
                                {
                                    UserId = attempt.UserId,
                                    ActivityType = ActivityType.QuizAttemptSubmitted,
                                    DescriptionFactory = () => $"Submitted Quiz Attempt ID: {attempt.Id} for Quiz ID: {attempt.QuizId}. Score: {attempt.Score}",
                                    Entity = attempt
                                });
                            }
                        }
                        break;

                    case AssignmentSubmissions submission:
                        if (action == ActionType.Insert)
                        {
                            pendingActivities.Add(new PendingActivity
                            {
                                UserId = submission.StudentId,
                                ActivityType = ActivityType.AssignmentSubmitted,
                                DescriptionFactory = () => $"Submitted Assignment ID: {submission.AssignmentId} (Submission ID: {submission.Id})",
                                Entity = submission
                            });
                        }
                        else if (action == ActionType.Update)
                        {
                            var marksProp = entry.Property("MarksAwarded");
                            if (marksProp.IsModified)
                            {
                                pendingActivities.Add(new PendingActivity
                                {
                                    UserId = submission.StudentId,
                                    ActivityType = ActivityType.AssignmentGraded,
                                    DescriptionFactory = () => $"Assignment ID: {submission.AssignmentId} graded. Marks Awarded: {submission.MarksAwarded}"
                                });
                            }
                        }
                        break;

                    case Certificates certificate:
                        if (action == ActionType.Insert)
                        {
                            pendingActivities.Add(new PendingActivity
                            {
                                UserId = certificate.UserId,
                                ActivityType = ActivityType.CertificateIssued,
                                DescriptionFactory = () => $"Certificate ID: {certificate.CertificateId} issued for Course ID: {certificate.CourseId}"
                            });
                        }
                        break;

                    case Payments payment:
                        if (action == ActionType.Insert || action == ActionType.Update)
                        {
                            var statusProp = entry.Property("Status");
                            if (statusProp.IsModified || action == ActionType.Insert)
                            {
                                if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Transferred)
                                {
                                    pendingActivities.Add(new PendingActivity
                                    {
                                        UserId = payment.UserId,
                                        ActivityType = ActivityType.PaymentSuccess,
                                        DescriptionFactory = () => $"Payment successful for Batch ID: {payment.BatchId}. Amount: {payment.Amount} {payment.Currency}"
                                    });
                                }
                                else if (payment.Status == PaymentStatus.Failed)
                                {
                                    pendingActivities.Add(new PendingActivity
                                    {
                                        UserId = payment.UserId,
                                        ActivityType = ActivityType.PaymentFailed,
                                        DescriptionFactory = () => $"Payment failed for Batch ID: {payment.BatchId}."
                                    });
                                }
                            }
                        }
                        break;

                    case Courses course:
                        if (action == ActionType.Insert)
                        {
                            pendingActivities.Add(new PendingActivity
                            {
                                UserId = course.InstructorId,
                                ActivityType = ActivityType.CourseCreated,
                                DescriptionFactory = () => $"Course created: {course.Title} (ID: {course.Id})",
                                Entity = course
                            });
                        }
                        else if (action == ActionType.Update)
                        {
                            var statusProp = entry.Property("Status");
                            if (statusProp.IsModified && course.Status == CourseStatus.Published)
                            {
                                pendingActivities.Add(new PendingActivity
                                {
                                    UserId = course.InstructorId,
                                    ActivityType = ActivityType.CoursePublished,
                                    DescriptionFactory = () => $"Course published: {course.Title} (ID: {course.Id})",
                                    Entity = course
                                });
                            }
                        }
                        break;

                    case InstructorLinkedAccount la:
                        pendingActivities.Add(new PendingActivity
                        {
                            UserId = la.InstructorId,
                            ActivityType = action == ActionType.Insert
                                ? ActivityType.PayoutAccountRegistered
                                : ActivityType.PayoutAccountUpdated,
                            DescriptionFactory = () =>
                                $"Instructor payout account (Step 1) {(action == ActionType.Insert ? "created" : "updated")}: {la.RazorpayAccountId}"
                        });
                        break;
                }
            }

            return auditEntries;
        }

        private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries, List<PendingActivity> pendingActivities)
        {
            if ((auditEntries == null || auditEntries.Count == 0) && (pendingActivities == null || pendingActivities.Count == 0))
                return;

            if (auditEntries != null && auditEntries.Count > 0)
            {
                foreach (var auditEntry in auditEntries)
                {
                    foreach (var prop in auditEntry.TemporaryProperties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.RecordId = (int)(prop.CurrentValue ?? 0);
                        }
                        else
                        {
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue ?? null!;
                        }
                    }
                    var auditLog = auditEntry.ToAuditLog();
                    AuditLogs.Add(auditLog);
                    _logger?.LogInformation("EF Core Audit Log created: Table: {TableName}, Record: {RecordId}, Action: {Action}", auditLog.TableName, auditLog.RecordId, auditLog.Action.ToString());
                }
            }

            if (pendingActivities != null && pendingActivities.Count > 0)
            {
                var activities = new List<ActivityLogs>();
                foreach (var pending in pendingActivities)
                {
                    var userId = pending.UserId;
                    if (pending.Entity is Users user)
                    {
                        userId = user.Id;
                    }

                    var desc = pending.DescriptionFactory();
                    activities.Add(new ActivityLogs
                    {
                        UserId = userId,
                        ActivityType = pending.ActivityType,
                        Description = desc,
                        Timestamp = DateTime.UtcNow
                    });
                    _logger?.LogInformation("Activity Log created: User ID: {UserId}, Type: {ActivityType}, Description: {Description}", userId, pending.ActivityType.ToString(), desc);
                }

                if (activities.Any())
                {
                    ActivityLogs.AddRange(activities);
                }
            }

            await base.SaveChangesAsync();
        }

        private class PendingActivity
        {
            public int UserId { get; set; }
            public ActivityType ActivityType { get; set; }
            public Func<string> DescriptionFactory { get; set; } = null!;
            public object? Entity { get; set; }
        }
    }
}
