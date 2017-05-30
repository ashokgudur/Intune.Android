using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Views;
using System.Threading;

namespace Intune.Android
{
    [Activity(Label = "Intune - Login", MainLauncher = true, Icon = "@drawable/icon")]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);

            var signInButton = FindViewById<Button>(Resource.Id.buttonSignIn);
            signInButton.Click += SignInButton_Click;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.login_menus, menu);
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

            var email = FindViewById<EditText>(Resource.Id.editEmail);
            var password = FindViewById<EditText>(Resource.Id.editPassword);

            if (string.IsNullOrWhiteSpace(email.Text.Trim()))
            {
                result.Text = "Please enter email address";
                return;
            }

            if (string.IsNullOrWhiteSpace(password.Text.Trim()))
            {
                result.Text = "Please enter password";
                return;
            }

            result.Text = "Logging into Intune...";
            var us = new IntuneUserService(this);
            ThreadPool.QueueUserWorkItem(o => us.SignIn(email.Text, password.Text));
        }

        private class IntuneUserService
        {
            Activity _activity;

            public IntuneUserService(Activity activity)
            {
                _activity = activity;
            }

            public void SignIn(string email, string password)
            {
                var result = _activity.FindViewById<TextView>(Resource.Id.textViewResult);
                var user = IntuneService.SignIn(email, password);
                if (user == null)
                {
                    _activity.RunOnUiThread(() => result.Text = "Cannot Login!!!");
                    return;
                }

                _activity.RunOnUiThread(() => result.Text = "Loading accounts...");
                showAccountsActivity(user);
            }

            private void showAccountsActivity(User user)
            {
                var accountsActivity = new Intent(_activity, typeof(AccountsActivity));
                accountsActivity.PutExtra("LoginUserId", user.Id);
                accountsActivity.PutExtra("LoginUserName", user.Name);
                _activity.StartActivity(accountsActivity);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            var result = FindViewById<TextView>(Resource.Id.textViewResult);
            result.Text = "";
        }

        private void showAccountsActivity(User user)
        {
            var accountsActivity = new Intent(this, typeof(AccountsActivity));
            accountsActivity.PutExtra("LoginUserId", user.Id);
            accountsActivity.PutExtra("LoginUserName", user.Name);
            StartActivity(accountsActivity);
        }
    }
}

