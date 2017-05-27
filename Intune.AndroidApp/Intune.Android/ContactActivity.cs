using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Contact - Intune")]
    public class ContactActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Contact);

            var okButton = FindViewById<Button>(Resource.Id.contactOkButton);
            okButton.Click += OkButton_Click; ;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var fullName = FindViewById<EditText>(Resource.Id.contactNameEditText);
            var email = FindViewById<EditText>(Resource.Id.contactEmailEditText);
            var mobile = FindViewById<EditText>(Resource.Id.contactMobileEditText);
            var address = FindViewById<EditText>(Resource.Id.contactAddressEditText);

            var contact = new Contact
            {
                Name = fullName.Text,
                Email = email.Text,
                Mobile = mobile.Text,
                Address = address.Text,
                UserId = Intent.GetIntExtra("LoginUserId", 0),
                CreatedOn = DateTime.Now
            };

            var result = FindViewById<TextView>(Resource.Id.contactResultTextView);
            result.Text = "Adding new contact...";
            contact = IntuneService.AddContact(contact);

            if (contact != null)
            {
                result.Text = string.Format("Contact {0} created.", contact.Name);
                clearForm();
                return;
            }

            result.Text = "Contact creation FAILED!";
        }

        private void clearForm()
        {
            var fullName = FindViewById<EditText>(Resource.Id.contactNameEditText);
            var email = FindViewById<EditText>(Resource.Id.contactEmailEditText);
            var mobile = FindViewById<EditText>(Resource.Id.contactMobileEditText);
            var address = FindViewById<EditText>(Resource.Id.contactAddressEditText);

            fullName.Text = "";
            email.Text = "";
            mobile.Text = "";
            address.Text = "";
        }
    }
}
