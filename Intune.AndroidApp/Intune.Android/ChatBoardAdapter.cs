using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Globalization;
using Android.Database;

namespace Intune.Android
{
    public enum CommentMessageDirection
    {
        None = 0,
        Sent = 1,
        Received = 2
    }

    public class CommentMessage
    {
        public int Id { get; set; }
        public CommentMessageDirection Direction { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ChatBoardAdapter : BaseAdapter
    {
        List<CommentMessage> _chatMessages;
        Activity _activity;

        public ChatBoardAdapter(Activity activity, List<CommentMessage> chatMessages)
        {
            _activity = activity;
            _chatMessages = chatMessages;
        }

        public void AddMessage(CommentMessage chatMessage)
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
            CommentMessage chatMessage = _chatMessages[position];
            return new JavaObjectWrapper<CommentMessage> { Obj = chatMessage };
        }

        public override long GetItemId(int position)
        {
            return _chatMessages[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var chatMessage = _chatMessages[position];
            var resource = getChatMessageListItemResource(chatMessage);
            var view = _activity.LayoutInflater.Inflate(resource, parent, false);

            switch (chatMessage.Direction)
            {
                case CommentMessageDirection.None:
                    renderMessagesDay(chatMessage, view);
                    break;
                case CommentMessageDirection.Sent:
                    renderSentMessage(chatMessage, view);
                    break;
                case CommentMessageDirection.Received:
                    renderReceivedMessage(chatMessage, view);
                    break;
                default:
                    throw new Exception("Invalid chatting direction");
            }

            return view;
        }

        private int getChatMessageListItemResource(CommentMessage chatMessage)
        {
            switch (chatMessage.Direction)
            {
                case CommentMessageDirection.None:
                    return Resource.Layout.ChatMessageDay;
                case CommentMessageDirection.Sent:
                    return Resource.Layout.ChatMessageSent;
                case CommentMessageDirection.Received:
                    return Resource.Layout.ChatMessageReceived;
                default:
                    throw new Exception("Invalid chatting direction");
            }
        }

        private void renderMessagesDay(CommentMessage chatMessage, View view)
        {
            var messagesDay = view.FindViewById<TextView>(Resource.Id.chatMessagesDay);
            var timeSpan = DateTime.Now.Subtract(chatMessage.Timestamp);

            if (chatMessage.Timestamp == DateTime.Today)
                messagesDay.Text = "Today";
            else if (timeSpan.TotalDays == 1)
                messagesDay.Text = "Yesterday";
            else
                messagesDay.Text = chatMessage.Timestamp.ToString("dddd, MMMM dd, yyyy");
        }

        private void renderSentMessage(CommentMessage chatMessage, View view)
        {
            var message = view.FindViewById<TextView>(Resource.Id.chatMessageSTextView);
            message.Text = chatMessage.Message;

            var messageTimestamp = view.FindViewById<TextView>(Resource.Id.chatMessageSTimestampTextView);
            messageTimestamp.Text = chatMessage.Timestamp.ToShortTimeString();
        }

        private void renderReceivedMessage(CommentMessage chatMessage, View view)
        {
            var message = view.FindViewById<TextView>(Resource.Id.chatMessageRTextView);
            message.Text = chatMessage.Message;

            var messageTimestamp = view.FindViewById<TextView>(Resource.Id.chatMessageRTimestampTextView);
            messageTimestamp.Text = chatMessage.Timestamp.ToShortTimeString();

            var messageUsername = view.FindViewById<TextView>(Resource.Id.chatMessageRUsernameTextView);
            messageUsername.Text = chatMessage.Username;

            if (string.IsNullOrWhiteSpace(chatMessage.Username))
            {
                messageUsername.Visibility = ViewStates.Gone;
                messageTimestamp.Gravity = GravityFlags.Left;
            }
            else
                messageUsername.Text = chatMessage.Username;
        }
    }
}
