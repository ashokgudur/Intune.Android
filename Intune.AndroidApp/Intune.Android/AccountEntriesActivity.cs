using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Account Entries - Intune")]
    public class AccountEntriesActivity : Activity
    {
        AccountEntriesAdapter _accountsAdapter = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AccountEntries);

            var accountName = Intent.GetStringExtra("AccountName");
            this.Title = string.Format("{0} - Entries", accountName);

            refreshList();
        }

        private void refreshList()
        {
            var accountId = Intent.GetIntExtra("AccountId", 0);
            _accountsAdapter = new AccountEntriesAdapter(this, accountId);
            var accountsListView = FindViewById<ListView>(Resource.Id.accountEntriesListView);
            accountsListView.Adapter = _accountsAdapter;
        }

        protected override void OnResume()
        {
            refreshList();
            base.OnResume();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.entries_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.entries_menu_refresh:
                case Resource.Id.entries_menu_contacts:
                    showContactsActivity();
                    break;
                case Resource.Id.entries_menu_comment:
                case Resource.Id.entries_menu_share:
                case Resource.Id.entries_menu_void:
                case Resource.Id.entries_menu_new:
                    showAccountEntryActivity();
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

        private void showAccountEntryActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountName = Intent.GetStringExtra("AccountName");

            var accountEntryActivity = new Intent(this, typeof(AccountEntryActivity));
            accountEntryActivity.PutExtra("LoginUserId", loginUserId);
            accountEntryActivity.PutExtra("AccountId", accountId);
            accountEntryActivity.PutExtra("accountName", accountName);
            StartActivity(accountEntryActivity);
        }
    }
}