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

            var account = _accounts[position];

            var accountName = view.FindViewById<TextView>(Resource.Id.accountNameTextView);
            accountName.Text = account.Name;

            var accountBalance = view.FindViewById<TextView>(Resource.Id.accountBalanceTextView);
            accountBalance.Text = System.Math.Abs(account.Balance)
                                    .ToString("C2", CultureInfo.CurrentCulture);

            var accountPersmission = view.FindViewById<TextView>(Resource.Id.accountPermissionTextView);
            accountPersmission.Text = string.Format("You are {0}", account.Role.ToString());

            var txn = account.Balance == 0 ? account.HasEntries ? "++" : "NA"
                    : account.Balance > 0 ? getBalanceTitle(account, "Receivable") 
                                            : getBalanceTitle(account, "Payable");
            var accountTx = view.FindViewById<TextView>(Resource.Id.accountTxTextView);
            accountTx.Text = string.Format("Balance is {0}", txn);

            return view;
        }

        private string getBalanceTitle(Account account, string ofType)
        {
            if (account.Role == UserAccountRole.Collaborator)
                return ofType == "Receivable" ? "Payable" : "Receivable";
            else
                return ofType == "Receivable" ? "Receivable" : "Payable";
        }
    }
}