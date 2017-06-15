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
using System.Text.RegularExpressions;

namespace Intune.Android
{
    [Activity(Label = "Intune", Icon = "@drawable/icon")]
    public class SignInActivity : Activity
    {
        TextInputLayout _idLayout;
        TextInputEditText _idEditText;
        TextInputLayout _passwordLayout;
        TextInputEditText _passwordEditText;
        CheckBox _rememberMeCheckBox;

        View _rootView;

        protected override void OnCreate(Bundle bundle)
        {
            SetTheme(Resource.Style.SignInTheme);

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SignIn);

            Title = "Welcome to Intune";

            _rootView = FindViewById<View>(Resource.Id.loginRootLinearLayout);

            _idLayout = FindViewById<TextInputLayout>(Resource.Id.signInIdInputLayout);
            _idEditText = FindViewById<TextInputEditText>(Resource.Id.signInIdTextInputEditText);

            _passwordLayout = FindViewById<TextInputLayout>(Resource.Id.signInPasswordInputLayout);
            _passwordEditText = FindViewById<TextInputEditText>(Resource.Id.signInPasswordTextInputEditText);
            _rememberMeCheckBox = FindViewById<CheckBox>(Resource.Id.signInRememberMeCheckBox);

            var robotoTypeface = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/Roboto-Regular.ttf");
            _idEditText.Typeface = robotoTypeface;
            _passwordEditText.Typeface = robotoTypeface;

            var signInMessageTextView = FindViewById<TextView>(Resource.Id.signInMessageTextView);
            signInMessageTextView.PaintFlags = PaintFlags.UnderlineText;

            var signInButton = FindViewById<Button>(Resource.Id.signInButton);
            signInButton.Click += SignInButton_Click;

#if DEBUG
            _idEditText.Text = "ashok.gudur@gmail.com";
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
                case Resource.Id.login_menu_sign_up:
                    signUpMenu_Click();
                    break;
                case Resource.Id.login_menu_forgot:
                    forgotPasswordMenu_Click();
                    break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void forgotPasswordMenu_Click()
        {
            //var email = _emailEditText.Text.Trim();

            //if (string.IsNullOrWhiteSpace(email))
            //{
            //    _emailLayout.ErrorEnabled = true;
            //    _emailLayout.Error = "Email is required";
            //    Snackbar.Make(_rootView, "Please enter email address", Snackbar.LengthLong)
            //            .SetAction("OK", (v) =>
            //            {
            //                _emailLayout.ErrorEnabled = false;
            //                _emailEditText.RequestFocus();
            //            })
            //            .Show();
            //    return;
            //}

            //Snackbar.Make(_rootView, "Emailing your password...", Snackbar.LengthIndefinite).Show();

            //try
            //{
            //    IntuneService.ForgotPassword(email);
            //    Snackbar.Make(_rootView, "Your password has been emailed.", Snackbar.LengthLong)
            //            .SetAction("OK", (v) =>
            //            {
            //                _passwordEditText.Text = "";
            //                _passwordEditText.RequestFocus();
            //            })
            //            .Show();
            //}
            //catch (Exception exp)
            //{
            //    Snackbar.Make(_rootView, exp.Message, Snackbar.LengthIndefinite)
            //            .SetAction("OK", (v) =>
            //            {
            //                _emailEditText.RequestFocus();
            //            })
            //            .Show();
            //}
        }

        private void signUpMenu_Click()
        {
            StartActivity(typeof(SignUpActivity));
        }

        private string getSignInId()
        {
            return _idEditText.Text.Trim().ToLower();
        }

        private bool isIdEmail()
        {
            var regex = new Regex("[A-Za-z.@]");
            return regex.IsMatch(getSignInId());
        }

        private bool isMobileNumberValid()
        {
            return Regex.IsMatch(getSignInId(), @"^[0-9]{10}$");
        }

        private void SignInButton_Click(object sender, System.EventArgs e)
        {
            hideKeyboard();
            _idLayout.ErrorEnabled = false;
            if (string.IsNullOrWhiteSpace(getSignInId()))
            {
                _idLayout.ErrorEnabled = true;
                _idLayout.Error = "mobile or email is required";
                Snackbar.Make(_rootView, "Please enter mobile or email.", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _idLayout.ErrorEnabled = false;
                            _idEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            if (isIdEmail() && !Patterns.EmailAddress.Matcher(getSignInId()).Matches() || !isMobileNumberValid())
            {
                _idLayout.ErrorEnabled = true;
                _idLayout.Error = "Valid mobile or email is required";
                Snackbar.Make(_rootView, "Enter valid mobile or email address.", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _idLayout.ErrorEnabled = false;
                            _idEditText.RequestFocus();
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
            ThreadPool.QueueUserWorkItem(o => us.SignIn(_idEditText.Text, _passwordEditText.Text));
        }

        private void hideKeyboard()
        {
            var imm = GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private class IntuneUserService
        {
            Activity _activity;

            public IntuneUserService(Activity activity)
            {
                _activity = activity;
            }

            public void SignIn(string signInid, string password)
            {
                var rootView = _activity.FindViewById<View>(Resource.Id.loginRootLinearLayout);
                var user = IntuneService.SignIn(signInid, password);
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

