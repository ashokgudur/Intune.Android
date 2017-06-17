using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System;

namespace Intune.Android
{
    public class AccountShareAdapter : BaseAdapter
    {
        List<Contact> _contacts;
        Activity _activity;

        public AccountShareAdapter(Activity activity, List<Contact> contacts)
        {
            _activity = activity;
            _contacts = contacts;
        }

        public override int Count
        {
            get
            {
                return _contacts.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            Contact contact = _contacts[position];
            return new JavaObjectWrapper<Contact> { Obj = contact };
        }

        public override long GetItemId(int position)
        {
            return _contacts[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ??
                _activity.LayoutInflater.Inflate(
                    Resource.Layout.AccountShareContactListItem, parent, false);

            var contact = _contacts[position];

            var contactName = view.FindViewById<CheckedTextView>(Resource.Id.accountSharedToCheckedTextView);
            contactName.Text = contact.Name;

            var contactSelected = view.FindViewById<CheckBox>(Resource.Id.contactSelectedCheckBox);
            contactSelected.SetOnCheckedChangeListener(null);
            contactSelected.Enabled = contact.HasIntune();
            contactSelected.Checked = contact.HasIntune() && contact.AccountSharedRole != UserAccountRole.Owner;

            var accountShareRole = view.FindViewById<RadioGroup>(Resource.Id.accountShareRoleRadioGroup);

            for (int i = 0; i < accountShareRole.ChildCount; i++)
            {
                var roleRadioButton = accountShareRole.GetChildAt(i) as RadioButton;
                roleRadioButton.Enabled = contactSelected.Checked;
                roleRadioButton.Checked = shouldCheckRole(roleRadioButton, contact);
            }

            contactSelected.SetOnCheckedChangeListener(new ContactCheckedChangeListener(accountShareRole, contact));

            if (contactSelected.Enabled)
            {
                accountShareRole.SetOnCheckedChangeListener(null);
                accountShareRole.SetOnCheckedChangeListener(new AccountShareRoleCheckedChangeListener(contact));
            }

            return view;
        }

        private bool shouldCheckRole(RadioButton roleRadioButton, Contact contact)
        {
            if (roleRadioButton.Id == Resource.Id.accountShareRoleImpRadioButton &&
                contact.AccountSharedRole == UserAccountRole.Impersonator) return true;

            if (roleRadioButton.Id == Resource.Id.accountShareRoleCollabRadioButton &&
                contact.AccountSharedRole == UserAccountRole.Collaborator) return true;

            if (roleRadioButton.Id == Resource.Id.accountShareRoleViewRadioButton &&
                contact.AccountSharedRole == UserAccountRole.Viewer) return true;

            return false;
        }

        private static UserAccountRole GetSharedUserRole(RadioButton radioButton)
        {
            if (radioButton.Id == Resource.Id.accountShareRoleImpRadioButton)
                return UserAccountRole.Impersonator;
            else if (radioButton.Id == Resource.Id.accountShareRoleCollabRadioButton)
                return UserAccountRole.Collaborator;
            else if (radioButton.Id == Resource.Id.accountShareRoleViewRadioButton)
                return UserAccountRole.Viewer;
            else
                throw new Exception("Invalid Account User Role");
        }

        private class AccountShareRoleCheckedChangeListener : Java.Lang.Object,
                            RadioGroup.IOnCheckedChangeListener
        {
            Contact _contact = null;

            public AccountShareRoleCheckedChangeListener(Contact contact)
            {
                _contact = contact;
            }

            public void OnCheckedChanged(RadioGroup group, int checkedId)
            {
                var radioButton = group.FindViewById<RadioButton>(checkedId);
                _contact.AccountSharedRole = GetSharedUserRole(radioButton);
            }
        }

        private class ContactCheckedChangeListener : Java.Lang.Object,
                                    CompoundButton.IOnCheckedChangeListener
        {
            RadioGroup _accountShareRoleRadioGroup;
            Contact _contact;

            public ContactCheckedChangeListener(RadioGroup accountShareRoleRadioGroup, Contact contact)
            {
                _accountShareRoleRadioGroup = accountShareRoleRadioGroup;
                _contact = contact;
            }

            public void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
            {
                if (!isChecked)
                    _contact.AccountSharedRole = UserAccountRole.Owner;
                //else
                //{
                //    var defaultRoleRadioButton = _accountShareRoleRadioGroup.GetChildAt(0) as RadioButton;
                //    defaultRoleRadioButton.Enabled = true;
                //    defaultRoleRadioButton.Checked = true;
                //}

                for (int i = 0; i < _accountShareRoleRadioGroup.ChildCount; i++)
                {
                    var radioButton = _accountShareRoleRadioGroup.GetChildAt(i) as RadioButton;
                    radioButton.Enabled = isChecked;

                    if (isChecked && radioButton.Checked)
                        _contact.AccountSharedRole = GetSharedUserRole(radioButton);
                }
            }
        }
    }
}