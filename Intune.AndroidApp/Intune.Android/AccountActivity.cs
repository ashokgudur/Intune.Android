using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Account - Intune")]
    public class AccountActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Contact);

            var okButton = FindViewById<Button>(Resource.Id.accountOkButton);
            okButton.Click += OkButton_Click; ;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var accountName = FindViewById<EditText>(Resource.Id.accountNameEditText);

            var account = new Account
            {
                Name = accountName.Text,
                UserId = Intent.GetIntExtra("LoginUserId", 0),
                AddedOn = DateTime.Now
            };

            var result = FindViewById<TextView>(Resource.Id.accountResultTextView);
            result.Text = "Adding new account...";
            account = IntuneService.AddAccount(account);

            if (account != null)
            {
                result.Text = string.Format("Account {0} created.", account.Name);
                clearForm();
                return;
            }

            result.Text = "Account creation FAILED!";
        }

        private void clearForm()
        {
            var fullName = FindViewById<EditText>(Resource.Id.accountNameEditText);
            fullName.Text = "";
        }
    }
}
