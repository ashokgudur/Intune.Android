using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace Intune.Android
{
    public class JavaObjectWrapper<T> : Java.Lang.Object
    {
        public T Obj { get; set; }
    }

    public class AccountsAdapter : BaseAdapter
    {
        List<Account> _accounts;
        Activity _activity;

        public AccountsAdapter(Activity activity, int userId, int contactId)
        {
            _activity = activity;
            _accounts = IntuneService.GetAllAccounts(userId, contactId);
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
            Account account = _accounts[position];
            return new JavaObjectWrapper<Account> { Obj = account };
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

            var commentIndicator = view.FindViewById<ImageView>(Resource.Id.accountCommentIndicatorImageView);
            commentIndicator.Visibility = account.HasUnreadComments || account.HasComments
                                            ? ViewStates.Visible : ViewStates.Gone;

            if (account.HasUnreadComments)
                commentIndicator.SetImageResource(Resource.Drawable.greendot);

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