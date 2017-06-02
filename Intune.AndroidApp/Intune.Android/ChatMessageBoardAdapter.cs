using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace Intune.Android
{
    public enum ChatMessageDirection
    {
        Sent = 0,
        Received = 1
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public ChatMessageDirection Direction { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ChatMessageBoardAdapter : BaseAdapter
    {
        List<ChatMessage> _chatMessages;
        Activity _activity;

        public ChatMessageBoardAdapter(Activity activity)
        {
            _activity = activity;
            _chatMessages = new List<ChatMessage>();
            //_accounts = IntuneService.GetAllAccounts(userId, contactId);
        }

        public void AddMessage(ChatMessage chatMessage)
        {
            _chatMessages.Add(chatMessage);
        }

        public override int Count
        {
            get
            {
                return _chatMessages.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            ChatMessage chatMessage = _chatMessages[position];
            return new JavaObjectWrapper<ChatMessage> { Obj = chatMessage };
        }

        public override long GetItemId(int position)
        {
            return _chatMessages[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var chatMessage = _chatMessages[position];

            var resource = Resource.Layout.ChatMessageSent;

            if (chatMessage.Direction == ChatMessageDirection.Received)
                resource = Resource.Layout.ChatMessageReceived;

            var view = convertView ?? _activity.LayoutInflater.Inflate(resource, parent, false);

            if (chatMessage.Direction == ChatMessageDirection.Sent)
                showMessageSent(chatMessage, view);
            else
                showMessageReceived(chatMessage, view);

            return view;
        }

        private void showMessageSent(ChatMessage chatMessage, View view)
        {
            //TODO: display the date in a separate line... Today, Yesterday, 2 days back... on 12-MAY-2017 etc.

            //var layout = view.FindViewById<LinearLayout>(Resource.Id.messageLinearLayout);
            //var messageBox = new TextView(_activity);
            //layout.AddView(messageBox);

            var chatMessageText = view.FindViewById<TextView>(Resource.Id.chatMessageSTextView);
            chatMessageText.Text = chatMessage.Message;

            var chatMessageTimestamp = view.FindViewById<TextView>(Resource.Id.chatMessageSTimestampTextView);
            chatMessageTimestamp.Text = chatMessage.Timestamp.ToShortTimeString();

            var chatMessageUsername = view.FindViewById<TextView>(Resource.Id.chatMessageSUsernameTextView);
            chatMessageUsername.Text = chatMessage.Username;
        }

        private void showMessageReceived(ChatMessage chatMessage, View view)
        {
            var chatMessageText = view.FindViewById<TextView>(Resource.Id.chatMessageRTextView);
            chatMessageText.Text = chatMessage.Message;

            var chatMessageTimestamp = view.FindViewById<TextView>(Resource.Id.chatMessageRTimestampTextView);
            chatMessageTimestamp.Text = chatMessage.Timestamp.ToShortTimeString();

            var chatMessageUsername = view.FindViewById<TextView>(Resource.Id.chatMessageRUsernameTextView);
            chatMessageUsername.Text = chatMessage.Username;
        }
    }
}