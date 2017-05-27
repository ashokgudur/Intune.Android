using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Views;

namespace Intune.Android
{
    [Activity(Label = "Intune - Login", MainLauncher = true, Icon = "@drawable/icon")]
    public class LoginActivity : Activity
    {
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.login_mennus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.login_menu_register:
                    RegisterMenu_Click();
                    break;
                case Resource.Id.login_menu_forgot:
                    ForgotPasswordMenu_Click();
                    break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);

            var signInButton = FindViewById<Button>(Resource.Id.buttonSignIn);
            signInButton.Click += SignInButton_Click;
        }

        private void ForgotPasswordMenu_Click()
        {
            var result = FindViewById<TextView>(Resource.Id.textViewResult);
            var email = FindViewById<EditText>(Resource.Id.editEmail);
            if (string.IsNullOrWhiteSpace(email.Text.Trim()))
            {
                result.Text = "Please enter email address";
                return;
            }

            result.Text = "Emailing your password...";
            try
            {
                IntuneService.ForgotPassword(email.Text.Trim());
                result.Text = "Your password has been emailed.";
            }
            catch (Exception exp)
            {
                result.Text = exp.Message;
            }
        }

        private void RegisterMenu_Click()
        {
            StartActivity(typeof(RegisterActivity));
        }

        private void SignInButton_Click(object sender, System.EventArgs e)
        {
            var result = FindViewById<TextView>(Resource.Id.textViewResult);

            result.Text = "Logging into Intune...";
            var email = FindViewById<EditText>(Resource.Id.editEmail);
            var password = FindViewById<EditText>(Resource.Id.editPassword);
            var user = IntuneService.SignIn(email.Text, password.Text);
            if (user == null)
            {
                result.Text = "Cannot Login!!!";
                return;
            }

            result.Text = "Loading accounts...";
            showAccountsActivity(user);
        }

        private void showAccountsActivity(User user)
        {
            var mainActivity = new Intent(this, typeof(AccountsActivity));
            mainActivity.PutExtra("LoginUserId", user.Id);
            mainActivity.PutExtra("LoginUserName", user.Name);
            StartActivity(mainActivity);
        }
    }
}

