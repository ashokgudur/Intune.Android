using System;

namespace Intune.ApiGateway.Model
{
    public class UserAccountShareRole
    {
        public int UserId { get; set; }
        public UserAccountRole Role { get; set; }

        public UserAccountShareRole() { }
    }
}
