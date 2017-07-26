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
using Intune.ApiGateway;
using Intune.ApiGateway.Model;

namespace Intune.Android
{
    [Activity(Label = "Intune")]
    public class ResetPasswordActivity : Activity
    {
        View _rootView;

        TextInputLayout _signInIdTextInputLayout;
        TextInputEditText _signInIdTextInputEditText;
        Button _sendVerifyCodeButton;

        LinearLayout _verifyCodeLinearLayout;
        TextInputLayout _verifyCodeTextInputLayout;
        TextInputEditText _verifyCodeTextInputEditText;
        Button _verifyOtpButton;

        LinearLayout _resetPasswordLinearLayout;
        TextInputLayout _newPasswordTextInputLayout;
        TextInputEditText _newPasswordTextInputEditText;
        Button _resetPasswordButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.SignInTheme);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ResetPassword);

            _rootView = FindViewById<View>(Resource.Id.resetPasswordRootLinearLayout);

            _signInIdTextInputLayout = FindViewById<TextInputLayout>(Resource.Id.signInIdTextInputLayout);
            _signInIdTextInputEditText = FindViewById<TextInputEditText>(Resource.Id.signInIdTextInputEditText);
            _sendVerifyCodeButton = FindViewById<Button>(Resource.Id.resetPasswordSendVerifyCodeButton);
            _sendVerifyCodeButton.Click += sendVerifyCodeButton_Click;

            _verifyCodeLinearLayout = FindViewById<LinearLayout>(Resource.Id.resetPasswordVerifyCodeLinearLayout);
            _verifyCodeLinearLayout.Visibility = ViewStates.Invisible;
            _verifyCodeTextInputLayout = FindViewById<TextInputLayout>(Resource.Id.resetPasswordVerifyCodeTextInputLayout);
            _verifyCodeTextInputEditText = FindViewById<TextInputEditText>(Resource.Id.resetPasswordVerifyCodeTextInputEditText);
            _verifyOtpButton = FindViewById<Button>(Resource.Id.resetPasswordVerifyOtpButton);
            _verifyOtpButton.Click += verifyOtpButton_Click;
            _verifyOtpButton.Visibility = ViewStates.Invisible;

            _resetPasswordLinearLayout = FindViewById<LinearLayout>(Resource.Id.resetPasswordLinearLayout);
            _resetPasswordLinearLayout.Visibility = ViewStates.Invisible;
            _newPasswordTextInputLayout = FindViewById<TextInputLayout>(Resource.Id.resetNewPasswordTextInputLayout);
            _newPasswordTextInputEditText = FindViewById<TextInputEditText>(Resource.Id.resetNewPasswordTextInputEditText);
            _resetPasswordButton = FindViewById<Button>(Resource.Id.resetPasswordButton);
            _resetPasswordButton.Click += resetPasswordButton_Click; ;
            _resetPasswordButton.Visibility = ViewStates.Invisible;
        }

        private void hideKeyboard()
        {
            var imm = GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private void resetPasswordButton_Click(object sender, EventArgs e)
        {
            hideKeyboard();

            var password = _newPasswordTextInputEditText.Text.Trim();
            _newPasswordTextInputLayout.ErrorEnabled = false;
            const int passwordMinLength = 6;
            //TODO: password max length... 
            // infact all the fields in all forms should be max length checked.
            if (string.IsNullOrWhiteSpace(password) || password.Length < passwordMinLength)
            {
                _newPasswordTextInputLayout.ErrorEnabled = true;
                _newPasswordTextInputLayout.Error = "Password is required";
                Snackbar.Make(_rootView, "Enter a password with min. 6 chars", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _newPasswordTextInputLayout.ErrorEnabled = false;
                            _newPasswordTextInputEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            _resetPasswordButton.Enabled = false;
            Snackbar.Make(_rootView, "Resetting password...", Snackbar.LengthIndefinite).Show();
            ThreadPool.QueueUserWorkItem(o => resetPassword());
        }

        private void resetPassword()
        {
            var signInId = getSignInId();
            if (!isIdEmail())
            {
                var mnv = new MobileNumberValidator(getSignInId());
                signInId = mnv.GetFullMobileNumber();
            }

            var user = new User
            {
                Email = signInId,
                Password = _newPasswordTextInputEditText.Text.Trim(),
                SessionToken = _verifyCodeTextInputEditText.Text.Trim(),
            };

            try
            {
                IntuneService.ResetPassword(user);
                Snackbar.Make(_rootView, "Resetting password successful.", Snackbar.LengthIndefinite)
                        .SetAction("SIGN-IN", (v) => { Finish(); }).Show();
                Finish();
            }
            catch (Exception)
            {
                Snackbar.Make(_rootView, "Cannot reset the password!", Snackbar.LengthLong)
                        .SetAction("RETRY", (v) =>
                        {
                            _newPasswordTextInputLayout.ErrorEnabled = false;
                            _newPasswordTextInputEditText.RequestFocus();
                        }).Show();

                RunOnUiThread(() => { _resetPasswordButton.Enabled = true; });
            }
        }

        private void sendVerifyCodeButton_Click(object sender, EventArgs e)
        {
            hideKeyboard();

            _signInIdTextInputLayout.ErrorEnabled = false;
            if (string.IsNullOrWhiteSpace(getSignInId()))
            {
                _signInIdTextInputLayout.ErrorEnabled = true;
                _signInIdTextInputLayout.Error = "Mobile or email is required";
                Snackbar.Make(_rootView, "Enter either mobile or email.", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _signInIdTextInputLayout.ErrorEnabled = false;
                            _signInIdTextInputEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            var isIdValid = false;
            if (isIdEmail())
                isIdValid = Patterns.EmailAddress.Matcher(getSignInId()).Matches();
            else
                isIdValid = new MobileNumberValidator(getSignInId()).IsValid();

            if (isIdValid)
            {
                _sendVerifyCodeButton.Enabled = false;
                Snackbar.Make(_rootView, "Sending verification code...", Snackbar.LengthIndefinite).Show();
                ThreadPool.QueueUserWorkItem(o => sendVerificationCode());
                return;
            }

            Snackbar.Make(_rootView, "Enter valid email or mobile number.", Snackbar.LengthLong)
                    .SetAction("OK", (v) =>
                    {
                        _signInIdTextInputLayout.ErrorEnabled = false;
                        _signInIdTextInputEditText.RequestFocus();
                    })
                    .Show();
        }

        private void sendVerificationCode()
        {
            try
            {
                var user = IntuneService.GetUserBySignInId(getSignInId());
                if (user != null)
                {
                    if (isIdEmail())
                        IntuneService.SendEmailOtp(getSignInId());
                    else
                    {
                        var mnv = new MobileNumberValidator(getSignInId());
                        IntuneService.SendMobileOtp(mnv.GetIsdCodeWithoutPlus(), mnv.GetMobileNumberWithoutIsdCode());
                    }
                }

                Snackbar.Make(_rootView, "Verification code has been sent.", Snackbar.LengthIndefinite).Show();
                RunOnUiThread(() =>
                 {
                     _verifyCodeLinearLayout.Visibility = ViewStates.Visible;
                     _verifyOtpButton.Visibility = ViewStates.Visible;
                     _verifyCodeTextInputEditText.Text = "";
                     _verifyCodeTextInputEditText.RequestFocus();
                 });
            }
            catch (Exception ex)
            {
                RunOnUiThread(() => { _sendVerifyCodeButton.Enabled = true; });
                Snackbar.Make(_rootView, $"Cannot send verification code. Error: {ex.Message}", Snackbar.LengthLong)
                        .SetAction("RETRY", (v) => { })
                        .Show();
            }
        }

        private string getSignInId()
        {
            return _signInIdTextInputEditText.Text.Trim().ToLower();
        }

        private bool isIdEmail()
        {
            var regex = new Regex("[A-Za-z.@]");
            return regex.IsMatch(getSignInId());
        }

        private void verifyOtpButton_Click(object sender, EventArgs e)
        {
            hideKeyboard();

            var verificationCode = _verifyCodeTextInputEditText.Text.Trim();
            _verifyCodeTextInputLayout.ErrorEnabled = false;
            if (string.IsNullOrWhiteSpace(verificationCode))
            {
                _verifyCodeTextInputLayout.ErrorEnabled = true;
                _verifyCodeTextInputLayout.Error = "Verification code is required";
                Snackbar.Make(_rootView, "Enter verification code you've received.", Snackbar.LengthLong)
                        .SetAction("OK", (v) =>
                        {
                            _verifyCodeTextInputLayout.ErrorEnabled = false;
                            _verifyCodeTextInputEditText.RequestFocus();
                        })
                        .Show();
                return;
            }

            Snackbar.Make(_rootView, "Verifying code...", Snackbar.LengthIndefinite).Show();
            ThreadPool.QueueUserWorkItem(o => verifyCode(verificationCode));
        }

        private void verifyCode(string verificationCode)
        {
            RunOnUiThread(() => { _verifyOtpButton.Enabled = false; });

            try
            {
                if (isIdEmail())
                    IntuneService.VerifyEmailOtp(getSignInId(), verificationCode);
                else
                {
                    var mnv = new MobileNumberValidator(getSignInId());
                    IntuneService.VerifyMobileOtp(mnv.GetIsdCodeWithoutPlus(), mnv.GetMobileNumberWithoutIsdCode(), verificationCode);
                }

                Snackbar.Make(_rootView, "Verification OK. Please enter new password.", Snackbar.LengthIndefinite).Show();
                RunOnUiThread(() =>
                {
                    _resetPasswordLinearLayout.Visibility = ViewStates.Visible;
                    _newPasswordTextInputLayout.Visibility = ViewStates.Visible;
                    _resetPasswordButton.Visibility = ViewStates.Visible;
                    _newPasswordTextInputEditText.Text = "";
                    _newPasswordTextInputEditText.RequestFocus();
                });
            }
            catch (Exception)
            {
                RunOnUiThread(() =>
                {
                    _verifyOtpButton.Enabled = true;
                    _verifyCodeTextInputLayout.ErrorEnabled = true;
                    _verifyCodeTextInputLayout.Error = "Valid verification code is required";
                    Snackbar.Make(_rootView, "Invalid verification code entered.", Snackbar.LengthLong)
                            .SetAction("RETRY", (v) =>
                            {
                                _verifyCodeTextInputLayout.ErrorEnabled = false;
                                _verifyCodeTextInputEditText.RequestFocus();
                            })
                            .Show();
                });
            }
        }
    }
}
