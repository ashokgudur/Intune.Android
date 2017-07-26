using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using System.Globalization;
using Android.Content;
using Android.Views.InputMethods;
using Android.Support.Design.Widget;
using System.Threading;
using Intune.ApiGateway;
using Intune.ApiGateway.Model;

namespace Intune.Android
{
    [Activity(Label = "Account Entry - Intune")]
    public class AccountEntryActivity : Activity
    {
        View _rootView;
        EditText _entryDate;
        RadioGroup _entryTxnType;
        EditText _entryQuantity;
        EditText _entryAmount;
        EditText _entryNotes;
        Button _okButton;
        Entry _entry = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AccountEntry);

            _rootView = FindViewById<View>(Resource.Id.accountEntryRootLinearLayout);
            _entryDate = FindViewById<EditText>(Resource.Id.entryDateEditText);
            _entryTxnType = FindViewById<RadioGroup>(Resource.Id.entryTxnTypeRadioGroup);
            _entryQuantity = FindViewById<EditText>(Resource.Id.entryQuantityEditText);
            _entryAmount = FindViewById<EditText>(Resource.Id.entryAmountEditText);
            _entryNotes = FindViewById<EditText>(Resource.Id.entryNotesEditText);

            var entryId = Intent.GetIntExtra("EntryId", 0);
            _entry = IntuneService.GetAccountEntry(entryId);

            var accountName = Intent.GetStringExtra("AccountName");
            if (_entry.IsNew)
                Title = string.Format("{0} - New Entry", accountName);
            else
                Title = string.Format("{0} - Entry", accountName);

            fillForm();

            var entryDatePicker = FindViewById<ImageButton>(Resource.Id.entryDatePickerImageButton);
            entryDatePicker.Click += EntryDatePicker_Click; ;

            _okButton = FindViewById<Button>(Resource.Id.entryOkButton);
            _okButton.Click += OkButton_Click;
            _okButton.Enabled = _entry.IsNew;

            var newButton = FindViewById<Button>(Resource.Id.entryNewButton);
            if (userCanAddAndVoidEntries())
                newButton.Click += NewButton_Click;
            else
                newButton.Visibility = ViewStates.Gone;
        }

        private void EntryDatePicker_Click(object sender, EventArgs e)
        {
            var culture = new CultureInfo("en-IN", true);
            var txnDate = DateTime.Today;
            DateTime.TryParse(_entryDate.Text, culture, DateTimeStyles.AllowWhiteSpaces, out txnDate);

            var datePickerFragment = DatePickerFragment.NewInstance(txnDate, delegate (DateTime date)
            {
                _entryDate.Text = date.ToString("dd-MM-yyyy");
            });

            datePickerFragment.Show(FragmentManager, DatePickerFragment.TAG);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.entry_menus, menu);

            var voidMenuItem = menu.FindItem(Resource.Id.entry_menu_void);
            voidMenuItem.SetVisible(userCanAddAndVoidEntries());

            return base.OnCreateOptionsMenu(menu);
        }

        private bool userCanAddAndVoidEntries()
        {
            var role = (UserAccountRole)Intent.GetIntExtra("AccountRole", 0);
            return role == UserAccountRole.Owner || role == UserAccountRole.Impersonator;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var voidMenuItem = menu.FindItem(Resource.Id.entry_menu_void);
            var enableVloidMenuItem = !_entry.IsNew && !_entry.IsVoid;
            voidMenuItem.SetEnabled(enableVloidMenuItem);

            var commentMenuItem = menu.FindItem(Resource.Id.entry_menu_comment);
            commentMenuItem.SetEnabled(!_entry.IsNew);

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
                    showMessageBoardActivity();
                    break;
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

            ThreadPool.QueueUserWorkItem(o => saveEntry(voidEntry));
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
            _okButton.Enabled = _entry.IsNew;
        }

        private void hideKeyboard()
        {
            var imm = GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            hideKeyboard();

            _entry.UserId = Intent.GetIntExtra("LoginUserId", 0);
            _entry.AccountId = Intent.GetIntExtra("AccountId", 0);
            var culture = new CultureInfo("en-IN", true);
            DateTime txnDate;
            if (!DateTime.TryParse(_entryDate.Text, culture, DateTimeStyles.AllowWhiteSpaces, out txnDate))
            {
                Snackbar.Make(_rootView, "Invalid date entered.", Snackbar.LengthLong).Show();
                return;
            }

            _entry.TxnDate = txnDate;
            _entry.TxnType = getTxnType(_entryTxnType.CheckedRadioButtonId);
            _entry.Quantity = _entryQuantity.Text.Trim() == "" ? 0 : double.Parse(_entryQuantity.Text);
            _entry.Amount = _entryAmount.Text.Trim() == "" ? 0 : decimal.Parse(_entryAmount.Text);
            _entry.Notes = _entryNotes.Text;
            _entry.VoidId = 0;

            if (!_entry.IsValid())
            {
                Snackbar.Make(_rootView, "Please enter all the details", Snackbar.LengthLong).Show();
                return;
            }

            Snackbar.Make(_rootView, "Adding new entry...", Snackbar.LengthLong).Show();
            ThreadPool.QueueUserWorkItem(o => saveEntry());
        }

        private void saveEntry()
        {
            _entry = saveEntry(_entry);
        }

        private Entry saveEntry(Entry entry)
        {
            var result = IntuneService.AddAccountEntry(entry);
            if (result != null)
            {
                Snackbar.Make(_rootView, $"{result.Notes} entry saved.", Snackbar.LengthLong).Show();
                RunOnUiThread(() => { _okButton.Enabled = result.IsNew; });
                return result;
            }

            Snackbar.Make(_rootView, "Saving entry FAILED!", Snackbar.LengthIndefinite)
                    .SetAction("RETRY", (v) => { }).Show();

            return null;
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
            _entryDate.Text = _entry.TxnDate.ToString("dd/MM/yyyy");

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

            _entryQuantity.Text = "";
            if (_entry.Quantity != 0)
                _entryQuantity.Text = _entry.Quantity.ToString();

            _entryAmount.Text = "";
            if (_entry.Amount != 0)
                _entryAmount.Text = _entry.Amount.ToString();

            _entryNotes.Text = _entry.Notes;
        }

        private void showMessageBoardActivity()
        {
            var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            var loginUserName = Intent.GetStringExtra("LoginUserName");
            var accountName = Intent.GetStringExtra("AccountName");
            var messageBoardActivity = new Intent(this, typeof(ChatboardActivity));
            messageBoardActivity.PutExtra("ByUserId", loginUserId);
            messageBoardActivity.PutExtra("AccountId", _entry.AccountId);
            messageBoardActivity.PutExtra("AccountName", accountName);
            messageBoardActivity.PutExtra("EntryId", _entry.Id);
            StartActivity(messageBoardActivity);
        }
    }
}
