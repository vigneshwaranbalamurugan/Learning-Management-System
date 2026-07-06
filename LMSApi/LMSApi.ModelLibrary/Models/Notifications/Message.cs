using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Message
    {
        public  MessageType MessageType { get; set; }
        public Message()
        {

        }
    }

    public class EmailAttachment
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }

    public class EmailMessage : Message
    {
        public string RecipientEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public List<EmailAttachment> Attachments { get; set; } = new();

        public EmailMessage(string recipientEmail, string subject, string body)
        {
            RecipientEmail = recipientEmail;
            Subject = subject;
            Body = body;
            IsHtml = false;
            MessageType = MessageType.Email;
        }

    }

    public class SMSMessage : Message
    {
        public string RecipientPhoneNumber { get; set; }
        public string Content { get; set; }

        public SMSMessage(string recipientPhoneNumber, string content)
        {
            RecipientPhoneNumber = recipientPhoneNumber;
            Content = content;
            MessageType = MessageType.SMS;
        }
    }

}