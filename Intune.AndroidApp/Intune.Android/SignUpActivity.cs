using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Support.Design.Widget;
using System.Text.RegularExpressions;
using Android.Util;
using Android.Views.InputMethods;
using Android.Content;
using System.Threading;

namespace Intune.Android
{
    [Activity(Label = "Intune - Sign up")]
    public class SignUpActivity : Activity
    {
        View _rootView;
        TextInputLayout _signUpFullNameTextInputLayout;
        TextInputLayout _signUpIdTextInputLayout;
        TextInputLayout _signUpPasswordTextInputLayout;
        TextInputEditText _signUpFullNameTextInputEditText;
        TextInputEditText _signUpIdTextInputEditText;
        TextInputEditText _signUpPasswordTextInputEditText;

        LinearLayout _signUpVerifyCodeLinearLayout;
        TextInputLayout _signUpVerifyCodeTextInputLayout;
        TextInputEditText _signUpVerifyCodeTextInputEditText;
        Button _signUpVerifyOtpButton;
        Button _signUpButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.SignInTheme);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SignUp);

            _rootView = FindViewById<View>(Resource.Id.signUpRootLinearLayout);

            _signUpFullNameTextInputLayout = FindViewById<TextInputLayout>(Resource.Id.signUpFullNameTextInputLayout);
            _signUpIdTextInputLayout = FindViewById<TextInputLayout>(Resource.Id.signUpIdTextInputLayout);
            _signUpPasswordTextInputLayout = FindViewById<TextInputLayout>(Resource.Id.signUpPasswordTextInputLayout);

            _signUpFullNameTextInputEditText = FindViewById<TextInputEditText>(Resource.Id.signUpFullNameTextInputEditText);
            _signUpIdTextInputEditText = FindViewById<TextInputEditText>(Resource.Id.signUpIdTextInputEditText);
            _signUpPasswordTextInputEditText = FindViewById<TextInputEditText>(Resource.Id.signUpPasswordTextInputEditText);

            _signUpVerifyCodeLinearLayout = FindViewById<LinearLayout>(Resource.Id.signUpVerifyCodeTextInputLayout);
            _signUpVerifyCodeLinearLayout.Visibility = ViewStates.Invisible;

            _signUpVerifyCodeTextInputLayout = FindViewById<TextInputLayout>(Resource.Id.signUpVerifyCodeTextInputLayout);
            _signUpVerifyCodeTextInputEditText = FindViewById<TextInputEditText>(Resource.Id.signUpVerifyCodeTextInputEditText);

            _signUpButton = FindViewById<Button>(Resource.Id.signUpButton);
            _signUpButton.Click += SignUpButton_Click;

            _signUpVerifyOtpButton = FindViewById<Button>(Resource.Id.signUpVerifyOtpButton);
            _signUpVerifyOtpButton.Click += SignUpVerifyOtpButton_Click; ;
            _signUpVerifyOtpButton.Visibility = ViewStates.Invisible;
        }

        private void hideKeyboard()
        {
            var imm = GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private void SignUpButton_Click(object sender, EventArgs e)
        {
            hideKeyboard();

            var fullName = _signUpFullNameTextInputEditText.Text.Trim();
            _signUpFullNameTextInputLayout.ErrorEnabled = false;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                _signUpFullNameTextInputLayout.ErrorEnabled = true;
                _signUpFullNameTextInputLayout.Error = "Your name is required";
                Snackbar.Make(_rootView, "Please enter your full name", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _signUpFullNameTextInputLayout.ErrorEnabled = false;
                            _signUpFullNameTextInputEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            _signUpIdTextInputLayout.ErrorEnabled = false;
            if (string.IsNullOrWhiteSpace(getSignUpId()))
            {
                _signUpIdTextInputLayout.ErrorEnabled = true;
                _signUpIdTextInputLayout.Error = "Mobile or email is required";
                Snackbar.Make(_rootView, "Enter either mobile or email.", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _signUpIdTextInputLayout.ErrorEnabled = false;
                            _signUpIdTextInputEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            var password = _signUpPasswordTextInputEditText.Text.Trim();
            _signUpPasswordTextInputLayout.ErrorEnabled = false;
            const int passwordMinLength = 6;
            //TODO: password max length... 
            // infact all the fields in all forms should be max length checked.
            if (string.IsNullOrWhiteSpace(password) || password.Length < passwordMinLength)
            {
                _signUpPasswordTextInputLayout.ErrorEnabled = true;
                _signUpPasswordTextInputLayout.Error = "Password is required";
                Snackbar.Make(_rootView, "Enter a password with min. 6 chars", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _signUpPasswordTextInputLayout.ErrorEnabled = false;
                            _signUpPasswordTextInputEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            if ((isIdEmail() && Patterns.EmailAddress.Matcher(getSignUpId()).Matches()) || isMobileNumberValid())
            {
                _signUpButton.Enabled = false;
                Snackbar.Make(_rootView, "Sending verification code...", Snackbar.LengthIndefinite).Show();
                ThreadPool.QueueUserWorkItem(o => sendVerificationCode());
                return;
            }

            Snackbar.Make(_rootView, "Enter valid email or mobile number.", Snackbar.LengthLong)
                    .SetAction("OK", (v) =>
                    {
                        _signUpIdTextInputLayout.ErrorEnabled = false;
                        _signUpIdTextInputEditText.RequestFocus();
                    })
                    .Show();
        }

        private void sendVerificationCode()
        {
            try
            {
                if (isIdEmail())
                    IntuneService.SendEmailOtp(getSignUpId());
                else
                {
                    //TODO: need to check whether country code is prefixed or not
                    var countryIsdCode = "+91";
                    IntuneService.SendMobileOtp(countryIsdCode, getSignUpId());
                }

                Snackbar.Make(_rootView, "Verification code has been sent.", Snackbar.LengthIndefinite).Show();
                RunOnUiThread(() =>
                 {
                     _signUpVerifyCodeLinearLayout.Visibility = ViewStates.Visible;
                     _signUpVerifyOtpButton.Visibility = ViewStates.Visible;
                     _signUpVerifyCodeTextInputEditText.Text = "";
                     _signUpVerifyCodeTextInputEditText.RequestFocus();
                 });
            }
            catch (Exception ex)
            {
                _signUpButton.Enabled = true;
                Snackbar.Make(_rootView, $"Cannot send verification code. Error: {ex.Message}", Snackbar.LengthLong)
                        .SetAction("RETRY", (v) => { })
                        .Show();
            }
        }

        private string getSignUpId()
        {
            return _signUpIdTextInputEditText.Text.Trim().ToLower();
        }

        private bool isIdEmail()
        {
            var regex = new Regex("[A-Za-z.@]");
            return regex.IsMatch(getSignUpId());
        }

        private bool isMobileNumberValid()
        {
            return Regex.IsMatch(getSignUpId(), @"^[0-9]{10}$");
        }

        private void SignUpVerifyOtpButton_Click(object sender, EventArgs e)
        {
            hideKeyboard();

            var verificationCode = _signUpVerifyCodeTextInputEditText.Text.Trim();
            _signUpVerifyCodeTextInputLayout.ErrorEnabled = false;
            if (string.IsNullOrWhiteSpace(verificationCode))
            {
                _signUpVerifyCodeTextInputLayout.ErrorEnabled = true;
                _signUpVerifyCodeTextInputLayout.Error = "Verification code is required";
                Snackbar.Make(_rootView, "Enter verification code you've received.", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _signUpVerifyCodeTextInputLayout.ErrorEnabled = false;
                            _signUpVerifyCodeTextInputEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            Snackbar.Make(_rootView, "Verifying code...", Snackbar.LengthIndefinite).Show();
            ThreadPool.QueueUserWorkItem(o => verifyCode(verificationCode));
        }

        private void verifyCode(string verificationCode)
        {
            RunOnUiThread(() => { _signUpVerifyOtpButton.Enabled = false; });

            try
            {
                if (isIdEmail())
                    IntuneService.VerifyEmailOtp(getSignUpId(), verificationCode);
                else
                {
                    //TODO: Country ISD code...
                    var countryIsdCode = "+91";
                    IntuneService.VerifyMobileOtp(countryIsdCode, getSignUpId(), verificationCode);
                }

                Snackbar.Make(_rootView, "Registering new user into Intune...", Snackbar.LengthIndefinite).Show();
                ThreadPool.QueueUserWorkItem(o => registerUser());
            }
            catch (Exception)
            {
                RunOnUiThread(() =>
                {
                    _signUpVerifyOtpButton.Enabled = true;
                    _signUpVerifyCodeTextInputLayout.ErrorEnabled = true;
                    _signUpVerifyCodeTextInputLayout.Error = "Valid verification code is required";
                    Snackbar.Make(_rootView, "Invalid verification code entered.", Snackbar.LengthLong)
                            .SetAction("RETRY", (v) =>
                            {
                                _signUpVerifyCodeTextInputLayout.ErrorEnabled = false;
                                _signUpVerifyCodeTextInputEditText.RequestFocus();
                            })
                            .Show();
                });
            }
        }

        private void registerUser()
        {
            try
            {
                var user = new User
                {
                    Name = _signUpFullNameTextInputEditText.Text.Trim(),
                    Email = getSignUpId(),
                    Mobile = getMobileNumber(),
                    Password = _signUpPasswordTextInputEditText.Text.Trim(),
                    //AtUserName = getMobileNumber(),
                    CreatedOn = DateTime.Now, //TODO: UTC date time?
                };

                user = IntuneService.RegiterUser(user);
                if (user == null)
                {
                    Snackbar.Make(_rootView, "Cannot register user!", Snackbar.LengthLong)
                            .SetAction("RETRY", (v) => { }).Show();
                    RunOnUiThread(() => { _signUpVerifyOtpButton.Enabled = true; });
                }
                else
                {
                    Snackbar.Make(_rootView, $"{user.Name} is registered.", Snackbar.LengthIndefinite)
                            .SetAction("SIGN-IN", (v) => { }).Show();
                    Finish();
                }
            }
            catch (Exception)
            {
                Snackbar.Make(_rootView, "Cannot register user!", Snackbar.LengthLong)
                            .SetAction("RETRY", (v) => { }).Show();
                RunOnUiThread(() => { _signUpVerifyOtpButton.Enabled = true; });
                return;
            }
        }

        private string getMobileNumber()
        {
            //TODO: Dummy number for mobile when email is used
            if (isIdEmail())
                return "+910000000000";
            else
                //TODO: check whether country code exist in the mobile number
                return $"+91{getSignUpId()}";
        }
    }
}
