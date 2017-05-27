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
            //contactsListView.ChoiceMode = ChoiceMode.Multiple;
            contactsListView.SetItemChecked(1, true);
            contactsListView.SetItemChecked(2, true);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_mennus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_refresh:
                case Resource.Id.menu_contacts:
                case Resource.Id.menu_comment:
                case Resource.Id.menu_share_account:
                case Resource.Id.menu_open:
                case Resource.Id.menu_new:
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}