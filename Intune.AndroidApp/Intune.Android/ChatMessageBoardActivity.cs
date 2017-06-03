using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;

namespace Intune.Android
{
    [Activity(Label = "Message Board - Intune", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class ChatMessageBoardActivity : Activity
    {
        ChatMessageBoardAdapter _messageBoardAdapter = null;

        //User _byUser { get; set; }
        //User _toUser { get; set; }
        //Account _account { get; set; }
        //Entry _entry { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ChatMessages);

            var byUserId = Intent.GetIntExtra("ByUserId", 0);
            var byUserName = Intent.GetStringExtra("ByUserName");
            var toUserId = Intent.GetIntExtra("ToUserId", 0);
            var toUserName = Intent.GetStringExtra("ToUserName");

            Title = string.Format("{0}@Intune", toUserName);

            var listView = FindViewById<ListView>(Resource.Id.chatMessageBoardListView);
            listView.Divider = null;

            var chatMessageBoardTitleBar = FindViewById<TextView>(Resource.Id.chatMessageBoardTitleBarTextView);
            chatMessageBoardTitleBar.Text = string.Format("Conversation between you and {0}", toUserName);

            setMessageBoardListAdapter(getContactComments(byUserId, byUserName, toUserId));

            var messageSendButton = FindViewById<ImageButton>(Resource.Id.chatSendMessageImageButton);
            messageSendButton.Click += MessageSendButton_Click;
        }

        public List<ChatMessage> getContactComments(int byUserId, string byUserName, int toUserId)
        {
            var comments = IntuneService.GetContactComments(byUserId, toUserId);
            var dates = comments.Select(c => c.DateTimeStamp.Date).Distinct().ToArray();
            var result = new List<ChatMessage>();

            foreach (var date in dates)
            {
                result.Add(new ChatMessage
                {
                    Direction = ChatMessageDirection.None,
                    Timestamp = date,
                });

                foreach (var comment in comments
                    .Where(c => c.DateTimeStamp.Date == date)
                    .OrderBy(c => c.Id).Select(c => c))
                {
                    result.Add(new ChatMessage
                    {
                        Id = comment.Id,
                        Direction = comment.ByUserName == byUserName
                                    ? ChatMessageDirection.Sent
                                    : ChatMessageDirection.Received,
                        Message = comment.CommentText,
                        Timestamp = comment.DateTimeStamp,
                    });
                }
            }

            return result;
        }

        private void MessageSendButton_Click(object sender, EventArgs e)
        {
            var chatMessageTextView = FindViewById<EditText>(Resource.Id.chatMessageEditText);
            var messageText = chatMessageTextView.Text.Trim();

            if (string.IsNullOrWhiteSpace(messageText))
                return;

            var chatMessage = new ChatMessage
            {
                Direction = ChatMessageDirection.Sent,
                Message = messageText,
                Timestamp = DateTime.Now,
            };

            _messageBoardAdapter.AddMessage(chatMessage);
            _messageBoardAdapter.NotifyDataSetChanged();
        }

        private void setMessageBoardListAdapter(List<ChatMessage> chatMessages)
        {
            _messageBoardAdapter = new ChatMessageBoardAdapter(this, chatMessages);
            var messageBoardListView = FindViewById<ListView>(Resource.Id.chatMessageBoardListView);
            messageBoardListView.Adapter = _messageBoardAdapter;
        }
    }
}