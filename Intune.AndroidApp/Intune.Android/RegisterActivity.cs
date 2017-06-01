using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Intune - Register new user")]
    public class RegisterActivity : Activity
    {
        User _user = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);

            var userId = Intent.GetIntExtra("LoginUserId", 0);
            _user = IntuneService.GetUserById(userId);

            if (_user.IsNew)
                Title = "Register New User - Intune";
            else
            {
                Title = string.Format("{0} - Intune", _user.Name);
                var email = FindViewById<EditText>(Resource.Id.emailEditText);
                email.Enabled = false;
            }

            fillForm();

            var okButton = FindViewById<Button>(Resource.Id.okButton);
            okButton.Click += OkButton_Click; ;

            if (!_user.IsNew)
                okButton.Text = "Save";
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var email = FindViewById<EditText>(Resource.Id.emailEditText);
            var password = FindViewById<EditText>(Resource.Id.passwordEditText);
            var fullName = FindViewById<EditText>(Resource.Id.fullNameEditText);
            var mobile = FindViewById<EditText>(Resource.Id.mobileEditText);
            var atUserName = FindViewById<EditText>(Resource.Id.atUserNameEditText);

            _user.Email = email.Text;
            _user.Password = password.Text;
            _user.Name = fullName.Text;
            _user.Mobile = mobile.Text;
            _user.AtUserName = atUserName.Text;

            var result = FindViewById<TextView>(Resource.Id.registerUserResultTextView);

            if (!_user.IsValid())
            {
                result.Text = "Please enter all the details";
                return;
            }

            if (_user.IsNew)
            {
                result.Text = "Registering new user...";
                _user.CreatedOn = DateTime.Now;
                _user = IntuneService.RegiterUser(_user);
                if (_user == null)
                    result.Text = string.Format("Can't register user!!!");
                else
                    result.Text = string.Format("User {0} registered", _user.Name);

                var okButton = FindViewById<Button>(Resource.Id.okButton);
                okButton.Text = "Save";
            }
            else
            {
                result.Text = "Updating user...";
                _user = IntuneService.UpdateUser(_user);
                if (_user == null)
                    result.Text = string.Format("Can't save user!!!");
                else
                    result.Text = string.Format("Profile saved", _user.Name);
            }
        }

        private void fillForm()
        {
            var email = FindViewById<EditText>(Resource.Id.emailEditText);
            var password = FindViewById<EditText>(Resource.Id.passwordEditText);
            var fullName = FindViewById<EditText>(Resource.Id.fullNameEditText);
            var mobile = FindViewById<EditText>(Resource.Id.mobileEditText);
            var atUserName = FindViewById<EditText>(Resource.Id.atUserNameEditText);

            email.Text = _user.Email;
            password.Text = _user.Password;
            fullName.Text = _user.Name;
            mobile.Text = _user.Mobile;
            atUserName.Text = _user.AtUserName;
        }
    }
}