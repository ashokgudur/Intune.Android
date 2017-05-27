using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Accounts - Intune")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            var loginUserName = Intent.GetStringExtra("LoginUserName");
            this.Title = string.Format("{0} - Accounts", loginUserName);

            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var accountAdapter = new AccountsAdapter(this, loginUserId);
            var accountsListView = FindViewById<ListView>(Resource.Id.accountsListView);
            accountsListView.Adapter = accountAdapter;
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