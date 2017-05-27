using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace Intune.Android
{
    public class AccountsAdapter : BaseAdapter
    {
        List<Account> _accounts;
        Activity _activity;
        int _userId;

        public AccountsAdapter(Activity activity, int userId)
        {
            _activity = activity;
            _userId = userId;
            _accounts = IntuneService.GetAllAccounts(_userId, 0);
        }

        public override int Count
        {
            get
            {
                return _accounts.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return _accounts[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ??
                _activity.LayoutInflater.Inflate(
                    Resource.Layout.AccountListItem, parent, false);
            var accountName = view.FindViewById<TextView>(Resource.Id.accountNameTextView);
            var accountBalance = view.FindViewById<TextView>(Resource.Id.balanceTextView);
            accountName.Text = _accounts[position].Name;
            accountBalance.Text = System.Math.Abs(_accounts[position].Balance)
                                    .ToString("C2", CultureInfo.CurrentCulture);

            return view;
        }
    }
}