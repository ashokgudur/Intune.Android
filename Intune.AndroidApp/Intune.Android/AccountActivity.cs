using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;

namespace Intune.Android
{
    [Activity(Label = "Account - Intune")]
    public class AccountActivity : Activity
    {
        Account _account = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Account);

            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountName = Intent.GetStringExtra("AccountName");
            _account = new Account { Id = accountId, Name = accountName, UserId = loginUserId };

            if (_account.Id == 0)
                this.Title = "New Account - Intune";
            else
                this.Title = string.Format("{0} - Intune", _account.Name);

            fillForm();

            loadContacts(UserAccountRole.Impersonator);

            var okButton = FindViewById<Button>(Resource.Id.accountOkButton);
            okButton.Click += OkButton_Click;
        }

        private void loadContacts(UserAccountRole ofRole)
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountShareToContactsAdapter = new AccountShareAdapter(this, loginUserId, accountId, ofRole);
            var contactsListView = FindViewById<ListView>(Resource.Id.accountSharedWithContactsListView);
            contactsListView.Adapter = accountShareToContactsAdapter;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.account_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.account_menu_contacts:
                    //TODO: display account contacts...?
                    break;
                case Resource.Id.account_menu_comment:
                    //showAccountCommentsActivity();
                    break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var result = FindViewById<TextView>(Resource.Id.accountResultTextView);
            var accountName = FindViewById<EditText>(Resource.Id.accountNameEditText);

            if (string.IsNullOrWhiteSpace(accountName.Text.Trim()))
            {
                result.Text = "Please enter account name";
                return;
            }

            _account.Name = accountName.Text;

            if (_account.Id == 0)
            {
                _account.UserId = Intent.GetIntExtra("LoginUserId", 0);
                _account.AddedOn = DateTime.Now;
            }

            if (_account.Id == 0)
            {
                result.Text = "Adding new account...";
                _account = IntuneService.AddAccount(_account);
            }
            else
            {
                result.Text = "Updating account...";
                IntuneService.UpdateAccount(_account);
            }

            result.Text = string.Format("Account {0} saved", _account.Name);
        }

        private void fillForm()
        {
            var fullName = FindViewById<EditText>(Resource.Id.accountNameEditText);
            fullName.Text = _account.Name;
        }
    }
}
