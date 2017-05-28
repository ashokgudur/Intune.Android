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

            var entriesListView = FindViewById<ListView>(Resource.Id.accountEntriesListView);
            entriesListView.ItemClick +=
                (object sender, AdapterView.ItemClickEventArgs e) =>
                {
                    var obj = entriesListView.GetItemAtPosition(e.Position);
                    var entry = ((JavaObjectWrapper<Entry>)obj).Obj;
                    showAccountEntryActivity(entry.Id);
                };
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
                    refreshList();
                    break;
                case Resource.Id.entries_menu_account_contacts:
                    //TODO: display account contacts...
                    showContactsActivity();
                    break;
                case Resource.Id.entries_menu_comment_account:
                case Resource.Id.entries_menu_share_account:
                case Resource.Id.entries_menu_edit_account:
                    showAccountActivity();
                    break;
                case Resource.Id.entries_menu_new_entry:
                    showAccountEntryActivity(0);
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
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountName = Intent.GetStringExtra("AccountName");
            var accountActivity = new Intent(this, typeof(AccountActivity));
            accountActivity.PutExtra("LoginUserId", loginUserId);
            accountActivity.PutExtra("AccountId", accountId);
            accountActivity.PutExtra("AccountName", accountName);
            StartActivity(accountActivity);
        }

        private void showAccountEntryActivity(int entryId)
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountName = Intent.GetStringExtra("AccountName");

            var accountEntryActivity = new Intent(this, typeof(AccountEntryActivity));
            accountEntryActivity.PutExtra("EntryId", entryId);
            accountEntryActivity.PutExtra("LoginUserId", loginUserId);
            accountEntryActivity.PutExtra("AccountId", accountId);
            accountEntryActivity.PutExtra("AccountName", accountName);
            StartActivity(accountEntryActivity);
        }
    }
}