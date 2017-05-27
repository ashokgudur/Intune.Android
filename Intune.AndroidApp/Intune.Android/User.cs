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
    }
}