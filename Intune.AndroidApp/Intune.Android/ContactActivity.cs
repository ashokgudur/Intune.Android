using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;

namespace Intune.Android
{
    [Activity(Label = "Contact - Intune")]
    public class ContactActivity : Activity
    {
        Contact _contact = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Contact);

            var contactId = Intent.GetIntExtra("ContactId", 0);
            _contact = IntuneService.GetContact(contactId);

            if (_contact.Id == 0)
                this.Title = "New Contact - Intune";
            else
                this.Title = string.Format("{0} - Intune", _contact.Name);

            fillForm();

            var okButton = FindViewById<Button>(Resource.Id.contactOkButton);
            okButton.Click += OkButton_Click; ;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.contact_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.contact_menu_accounts:
                    //showAccountsActivity();
                    break;
                case Resource.Id.contact_menu_comment:
                    break;
                case Resource.Id.contact_menu_share:
                    //showContactActivity(0);
                    break;
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var result = FindViewById<TextView>(Resource.Id.contactResultTextView);
            var fullName = FindViewById<EditText>(Resource.Id.contactNameEditText);
            var email = FindViewById<EditText>(Resource.Id.contactEmailEditText);
            var mobile = FindViewById<EditText>(Resource.Id.contactMobileEditText);
            var address = FindViewById<EditText>(Resource.Id.contactAddressEditText);

            _contact.Name = fullName.Text;
            _contact.Email = email.Text;
            _contact.Mobile = mobile.Text;
            _contact.Address = address.Text;

            if (!_contact.IsValid())
            {
                result.Text = "Please enter all the details...";
                return;
            }

            if (_contact.Id == 0)
            {
                _contact.UserId = Intent.GetIntExtra("LoginUserId", 0);
                _contact.CreatedOn = DateTime.Now;
            }

            if (_contact.Id == 0)
            {
                result.Text = "Adding new contact...";
                _contact = IntuneService.AddContact(_contact);
            }
            else
            {
                result.Text = "Updating contact...";
                IntuneService.UpdateContact(_contact);
            }

            result.Text = string.Format("Contact {0} saved.", _contact.Name);
        }

        private void fillForm()
        {
            var fullName = FindViewById<EditText>(Resource.Id.contactNameEditText);
            var email = FindViewById<EditText>(Resource.Id.contactEmailEditText);
            var mobile = FindViewById<EditText>(Resource.Id.contactMobileEditText);
            var address = FindViewById<EditText>(Resource.Id.contactAddressEditText);

            fullName.Text = _contact.Name;
            email.Text = _contact.Email;
            mobile.Text = _contact.Mobile;
            address.Text = _contact.Address;
        }
    }
}
