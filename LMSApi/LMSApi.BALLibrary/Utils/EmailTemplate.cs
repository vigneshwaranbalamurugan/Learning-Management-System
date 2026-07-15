using LMSApi.ModelLibrary.Enums;

namespace LMSApi.BALLibrary.Utils
{
    public static class EmailTemplate
    {
        public static string GetVerificationTemplate(string email, string link)
        {
            return $@"<html>
            <body>
              <h2>Verify your email</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(email)},</p>
              <p>Thank you for registering. Please click the button below to verify your email address.</p>
              <p><a href=""{System.Net.WebUtility.HtmlEncode(link)}"" style=""display:inline-block;padding:10px 20px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:4px;"">Verify Email</a></p>
              <p>If the button doesn't work, copy and paste the following link into your browser:</p>
              <p>{System.Net.WebUtility.HtmlEncode(link)}</p>
              <hr />
              <small>Do not share this link. It expires in 24 hours.</small>
            </body>
          </html>";
        }
        public static string GetPasswordResetTemplate(string email, string link)
        {
            return $@"<html>
            <body>
              <h2>Reset Your Password</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(email)},</p>
              <p>You requested to reset your password. Please click the button below to reset it.</p>
              <p><a href=""{System.Net.WebUtility.HtmlEncode(link)}"" style=""display:inline-block;padding:10px 20px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:4px;"">Reset Password</a></p>
              <p>If the button doesn't work, copy and paste the following link into your browser:</p>
              <p>{System.Net.WebUtility.HtmlEncode(link)}</p>
              <hr />
              <small>If you did not request a password reset, please ignore this email. This link expires in 24 hours.</small>
            </body>
          </html>";
        }

        public static string GetWelcomeTemplate(string email, string name, string role)
        {
            return $@"<html>
            <body>
              <h2>Welcome to LMS!</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>Welcome to the platform! You have successfully registered as a {System.Net.WebUtility.HtmlEncode(role)}.</p>
              <p>We are excited to have you on board.</p>
            </body>
          </html>";
        }

        public static string GetCourseEnrollmentTemplate(string name, string courseTitle, CourseAccessType accessType, string? batchName)
        {
            var batchInfo = accessType == CourseAccessType.CohortBased && !string.IsNullOrEmpty(batchName) 
                ? $"<p>You have been enrolled in the cohort batch: <strong>{System.Net.WebUtility.HtmlEncode(batchName)}</strong>.</p>" 
                : "";

            return $@"<html>
            <body>
              <h2>Enrollment Successful</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>You have successfully enrolled in the course: <strong>{System.Net.WebUtility.HtmlEncode(courseTitle)}</strong>.</p>
              {batchInfo}
              <p>Happy learning!</p>
            </body>
          </html>";
        }

        public static string GetPaymentInvoiceTemplate(string learnerName, string courseTitle, decimal amount, string currency, string invoiceNo)
        {
            return $@"<html>
            <body>
              <h2>Payment Successful</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(learnerName)},</p>
              <p>Thank you for purchasing <strong>{System.Net.WebUtility.HtmlEncode(courseTitle)}</strong>.</p>
              <p>Your payment of {System.Net.WebUtility.HtmlEncode(currency)} {amount} was successful. Invoice <strong>{System.Net.WebUtility.HtmlEncode(invoiceNo)}</strong> is attached to this email as a PDF.</p>
              <p>Happy learning!</p>
            </body>
          </html>";
        }

        public static string GetCourseStatusUpdatedTemplate(string name, string courseTitle, string newStatus, string? reason)
        {
            var reasonInfo = !string.IsNullOrEmpty(reason) 
                ? $"<p>Reason: {System.Net.WebUtility.HtmlEncode(reason)}</p>" 
                : "";

            return $@"<html>
            <body>
              <h2>Course Status Updated</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>The status of your course <strong>{System.Net.WebUtility.HtmlEncode(courseTitle)}</strong> has been updated to: <strong>{System.Net.WebUtility.HtmlEncode(newStatus)}</strong>.</p>
              {reasonInfo}
            </body>
          </html>";
        }

        public static string GetInstructorPayoutTemplate(string name, string courseTitle, decimal amount, string payoutId)
        {
            return $@"<html>
            <body>
              <h2>Payout Initiated</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>A payout of <strong>&#8377;{amount}</strong> for the course <strong>{System.Net.WebUtility.HtmlEncode(courseTitle)}</strong> has been initiated.</p>
              <p>Payout Reference ID: {System.Net.WebUtility.HtmlEncode(payoutId)}</p>
            </body>
          </html>";
        }

        public static string GetContentPublishedTemplate(string name, string courseTitle, string contentType, string contentTitle, string batchName)
        {
            return $@"<html>
            <body>
              <h2>New Content Published</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>A new {System.Net.WebUtility.HtmlEncode(contentType)} titled <strong>{System.Net.WebUtility.HtmlEncode(contentTitle)}</strong> has been published in the course <strong>{System.Net.WebUtility.HtmlEncode(courseTitle)}</strong>.</p>
              <p>Batch: {System.Net.WebUtility.HtmlEncode(batchName)}</p>
            </body>
          </html>";
        }

        public static string GetAssignmentGradedTemplate(string name, string assignmentTitle, int marks, int totalMarks, bool isPassed, string feedback)
        {
            var status = isPassed ? "Passed" : "Failed";
            return $@"<html>
            <body>
              <h2>Assignment Graded</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>Your submission for the assignment <strong>{System.Net.WebUtility.HtmlEncode(assignmentTitle)}</strong> has been graded.</p>
              <p>Marks: <strong>{marks} / {totalMarks}</strong></p>
              <p>Status: <strong>{status}</strong></p>
              <p>Feedback: {System.Net.WebUtility.HtmlEncode(feedback)}</p>
            </body>
          </html>";
        }

        public static string GetBatchStatusTemplate(string name, string batchName, string courseTitle, string status)
        {
            return $@"<html>
            <body>
              <h2>Batch Status Updated</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>The batch <strong>{System.Net.WebUtility.HtmlEncode(batchName)}</strong> for the course <strong>{System.Net.WebUtility.HtmlEncode(courseTitle)}</strong> is now <strong>{System.Net.WebUtility.HtmlEncode(status)}</strong>.</p>
            </body>
          </html>";
        }

        public static string GetPayoutWebhookTemplate(string name, string status, string payoutId, string? failureReason)
        {
            var reasonInfo = !string.IsNullOrEmpty(failureReason) 
                ? $"<p>Reason: {System.Net.WebUtility.HtmlEncode(failureReason)}</p>" 
                : "";
            return $@"<html>
            <body>
              <h2>Payout Status Update</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>Your payout with reference ID <strong>{System.Net.WebUtility.HtmlEncode(payoutId)}</strong> is now: <strong>{System.Net.WebUtility.HtmlEncode(status)}</strong>.</p>
              {reasonInfo}
            </body>
          </html>";
        }

        public static string GetCertificateIssuedTemplate(string learnerName, string courseName, string certificateImageUrl, Guid certificateId, string frontendUrl)
        {
            var verifyUrl = $"{frontendUrl.TrimEnd('/')}/verify-certificate/{certificateId}";
            return $@"<html>
            <body>
              <h2>Congratulations on Completing Your Course!</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(learnerName)},</p>
              <p>You have successfully completed the course: <strong>{System.Net.WebUtility.HtmlEncode(courseName)}</strong>.</p>
              <p>Your certificate has been generated. You can view and download it using the link below:</p>
              <p><a href=""{System.Net.WebUtility.HtmlEncode(certificateImageUrl)}"" style=""display:inline-block;padding:10px 20px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:4px;"">View Certificate</a></p>
              <p>Certificate ID: {certificateId}</p>
              <p>You can verify this certificate anytime at: <a href=""{verifyUrl}"">{verifyUrl}</a></p>
            </body>
          </html>";
        }

        public static string GetPayoutOnboardingTemplate(string name, string stepTitle, string message)
        {
            return $@"<html>
            <body>
              <h2>Payout Onboarding: {System.Net.WebUtility.HtmlEncode(stepTitle)}</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>{System.Net.WebUtility.HtmlEncode(message)}</p>
              <br />
              <p>Thank you,</p>
              <p>The LMS Team</p>
            </body>
          </html>";
        }

        /// <summary>
        /// Email sent to learner and instructor when a payment dispute is raised or its status changes.
        /// </summary>
        public static string GetPaymentDisputeTemplate(string name, string disputeEvent, string courseTitle, decimal amount, string? disputeId)
        {
            var disputeRef = !string.IsNullOrEmpty(disputeId)
                ? $"<p>Dispute Reference ID: <strong>{System.Net.WebUtility.HtmlEncode(disputeId)}</strong></p>"
                : "";
            return $@"<html>
            <body>
              <h2>Payment Dispute Update</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>A payment dispute event has occurred for the course <strong>{System.Net.WebUtility.HtmlEncode(courseTitle)}</strong> (Amount: &#8377;{amount}).</p>
              <p>Event: <strong>{System.Net.WebUtility.HtmlEncode(disputeEvent)}</strong></p>
              {disputeRef}
              <p>If you have any questions, please contact our support team.</p>
              <br />
              <p>Thank you,</p>
              <p>The LMS Team</p>
            </body>
          </html>";
        }

        /// <summary>
        /// Email sent to the learner when an order notification delivery succeeds or fails.
        /// </summary>
        public static string GetOrderNotificationTemplate(string name, bool delivered)
        {
            var statusMsg = delivered
                ? "Your order confirmation notification was delivered successfully."
                : "We encountered an issue delivering your order confirmation notification. Please check your notification settings.";
            return $@"<html>
            <body>
              <h2>Order Notification {(delivered ? "Delivered" : "Failed")}</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>{statusMsg}</p>
              <br />
              <p>Thank you,</p>
              <p>The LMS Team</p>
            </body>
          </html>";
        }

        /// <summary>
        /// Email sent to the instructor when their Route product status changes.
        /// </summary>
        public static string GetProductRouteTemplate(string name, string productEvent, string message)
        {
            return $@"<html>
            <body>
              <h2>Payout Product Update</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>
              <p>Your Razorpay Route payout product status has changed.</p>
              <p>Event: <strong>{System.Net.WebUtility.HtmlEncode(productEvent)}</strong></p>
              <p>{System.Net.WebUtility.HtmlEncode(message)}</p>
              <br />
              <p>Thank you,</p>
              <p>The LMS Team</p>
            </body>
          </html>";
        }

        /// <summary>
        /// Email sent to admin when Razorpay processes a settlement to the platform bank account.
        /// </summary>
        public static string GetSettlementTemplate(string adminName, string settlementId, string rawSummary)
        {
            return $@"<html>
            <body>
              <h2>Settlement Processed</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(adminName)},</p>
              <p>Razorpay has processed a settlement to the platform bank account.</p>
              <p>Settlement ID: <strong>{System.Net.WebUtility.HtmlEncode(settlementId)}</strong></p>
              <p>{System.Net.WebUtility.HtmlEncode(rawSummary)}</p>
              <br />
              <p>The LMS Team</p>
            </body>
          </html>";
        }
    }
}
