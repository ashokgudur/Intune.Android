using System;

namespace Intune.ApiGateway.Model
{
    public class ChatMessage
    {
        public string ByEmail { get; set; }
        public string ByName { get; set; }
        public string ToEmail { get; set; }
        public string ToName { get; set; }
        public string Text { get; set; }
        public int ByUserId { get; set; }
        public int ToUserId { get; set; }
        public int AccountId { get; set; }
        public int EntryId { get; set; }
        public DateTime DateTimeStamp { get; set; }

        public CommentType CommentType
        {
            get
            {
                if (EntryId > 0)
                    return CommentType.Entry;
                else if (AccountId > 0)
                    return CommentType.Account;
                else
                    return CommentType.Contact;
            }
        }
    }
}
