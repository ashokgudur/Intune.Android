using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Intune.Android
{
    public class Contact
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ContactUserId { get; set; }
        public bool HasComments { get; set; }
        public bool HasUnreadComments { get; set; }

        public bool HasIntune()
        {
            return ContactUserId > 0;
        }

        public bool IsValid()
        {
            //TODO: Include validations for min/max length of all fields
            //TODO: Validate for Email and Mobile formats.

            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (string.IsNullOrWhiteSpace(Email))
                return false;

            if (string.IsNullOrWhiteSpace(Mobile))
                return false;

            return true;
        }
    }
}