using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;
using Intune.ApiGateway;
using Intune.ApiGateway.Model;

namespace Intune.Android
{
    public class ContactsAdapter : BaseAdapter
    {
        List<Contact> _contacts;
        Activity _activity;

        public ContactsAdapter(Activity activity, int userId)
        {
            _activity = activity;
            _contacts = IntuneService.GetAllContacts(userId);
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
                    Resource.Layout.ContactListItem, parent, false);

            var contact = _contacts[position];

            var contactName = view.FindViewById<TextView>(Resource.Id.contactNameTextView);
            contactName.Text = contact.Name;

            var contactIsIntuned = view.FindViewById<TextView>(Resource.Id.contactIsIntunedTextView);
            contactIsIntuned.Text = contact.HasIntune() ? "Intuned" : "";

            var contactMobile = view.FindViewById<TextView>(Resource.Id.contactMobileTextView);
            contactMobile.Text = contact.Mobile;

            var contactAddress = view.FindViewById<TextView>(Resource.Id.contactAddressTextView);
            contactAddress.Text = contact.Address;

            var commentIndicator = view.FindViewById<ImageView>(Resource.Id.contactCommentIndicatorImageView);
            commentIndicator.Visibility = contact.HasUnreadComments || contact.HasComments
                                            ? ViewStates.Visible : ViewStates.Gone;

            //if (contact.HasUnreadComments)
            //    commentsIndicator.SetImageResource(Resource.Drawable.greendot);

            return view;
        }
    }
}