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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Accounts);

            var loginUserName = Intent.GetStringExtra("LoginUserName");
            this.Title = string.Format("{0} - Accounts", loginUserName);

            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountsAdapter = new AccountsAdapter(this, loginUserId);
            var accountsListView = FindViewById<ListView>(Resource.Id.accountsListView);
            accountsListView.Adapter = accountsAdapter;
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
                case Resource.Id.accounts_menu_contacts:
                    showContactsActivity();
                    break;
                case Resource.Id.accounts_menu_comment:
                case Resource.Id.accounts_menu_share:
                case Resource.Id.accounts_menu_open:
                case Resource.Id.accounts_menu_new:
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
    }
}