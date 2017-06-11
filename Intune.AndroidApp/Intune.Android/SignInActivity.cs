using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Views;
using System.Threading;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Views.InputMethods;
using Android.Util;

namespace Intune.Android
{
    [Activity(Label = "Intune", MainLauncher = true, Icon = "@drawable/icon")]
    public class SignInActivity : Activity
    {
        TextInputLayout _emailLayout;
        TextInputLayout _passwordLayout;
        TextInputEditText _emailEditText;
        TextInputEditText _passwordEditText;
        View _rootView;

        protected override void OnCreate(Bundle bundle)
        {
            SetTheme(Resource.Style.SignInTheme);

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SignIn);

            Title = "Welcome to Intune";

            _emailLayout = FindViewById<TextInputLayout>(Resource.Id.loginEmailInputLayout);
            _passwordLayout = FindViewById<TextInputLayout>(Resource.Id.loginPasswordInputLayout);
            _emailEditText = FindViewById<TextInputEditText>(Resource.Id.loginEmailEditText);
            _passwordEditText = FindViewById<TextInputEditText>(Resource.Id.loginPasswordEditText);
            _rootView = FindViewById<View>(Resource.Id.loginRootLinearLayout);

            var robotoTypeface = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/Roboto-Regular.ttf");
            _emailEditText.Typeface = robotoTypeface;
            _passwordEditText.Typeface = robotoTypeface;

            var signInMessageTextView = FindViewById<TextView>(Resource.Id.signInMessageTextView);
            signInMessageTextView.PaintFlags = PaintFlags.UnderlineText;

            var signInButton = FindViewById<Button>(Resource.Id.signInButton);
            signInButton.Click += SignInButton_Click;

#if DEBUG
            _emailEditText.Text = "ashok.gudur@gmail.com";
            _passwordEditText.Text = "ashokg";
#endif
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
            var email = _emailEditText.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                _emailLayout.ErrorEnabled = true;
                _emailLayout.Error = "Email is required";
                Snackbar.Make(_rootView, "Please enter email address", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _emailLayout.ErrorEnabled = false;
                            _emailEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            Snackbar.Make(_rootView, "Emailing your password...", Snackbar.LengthIndefinite).Show();

            try
            {
                IntuneService.ForgotPassword(email);
                Snackbar.Make(_rootView, "Your password has been emailed.", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _passwordEditText.Text = "";
                            _passwordEditText.RequestFocus();
                        })
                        .Show();
            }
            catch (Exception exp)
            {
                Snackbar.Make(_rootView, exp.Message, Snackbar.LengthIndefinite)
                        .SetAction("OK", (v) =>
                        {
                            _emailEditText.RequestFocus();
                        })
                        .Show();
            }
        }

        private void RegisterMenu_Click()
        {
            StartActivity(typeof(SignUpActivity));
        }

        private void SignInButton_Click(object sender, System.EventArgs e)
        {
            hideKeyboard();
            _emailLayout.ErrorEnabled = false;
            var email = _emailEditText.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                _emailLayout.ErrorEnabled = true;
                _emailLayout.Error = "Email is required";
                Snackbar.Make(_rootView, "Please enter email", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _emailLayout.ErrorEnabled = false;
                            _emailEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            if (!Patterns.EmailAddress.Matcher(email).Matches())
            {
                _emailLayout.ErrorEnabled = true;
                _emailLayout.Error = "Not a valid email address";
                Snackbar.Make(_rootView, "Please enter valid email address", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _emailLayout.ErrorEnabled = false;
                            _emailEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            _passwordLayout.ErrorEnabled = false;

            if (string.IsNullOrWhiteSpace(_passwordEditText.Text.Trim()))
            {
                _passwordLayout.ErrorEnabled = true;
                _passwordLayout.Error = "Password is required";
                Snackbar.Make(_rootView, "Please enter password", Snackbar.LengthLong)
                        .SetAction("Clear", (v) =>
                        {
                            _passwordLayout.ErrorEnabled = false;
                            _passwordEditText.Text = string.Empty;
                            _passwordEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            Snackbar.Make(_rootView, "Logging into Intune...", Snackbar.LengthIndefinite).Show();
            var us = new IntuneUserService(this);
            ThreadPool.QueueUserWorkItem(o => us.SignIn(_emailEditText.Text, _passwordEditText.Text));
        }

        private void hideKeyboard()
        {
            var imm = GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private void showKeyboard(View view)
        {
            var imm = GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm.ShowSoftInput(view, ShowFlags.Forced);
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
                var rootView = _activity.FindViewById<View>(Resource.Id.loginRootLinearLayout);
                var user = IntuneService.SignIn(email, password);
                if (user == null)
                {
                    Snackbar.Make(rootView, "Cannot login!!!", Snackbar.LengthLong)
                    .SetAction("OK", (v) => { })
                    .Show();
                    return;
                }

                Snackbar.Make(rootView, "Loading accounts...", Snackbar.LengthIndefinite).Show();
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
    }
}

