using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Accounts - Intune")]
    public class AccountsActivity : Activity
    {
        AccountsAdapter _accountsAdapter = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Accounts);

            var loginUserName = Intent.GetStringExtra("LoginUserName");
            this.Title = string.Format("{0} - Accounts", loginUserName);

            refreshList();
            var accountsListView = FindViewById<ListView>(Resource.Id.accountsListView);
            accountsListView.ItemClick +=
                (object sender, AdapterView.ItemClickEventArgs e) =>
                {
                    var obj = accountsListView.GetItemAtPosition(e.Position);
                    var account = ((JavaObjectWrapper<Account>)obj).Obj;
                    showAccountEntriesActivity(account);
                };
        }

        private void refreshList()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            _accountsAdapter = new AccountsAdapter(this, loginUserId);
            var accountsListView = FindViewById<ListView>(Resource.Id.accountsListView);
            accountsListView.Adapter = _accountsAdapter;
        }

        protected override void OnResume()
        {
            refreshList();
            base.OnResume();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.accounts_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.accounts_menu_refresh:
                    refreshList();
                    break;
                case Resource.Id.accounts_menu_contacts:
                    showContactsActivity();
                    break;
                case Resource.Id.accounts_menu_new:
                    showAccountActivity();
                    break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void showContactsActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var loginUserName = Intent.GetStringExtra("LoginUserName");
            var contactsActivity = new Intent(this, typeof(ContactsActivity));
            contactsActivity.PutExtra("LoginUserId", loginUserId);
            contactsActivity.PutExtra("LoginUserName", loginUserName);
            StartActivity(contactsActivity);
        }

        private void showAccountActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountActivity = new Intent(this, typeof(AccountActivity));
            accountActivity.PutExtra("LoginUserId", loginUserId);
            StartActivity(accountActivity);
        }

        private void showAccountEntriesActivity(Account account)
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountsEntryActivity = new Intent(this, typeof(AccountEntriesActivity));
            accountsEntryActivity.PutExtra("LoginUserId", loginUserId);
            accountsEntryActivity.PutExtra("AccountId", account.Id);
            accountsEntryActivity.PutExtra("AccountName", account.Name);
            StartActivity(accountsEntryActivity);
        }
    }
}