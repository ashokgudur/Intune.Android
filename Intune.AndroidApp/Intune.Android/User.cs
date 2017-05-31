using System;

namespace Intune.Android
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string AtUserName { get; set; }
        public string Password { get; set; }
        public DateTime CreatedOn { get; set; }
        public string SessionToken { get; set; }
        public bool IsNew { get { return Id == 0; } }

        public bool IsValid()
        {
            //TODO: Additional validations for max/min length should be cheecked.
            //TODO: Validate Email for email format
            //TODO: Validate Mobile Number for its length, with country code included.
            //TODO: Validate AtUserName to prefix with '@' symobol. Must be included by user.

            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (string.IsNullOrWhiteSpace(Mobile)) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;
            if (string.IsNullOrWhiteSpace(AtUserName)) return false;
            if (string.IsNullOrWhiteSpace(Password)) return false;

            return true;
        }
    }
}