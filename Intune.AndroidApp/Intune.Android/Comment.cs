using System;

namespace Intune.Android
{
    public enum CommentStatus
    {
        Unread = 0,
        Read = 1,
    }

    public class Comment
    {
        public int Id { get; set; }
        public int ByUserId { get; set; }
        public string ByUserName { get; set; }
        public int ToUserId { get; set; }
        public string ToUserName { get; set; }
        public int AccountId { get; set; }
        public int EntryId { get; set; }
        public string CommentText { get; set; }
        public DateTime DateTimeStamp { get; set; }
        public CommentStatus Status { get; set; }

        public Comment()
        {
        }

        public bool IsValid()
        {
            //TODO: Include validations for min/max length of all fields
            //TODO: Validate for Email and Mobile formats.

            if (string.IsNullOrWhiteSpace(CommentText))
                return false;

            return true;
        }
    }
}
