using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Intune.Android
{
    [Activity(Label = "Message Board - Intune", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class ChatMessageBoardActivity : Activity
    {
        ChatMessageBoardAdapter _messageBoardAdapter = null;
        bool _sentMessage = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ChatMessages);

            setMessageBoardListAdapter();

            var messageSendButton = FindViewById<ImageButton>(Resource.Id.chatSendMessageImageButton);
            messageSendButton.Click += MessageSendButton_Click;

            //var contactId = Intent.GetIntExtra("ContactId", 0);
            //if (contactId == 0)
            //{
            //    var loginUserName = Intent.GetStringExtra("LoginUserName");
            //    Title = string.Format("{0} - Accounts", loginUserName);
            //}
            //else
            //{
            //    var contactName = Intent.GetStringExtra("ContactName");
            //    Title = string.Format("{0} - Accounts", contactName);
            //}
        }

        private void MessageSendButton_Click(object sender, EventArgs e)
        {
            var chatMessageTextView = FindViewById<EditText>(Resource.Id.chatMessageEditText);

            if (string.IsNullOrWhiteSpace(chatMessageTextView.Text))
                return;

            var chatMessage = new ChatMessage
            {
                Direction = _sentMessage ? ChatMessageDirection.Sent : ChatMessageDirection.Received,
                Message = chatMessageTextView.Text,
                Timestamp = DateTime.Now,
                Username = "Ashok Guduru",
            };

            _messageBoardAdapter.AddMessage(chatMessage);
            _messageBoardAdapter.NotifyDataSetChanged();

            _sentMessage = !_sentMessage;
        }

        private void setMessageBoardListAdapter()
        {
            //var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
            //var contactId = Intent.GetIntExtra("ContactId", 0);
            _messageBoardAdapter = new ChatMessageBoardAdapter(this);
            var messageBoardListView = FindViewById<ListView>(Resource.Id.chatMessageBoardListView);
            messageBoardListView.Adapter = _messageBoardAdapter;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.accounts_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //public override bool OnOptionsItemSelected(IMenuItem item)
        //{
        //    switch (item.ItemId)
        //    {
        //        case Resource.Id.accounts_menu_refresh:
        //            refreshList();
        //            break;
        //        case Resource.Id.accounts_menu_profile:
        //            showUserProfileActivity();
        //            break;
        //        case Resource.Id.accounts_menu_contacts:
        //            showContactsActivity();
        //            break;
        //        case Resource.Id.accounts_menu_new:
        //            showAccountActivity();
        //            break;
        //        default:
        //            break;
        //    }

        //    return base.OnOptionsItemSelected(item);
        //}

        //private void showUserProfileActivity()
        //{
        //    var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
        //    var loginUserName = Intent.GetStringExtra("LoginUserName");
        //    var userProfileActivity = new Intent(this, typeof(RegisterActivity));
        //    userProfileActivity.PutExtra("LoginUserId", loginUserId);
        //    userProfileActivity.PutExtra("LoginUserName", loginUserName);
        //    StartActivity(userProfileActivity);
        //}

        //private void showContactsActivity()
        //{
        //    var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
        //    var loginUserName = Intent.GetStringExtra("LoginUserName");
        //    var contactsActivity = new Intent(this, typeof(ContactsActivity));
        //    contactsActivity.PutExtra("LoginUserId", loginUserId);
        //    contactsActivity.PutExtra("LoginUserName", loginUserName);
        //    StartActivity(contactsActivity);
        //}

        //private void showAccountActivity()
        //{
        //    var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
        //    var accountActivity = new Intent(this, typeof(AccountActivity));
        //    accountActivity.PutExtra("LoginUserId", loginUserId);
        //    StartActivity(accountActivity);
        //}

        //private void showAccountEntriesActivity(Account account)
        //{
        //    var loginUserId = Intent.GetIntExtra("LoginUserId", 0);
        //    var accountsEntryActivity = new Intent(this, typeof(AccountEntriesActivity));
        //    accountsEntryActivity.PutExtra("LoginUserId", loginUserId);
        //    accountsEntryActivity.PutExtra("AccountId", account.Id);
        //    accountsEntryActivity.PutExtra("AccountName", account.Name);
        //    accountsEntryActivity.PutExtra("AccountRole", (int)account.Role);
        //    StartActivity(accountsEntryActivity);
        //}
    }
}