using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace Intune.Android
{
    public class AccountEntriesAdapter : BaseAdapter
    {
        List<Entry> _accountEntries;
        Activity _activity;
        UserAccountRole _role;

        public double TotalCreditQuantity { get; set; }
        public double TotalDebitQuantity { get; set; }
        public decimal TotalCreditAmount { get; set; }
        public decimal TotalDebitAmount { get; set; }

        public decimal BalanceAmount
        {
            get { return TotalCreditAmount - TotalDebitAmount; }
        }

        public double BalanceQuantity
        {
            get { return TotalCreditQuantity - TotalDebitQuantity; }
        }

        public AccountEntriesAdapter(Activity activity, int accountId, UserAccountRole role)
        {
            _activity = activity;
            _role = role;
            _accountEntries = IntuneService.GetAccountEntries(accountId);
            calculateTotals();
        }

        public override int Count
        {
            get
            {
                return _accountEntries.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            Entry accountEntry = _accountEntries[position];
            return new JavaObjectWrapper<Entry> { Obj = accountEntry };
        }

        public override long GetItemId(int position)
        {
            return _accountEntries[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ??
                _activity.LayoutInflater.Inflate(
                    Resource.Layout.AccountEntriesListItem, parent, false);

            var accountEntry = _accountEntries[position];

            var entryDate = view.FindViewById<TextView>(Resource.Id.entryDateTextView);
            entryDate.Text = accountEntry.TxnDate.ToString("dd-MMM-yyyy");

            var entryTxnType = view.FindViewById<TextView>(Resource.Id.entryTxnTypeTextView);
            entryTxnType.Text = getTxnType(accountEntry);

            var entryTxnQuantity = view.FindViewById<TextView>(Resource.Id.entryQuantityTextView);
            entryTxnQuantity.Text = accountEntry.Quantity.ToString("#0");

            var entryTxnAmount = view.FindViewById<TextView>(Resource.Id.entryAmountTextView);
            entryTxnAmount.Text = System.Math.Abs(accountEntry.Amount)
                                    .ToString("C2", CultureInfo.CurrentCulture);

            var entryNotes = view.FindViewById<TextView>(Resource.Id.entryNotesTextView);
            entryNotes.Text = accountEntry.Notes;

            //var commentIndicator = view.FindViewById<ImageView>(Resource.Id.accountEntryCommentIndicatorImageView);
            //commentIndicator.Visibility = accountEntry.HasUnreadComments || accountEntry.HasComments
            //                                ? ViewStates.Visible : ViewStates.Gone;

            //if (accountEntry.HasUnreadComments)
            //    commentIndicator.SetImageResource(Resource.Drawable.greendot);

            return view;
        }

        private string getTxnType(Entry entry)
        {
            if (_role != UserAccountRole.Collaborator)
                return entry.TxnType.ToString();

            if (entry.TxnType == TxnType.Paid || entry.TxnType == TxnType.Issued)
                return TxnType.Received.ToString();

            if (entry.Amount >= 0)
                return TxnType.Paid.ToString();
            else
                return TxnType.Issued.ToString();
        }

        private void calculateTotals()
        {
            foreach (var entry in _accountEntries)
            {
                if (entry.TxnType == TxnType.Paid || entry.TxnType == TxnType.Issued)
                {
                    TotalCreditAmount += entry.Amount;
                    TotalCreditQuantity += entry.Quantity;
                }
                else
                {
                    TotalDebitAmount += entry.Amount;
                    TotalDebitQuantity += entry.Quantity;
                }
            }
        }
    }
}