using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace Intune.Android
{
    public class AccountShareAdapter : BaseAdapter
    {
        List<int> _userIds;
        List<Contact> _contacts;
        Activity _activity;

        public AccountShareAdapter(Activity activity, int userId, int accountId, UserAccountRole ofRole)
        {
            _activity = activity;
            _contacts = IntuneService.GetAllContacts(userId);
            _userIds = IntuneService.GetAccountUsers(accountId, ofRole);
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
            contactSelected.Checked = contact.HasIntune() &&
                                    _userIds.Exists(u => u == contact.ContactUserId);

            var accountShareRole = view.FindViewById<RadioGroup>(Resource.Id.accountShareRoleRadioGroup);

            for (int i = 0; i < accountShareRole.ChildCount; i++)
            {
                var rb = accountShareRole.GetChildAt(i);
                rb.Enabled = contactSelected.Checked;
            }

            contactSelected.SetOnCheckedChangeListener(new CheckedChangeListener(accountShareRole));

            return view;
        }

        private class CheckedChangeListener : Java.Lang.Object,
                                    CompoundButton.IOnCheckedChangeListener
        {
            RadioGroup _accountShareRole;

            public CheckedChangeListener(RadioGroup accountShareRole)
            {
                _accountShareRole = accountShareRole;
            }

            public void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
            {
                for (int i = 0; i < _accountShareRole.ChildCount; i++)
                {
                    var rb = _accountShareRole.GetChildAt(i);
                    rb.Enabled = isChecked;
                }
            }
        }
    }
}