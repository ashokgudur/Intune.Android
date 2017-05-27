using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;

namespace Intune.Android
{
    [Activity(Label = "New Entry - Intune")]
    public class AccountEntryActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AccountEntry);

            var accountName = Intent.GetStringExtra("AccountName");
            this.Title = string.Format("{0} - New Entry", accountName);

            var okButton = FindViewById<Button>(Resource.Id.entryOkButton);
            okButton.Click += OkButton_Click; ;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.entry_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.entry_menu_void:
                    //showAccountsActivity();
                    //break;
                case Resource.Id.entry_menu_comment:
                    //showAccountsActivity();
                    //break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }


        private void OkButton_Click(object sender, EventArgs e)
        {
            var entryDate = FindViewById<EditText>(Resource.Id.entryDateEditText);
            var entryTxnType = FindViewById<RadioGroup>(Resource.Id.entryTxnTypeRadioGroup);
            var entryQuantity = FindViewById<EditText>(Resource.Id.entryQuantityEditText);
            var entryAmount = FindViewById<EditText>(Resource.Id.entryAmountEditText);
            var entryNotes = FindViewById<EditText>(Resource.Id.entryNotesEditText);

            var entry = new Entry
            {
                UserId = Intent.GetIntExtra("LoginUserId", 0),
                AccountId = Intent.GetIntExtra("AccountId", 0),
                TxnDate = DateTime.Parse(entryDate.Text),
                TxnType = getTxnType(entryTxnType.CheckedRadioButtonId),
                Quantity = entryQuantity.Text.Trim() == "" ? 0 : double.Parse(entryQuantity.Text),
                Amount = entryAmount.Text.Trim() == "" ? 0 : decimal.Parse(entryAmount.Text),
                Notes = entryNotes.Text,
                VoidId = 0
            };

            var result = FindViewById<TextView>(Resource.Id.entryResultTextView);
            result.Text = "Adding new entry...";
            entry = IntuneService.AddAccountEntry(entry);

            if (entry != null)
            {
                result.Text = string.Format("{0} created.", entry.Notes);
                clearForm();
                return;
            }

            result.Text = "Account Txn entry FAILED!";
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

        private void clearForm()
        {
            var entryDate = FindViewById<EditText>(Resource.Id.entryDateEditText);
            entryDate.Text = "";

            var entryTxnTypePaid = FindViewById<RadioButton>(Resource.Id.entryTxnTypePaidRadioButton);
            entryTxnTypePaid.Checked = true;

            var entryQuantity = FindViewById<EditText>(Resource.Id.entryQuantityEditText);
            entryQuantity.Text = "";

            var entryAmount = FindViewById<EditText>(Resource.Id.entryAmountEditText);
            entryAmount.Text = "";

            var entryNotes = FindViewById<EditText>(Resource.Id.entryNotesEditText);
            entryNotes.Text = "";
        }
    }
}
