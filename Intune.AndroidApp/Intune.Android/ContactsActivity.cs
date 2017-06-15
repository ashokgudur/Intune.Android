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

            var contactsListView = FindViewById<ListView>(Resource.Id.contactsListView);
            contactsListView.ItemClick +=
                (object sender, AdapterView.ItemClickEventArgs e) =>
                {
                    var obj = contactsListView.GetItemAtPosition(e.Position);
                    var contact = ((JavaObjectWrapper<Contact>)obj).Obj;
                    showContactActivity(contact.Id);
                };
        }

        private void refreshList()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var contactsAdapter = new ContactsAdapter(this, loginUserId);
            var contactsListView = FindViewById<ListView>(Resource.Id.contactsListView);
            contactsListView.Adapter = contactsAdapter;
        }

        protected override void OnResume()
        {
            base.OnResume();
            refreshList();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.contacts_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var userProfileMenuItem = menu.FindItem(Resource.Id.contacts_menu_profile);
            userProfileMenuItem.SetEnabled(false);

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.contacts_menu_refresh:
                    refreshList();
                    break;
                case Resource.Id.contacts_menu_profile:
                    showUserProfileActivity();
                    break;
                case Resource.Id.contacts_menu_accounts:
                    showAccountsActivity();
                    break;
                case Resource.Id.contacts_menu_new:
                    showContactActivity(0);
                    break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void showUserProfileActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var loginUserName = Intent.GetStringExtra("LoginUserName");
            var userProfileActivity = new Intent(this, typeof(UserProfileActivity));
            userProfileActivity.PutExtra("LoginUserId", loginUserId);
            userProfileActivity.PutExtra("LoginUserName", loginUserName);
            StartActivity(userProfileActivity);
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

        private void showContactActivity(int contactId)
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var loginUserName = Intent.GetStringExtra("LoginUserName");
            var contactActivity = new Intent(this, typeof(ContactActivity));
            contactActivity.PutExtra("LoginUserId", loginUserId);
            contactActivity.PutExtra("LoginUserName", loginUserName);
            contactActivity.PutExtra("ContactId", contactId);
            StartActivity(contactActivity);
        }
    }
}