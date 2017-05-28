using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Intune - Register new user")]
    public class RegisterActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);

            var okButton = FindViewById<Button>(Resource.Id.okButton);
            okButton.Click += OkButton_Click; ;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var email = FindViewById<EditText>(Resource.Id.emailEditText);
            var password = FindViewById<EditText>(Resource.Id.passwordEditText);
            var fullName = FindViewById<EditText>(Resource.Id.fullNameEditText);
            var mobile = FindViewById<EditText>(Resource.Id.mobileEditText);
            var atUserName = FindViewById<EditText>(Resource.Id.atUserNameEditText);

            var user = new User
            {
                Email = email.Text,
                Password = password.Text,
                Name = fullName.Text,
                Mobile = mobile.Text,
                AtUserName = atUserName.Text,
                CreatedOn = DateTime.Now
            };

            var result = FindViewById<TextView>(Resource.Id.registerUserResultTextView);

            if (!user.IsValid())
            {
                result.Text = "Please enter all the details";
                return;
            }

            result.Text = "Registering new user...";
            user = IntuneService.RegiterUser(user);

            if (user != null)
            {
                result.Text = string.Format("{0} is registered", user.Name);
                clearForm();
                return;
            }

            result.Text = "Registeration FAILED!!!";
        }

        private void clearForm()
        {
            var email = FindViewById<EditText>(Resource.Id.emailEditText);
            var password = FindViewById<EditText>(Resource.Id.passwordEditText);
            var fullName = FindViewById<EditText>(Resource.Id.fullNameEditText);
            var mobile = FindViewById<EditText>(Resource.Id.mobileEditText);
            var atUserName = FindViewById<EditText>(Resource.Id.atUserNameEditText);

            email.Text = "";
            password.Text = "";
            fullName.Text = "";
            mobile.Text = "";
            atUserName.Text = "";
        }
    }
}