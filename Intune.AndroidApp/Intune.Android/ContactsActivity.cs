using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Contacts - Intune")]
    public class ContactsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Contacts);

            var loginUserName = Intent.GetStringExtra("LoginUserName");
            this.Title = string.Format("{0} - Contacts", loginUserName);

            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var contactsAdapter = new ContactsAdapter(this, loginUserId);
            var contactsListView = FindViewById<ListView>(Resource.Id.contactsListView);
            contactsListView.Adapter = contactsAdapter;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.contacts_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.contacts_menu_refresh:
                case Resource.Id.contacts_menu_accounts:
                    showAccountsActivity();
                    break;
                case Resource.Id.contacts_menu_comment:
                case Resource.Id.contacts_menu_share:
                case Resource.Id.contacts_menu_open:
                case Resource.Id.contacts_menu_new:
                    showContactActivity();
                    break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void showAccountsActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var loginUserName = Intent.GetStringExtra("LoginUserName");
            var accountsActivity = new Intent(this, typeof(AccountsActivity));
            accountsActivity.PutExtra("LoginUserId", loginUserId);
            accountsActivity.PutExtra("LoginUserName", loginUserName);
            StartActivity(accountsActivity);
        }

        private void showContactActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var contactActivity = new Intent(this, typeof(ContactActivity));
            contactActivity.PutExtra("LoginUserId", loginUserId);
            StartActivity(contactActivity);
        }
    }
}