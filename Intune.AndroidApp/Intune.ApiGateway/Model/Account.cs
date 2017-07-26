using System;

namespace Intune.ApiGateway.Model
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UserAccountRole Role { get; set; }
        public DateTime AddedOn { get; set; }
        public int UserId { get; set; }
        public int ContactId { get; set; }
        public decimal Balance { get; set; }
        public bool HasEntries { get; set; }
        public bool HasComments { get; set; }
        public bool HasUnreadComments { get; set; }
        public bool IsNew { get { return Id == 0; } }
    }
}
