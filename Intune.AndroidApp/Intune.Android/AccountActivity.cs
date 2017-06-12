using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using Android.Content;

namespace Intune.Android
{
    [Activity(Label = "Account - Intune")]
    public class AccountActivity : Activity
    {
        Account _account = null;
        List<Contact> _contacts;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Account);

            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountName = Intent.GetStringExtra("AccountName");
            _account = new Account { Id = accountId, Name = accountName, UserId = loginUserId };

            if (_account.IsNew)
                Title = "New Account - Intune";
            else
                Title = string.Format("{0} - Intune", _account.Name);

            fillForm();
            _contacts = IntuneService.GetAccountSharedContacts(loginUserId, accountId);
            loadContacts();

            var okButton = FindViewById<Button>(Resource.Id.accountOkButton);
            okButton.Click += OkButton_Click;
        }

        private void loadContacts()
        {
            var accountShareToContactsAdapter = new AccountShareAdapter(this, _contacts);
            var contactsListView = FindViewById<ListView>(Resource.Id.accountSharedWithContactsListView);
            contactsListView.Adapter = accountShareToContactsAdapter;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.account_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var commentMenuItem = menu.FindItem(Resource.Id.account_menu_comment);
            commentMenuItem.SetEnabled(!_account.IsNew);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.account_menu_comment:
                    showMessageBoardActivity();
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

            if (_account.IsNew)
            {
                _account.UserId = Intent.GetIntExtra("LoginUserId", 0);
                _account.AddedOn = DateTime.Now;
            }

            if (_account.IsNew)
            {
                result.Text = "Adding new account...";
                _account = IntuneService.AddAccount(_account);
            }
            else
            {
                result.Text = "Updating account...";
                IntuneService.UpdateAccount(_account);
            }

            var accountShares = new List<UserAccountShareRole>();
            var sharedUsers = _contacts.Where(c => c.ContactUserId > 0 &&
                                c.AccountSharedRole != UserAccountRole.Owner).ToArray();
            foreach (var sharedUser in sharedUsers)
                accountShares.Add(
                    new UserAccountShareRole
                    {
                        UserId = sharedUser.ContactUserId,
                        Role = sharedUser.AccountSharedRole
                    });

            IntuneService.AddAccountSharing(_account.Id, accountShares.ToArray());
            result.Text = string.Format("Account {0} saved", _account.Name);
            Finish();
        }

        private void fillForm()
        {
            var fullName = FindViewById<EditText>(Resource.Id.accountNameEditText);
            fullName.Text = _account.Name;
        }

        private void showMessageBoardActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var loginUserName = Intent.GetStringExtra("LoginUserName");
            var messageBoardActivity = new Intent(this, typeof(ChatBoardActivity));
            messageBoardActivity.PutExtra("ByUserId", loginUserId);
            messageBoardActivity.PutExtra("AccountId", _account.Id);
            messageBoardActivity.PutExtra("AccountName", _account.Name);
            StartActivity(messageBoardActivity);
        }
    }
}
