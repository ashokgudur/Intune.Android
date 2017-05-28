using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using System.Globalization;

namespace Intune.Android
{
    [Activity(Label = "Account Entry - Intune")]
    public class AccountEntryActivity : Activity
    {
        Entry _entry = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AccountEntry);

            var entryId = Intent.GetIntExtra("EntryId", 0);
            _entry = IntuneService.GetAccountEntry(entryId);

            var accountName = Intent.GetStringExtra("AccountName");
            if (_entry.Id == 0)
                this.Title = string.Format("{0} - New Entry", accountName);
            else
                this.Title = string.Format("{0} - Entry", accountName);

            fillForm();

            var okButton = FindViewById<Button>(Resource.Id.entryOkButton);
            okButton.Click += OkButton_Click;
            okButton.Enabled = _entry.Id == 0;

            var newButton = FindViewById<Button>(Resource.Id.entryNewButton);
            newButton.Click += NewButton_Click; ;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.entry_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var voidMenuItem = menu.FindItem(Resource.Id.entry_menu_void);
            var enableVloidMenuItem = _entry.Id != 0 && _entry.VoidId == 0;
            voidMenuItem.SetEnabled(enableVloidMenuItem);

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.entry_menu_void:
                    voidCurrentEntry();
                    break;
                case Resource.Id.entry_menu_comment:
                //showAccountsActivity();
                //break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void voidCurrentEntry()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Confirm void");
            alert.SetMessage("Please confirm that you want to void this entry.");
            alert.SetPositiveButton("OK", (sender, args) => { voidEntry(); Finish(); });
            alert.SetNegativeButton("Cancel", (sender, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void voidEntry()
        {
            var voidEntry = new Entry
            {
                UserId = Intent.GetIntExtra("LoginUserId", 0),
                AccountId = _entry.AccountId,
                Notes = composeVoidNotes(),
                TxnType = makeVoidTxnType(),
                TxnDate = DateTime.Today,
                Quantity = _entry.Quantity,
                Amount = _entry.Amount,
                VoidId = _entry.Id,
            };

            IntuneService.AddAccountEntry(voidEntry);
        }

        private TxnType makeVoidTxnType()
        {
            if (_entry.TxnType == TxnType.Paid || _entry.TxnType == TxnType.Issued)
                return TxnType.Received;
            else if (_entry.Amount > 0)
                return TxnType.Paid;
            else
                return TxnType.Issued;
        }

        private string composeVoidNotes()
        {
            return string.Format("Void of {0} on {1} of Qty: {2} and {3}",
                _entry.Notes, _entry.TxnDate.ToShortDateString(), _entry.Quantity,
                _entry.Amount.ToString("C2", CultureInfo.CurrentCulture));
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            _entry = new Entry();
            fillForm();
            var okButton = FindViewById<Button>(Resource.Id.entryOkButton);
            okButton.Enabled = _entry.Id == 0;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var entryDate = FindViewById<EditText>(Resource.Id.entryDateEditText);
            var entryTxnType = FindViewById<RadioGroup>(Resource.Id.entryTxnTypeRadioGroup);
            var entryQuantity = FindViewById<EditText>(Resource.Id.entryQuantityEditText);
            var entryAmount = FindViewById<EditText>(Resource.Id.entryAmountEditText);
            var entryNotes = FindViewById<EditText>(Resource.Id.entryNotesEditText);

            _entry.UserId = Intent.GetIntExtra("LoginUserId", 0);
            _entry.AccountId = Intent.GetIntExtra("AccountId", 0);
            var culture = new System.Globalization.CultureInfo("en-IN", true);
            _entry.TxnDate = Convert.ToDateTime(entryDate.Text, culture);
            _entry.TxnType = getTxnType(entryTxnType.CheckedRadioButtonId);
            _entry.Quantity = entryQuantity.Text.Trim() == "" ? 0 : double.Parse(entryQuantity.Text);
            _entry.Amount = entryAmount.Text.Trim() == "" ? 0 : decimal.Parse(entryAmount.Text);
            _entry.Notes = entryNotes.Text;
            _entry.VoidId = 0;

            var result = FindViewById<TextView>(Resource.Id.entryResultTextView);
            result.Text = "Adding new entry...";
            _entry = IntuneService.AddAccountEntry(_entry);

            if (_entry != null)
            {
                result.Text = string.Format("{0} entry saved.", _entry.Notes);
                return;
            }

            result.Text = "Entry saving FAILED!";
        }

        private TxnType getTxnType(int checkedRadioButtonId)
        {
            var radioButton = FindViewById<RadioButton>(checkedRadioButtonId);
            if (radioButton.Id == Resource.Id.entryTxnTypePaidRadioButton)
                return TxnType.Paid;
            else if (radioButton.Id == Resource.Id.entryTxnTypeIssuedRadioButton)
                return TxnType.Issued;
            else if (radioButton.Id == Resource.Id.entryTxnTypeReceviedRadioButton)
                return TxnType.Received;
            else
                throw new Exception("Invalid txn type value");
        }

        private void fillForm()
        {
            var entryDate = FindViewById<EditText>(Resource.Id.entryDateEditText);
            entryDate.Text = _entry.TxnDate.ToString("dd/MM/yyyy");

            if (_entry.TxnType == TxnType.Paid)
            {
                var entryTxnTypePaid = FindViewById<RadioButton>(Resource.Id.entryTxnTypePaidRadioButton);
                entryTxnTypePaid.Checked = true;
            }
            else if (_entry.TxnType == TxnType.Issued)
            {
                var entryTxnTypeIssued = FindViewById<RadioButton>(Resource.Id.entryTxnTypeIssuedRadioButton);
                entryTxnTypeIssued.Checked = true;
            }
            else if (_entry.TxnType == TxnType.Received)
            {
                var entryTxnTypeReceived = FindViewById<RadioButton>(Resource.Id.entryTxnTypeReceviedRadioButton);
                entryTxnTypeReceived.Checked = true;
            }

            var entryQuantity = FindViewById<EditText>(Resource.Id.entryQuantityEditText);
            entryQuantity.Text = _entry.Quantity.ToString();

            var entryAmount = FindViewById<EditText>(Resource.Id.entryAmountEditText);
            entryAmount.Text = _entry.Amount.ToString();

            var entryNotes = FindViewById<EditText>(Resource.Id.entryNotesEditText);
            entryNotes.Text = _entry.Notes;
        }
    }
}
