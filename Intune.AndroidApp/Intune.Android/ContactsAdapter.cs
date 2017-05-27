using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace Intune.Android
{
    public class ContactsAdapter : BaseAdapter
    {
        List<Contact> _contacts;
        Activity _activity;
        int _userId;

        public ContactsAdapter(Activity activity, int userId)
        {
            _activity = activity;
            _userId = userId;
            _contacts = IntuneService.GetAllContacts(_userId);
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
            return null;
        }

        public override long GetItemId(int position)
        {
            return _contacts[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ??
                _activity.LayoutInflater.Inflate(
                    Resource.Layout.ContactListItem, parent, false);

            var contact = _contacts[position];

            var contactName = view.FindViewById<TextView>(Resource.Id.contactNameTextView);
            contactName.Text = contact.Name;

            return view;
        }
    }
}