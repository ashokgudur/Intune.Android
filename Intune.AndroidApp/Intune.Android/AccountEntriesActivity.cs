using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Intune.ApiGateway.Model;
using System.Globalization;

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
            Title = string.Format("{0} - Entries", accountName);

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
            _accountsAdapter = new AccountEntriesAdapter(this, accountId, getAccountRole());
            var accountsListView = FindViewById<ListView>(Resource.Id.accountEntriesListView);
            accountsListView.Adapter = _accountsAdapter;

            displayTotals();
        }

        private void displayTotals()
        {
            var totalPaid = FindViewById<TextView>(Resource.Id.totalPaidTextView);
            totalPaid.Text = string.Format("TOTAL {0}", getTotalsTxnType("Paid"));

            var totalPaidQty = FindViewById<TextView>(Resource.Id.totalPaidQtyTextView);
            totalPaidQty.Text = _accountsAdapter.TotalCreditQuantity.ToString("#0");

            var totalPaidAmount = FindViewById<TextView>(Resource.Id.totalPaidAmountTextView);
            totalPaidAmount.Text = System.Math.Abs(_accountsAdapter.TotalCreditAmount)
                                    .ToString("C2", CultureInfo.CurrentCulture);

            var totalReceived = FindViewById<TextView>(Resource.Id.totalReceivedTextView);
            totalReceived.Text = string.Format("TOTAL {0}", getTotalsTxnType("Rcvd"));

            var totalReceivedQty = FindViewById<TextView>(Resource.Id.totalReceivedQtyTextView);
            totalReceivedQty.Text = _accountsAdapter.TotalDebitQuantity.ToString("#0");

            var totalReceivedAmount = FindViewById<TextView>(Resource.Id.totalReceivedAmountTextView);
            totalReceivedAmount.Text = System.Math.Abs(_accountsAdapter.TotalDebitAmount)
                                    .ToString("C2", CultureInfo.CurrentCulture);

            var balanceTitle = _accountsAdapter.BalanceAmount == 0
                                ? "Zero"
                                : _accountsAdapter.BalanceAmount > 0
                                ? getBalanceTitle("Rcvbl") : getBalanceTitle("Paybl");

            var totalBalance = FindViewById<TextView>(Resource.Id.totalBalanceTextView);
            totalBalance.Text = string.Format("TOTAL {0}", balanceTitle);

            var totalBalanceQty = FindViewById<TextView>(Resource.Id.totalBalanceQtyTextView);
            totalBalanceQty.Text = _accountsAdapter.BalanceQuantity.ToString("#0");

            var totalBalanceAmount = FindViewById<TextView>(Resource.Id.totalBalanceAmountTextView);
            totalBalanceAmount.Text = System.Math.Abs(_accountsAdapter.BalanceAmount)
                                    .ToString("C2", CultureInfo.CurrentCulture);
        }

        private UserAccountRole getAccountRole()
        {
            var accountRole = Intent.GetIntExtra("AccountRole", 0);
            return (UserAccountRole)accountRole;
        }

        private string getTotalsTxnType(string ofType)
        {
            if (getAccountRole() == UserAccountRole.Collaborator)
                return ofType == "Paid" ? "RECEIVED" : "PAID";
            else
                return ofType == "Paid" ? "PAID" : "RECEIVED";
        }

        private string getBalanceTitle(string ofType)
        {
            if (getAccountRole() == UserAccountRole.Collaborator)
                return ofType == "Rcvbl" ? "PAYABLE" : "RECEIVABLE";
            else
                return ofType == "Rcvbl" ? "RECEIVABLE" : "PAYABLE";
        }

        protected override void OnResume()
        {
            base.OnResume();
            refreshList();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.entries_menus, menu);
            var editAccountMenuItem = menu.FindItem(Resource.Id.entries_menu_edit_account);
            var newEntryMenuItem = menu.FindItem(Resource.Id.entries_menu_new_entry);
            var role = (UserAccountRole)Intent.GetIntExtra("AccountRole", 0);
            editAccountMenuItem.SetVisible(role == UserAccountRole.Owner);
            var canAddEntries = role == UserAccountRole.Owner || role == UserAccountRole.Impersonator;
            newEntryMenuItem.SetVisible(canAddEntries);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.entries_menu_refresh:
                    refreshList();
                    break;
                case Resource.Id.entries_menu_comment_account:
                    showMessageBoardActivity();
                    break;
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

        private void showMessageBoardActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var loginUserName = Intent.GetStringExtra("LoginUserName");
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountName = Intent.GetStringExtra("AccountName");
            var messageBoardActivity = new Intent(this, typeof(ChatboardActivity));
            messageBoardActivity.PutExtra("ByUserId", loginUserId);
            messageBoardActivity.PutExtra("AccountId", accountId);
            messageBoardActivity.PutExtra("AccountName", accountName);
            StartActivity(messageBoardActivity);
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
            var accountRole = Intent.GetIntExtra("AccountRole", 0);

            var accountEntryActivity = new Intent(this, typeof(AccountEntryActivity));
            accountEntryActivity.PutExtra("EntryId", entryId);
            accountEntryActivity.PutExtra("LoginUserId", loginUserId);
            accountEntryActivity.PutExtra("AccountId", accountId);
            accountEntryActivity.PutExtra("AccountName", accountName);
            accountEntryActivity.PutExtra("AccountRole", accountRole);
            StartActivity(accountEntryActivity);
        }
    }
}