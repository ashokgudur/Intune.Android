using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Client;
using System.Threading.Tasks;

namespace Intune.Android
{
    [Activity(Label = "Message Board - Intune", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class ChatMessageBoardActivity : Activity
    {
        ChatMessageBoardAdapter _messageBoardAdapter = null;
        IHubProxy _hubProxy = null;
        HubConnection _hubConnection = null;
        User _byUser { get; set; }
        User _toUser { get; set; }
        Account _account { get; set; }
        Entry _entry { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ChatMessages);

            var byUserId = Intent.GetIntExtra("ByUserId", 0);
            _byUser = IntuneService.GetUserById(byUserId);
            var toUserId = Intent.GetIntExtra("ToUserId", 0);
            _toUser = IntuneService.GetUserById(toUserId);

            Title = string.Format("{0}@Intune", _toUser.Name);

            var listView = FindViewById<ListView>(Resource.Id.chatMessageBoardListView);
            listView.Divider = null;

            var chatMessageBoardTitleBar = FindViewById<TextView>(Resource.Id.chatMessageBoardTitleBarTextView);
            chatMessageBoardTitleBar.Text = string.Format("Conversation between you and {0}", _toUser.Name);

            setMessageBoardListAdapter(getContactComments(byUserId, _byUser.Name, toUserId));

            connectAsync();

            var sendMessageButton = FindViewById<ImageButton>(Resource.Id.chatSendMessageImageButton);
            sendMessageButton.Click += SendMessageButton_Click;
        }

        protected override void OnDestroy()
        {
            if (_hubConnection != null)
            {
                _hubConnection.Stop();
                _hubConnection.Dispose();
            }

            base.OnDestroy();
        }

        public List<CommentMessage> getContactComments(int byUserId, string byUserName, int toUserId)
        {
            var comments = IntuneService.GetContactComments(byUserId, toUserId);
            var dates = comments.Select(c => c.DateTimeStamp.Date).Distinct().ToArray();
            var result = new List<CommentMessage>();

            foreach (var date in dates)
            {
                result.Add(new CommentMessage
                {
                    Direction = CommentMessageDirection.None,
                    Timestamp = date,
                });

                foreach (var comment in comments
                    .Where(c => c.DateTimeStamp.Date == date)
                    .OrderBy(c => c.Id).Select(c => c))
                {
                    result.Add(new CommentMessage
                    {
                        Id = comment.Id,
                        Direction = comment.ByUserName == byUserName
                                    ? CommentMessageDirection.Sent
                                    : CommentMessageDirection.Received,
                        Message = comment.CommentText,
                        Timestamp = comment.DateTimeStamp,
                    });
                }
            }

            return result;
        }

        private void SendMessageButton_Click(object sender, EventArgs e)
        {
            var chatMessageTextView = FindViewById<EditText>(Resource.Id.chatMessageEditText);
            var messageText = chatMessageTextView.Text.Trim();

            if (string.IsNullOrWhiteSpace(messageText))
                return;

            saveComment(messageText);

            var commentMessage = new CommentMessage
            {
                Direction = CommentMessageDirection.Sent,
                Message = messageText,
                Timestamp = DateTime.Now,
            };

            var chatMessage = new ChatMessage
            {
                ByEmail = _byUser.Email,
                ByName = _byUser.Name,
                ToEmail = _toUser == null ? "" : _toUser.Email,
                ToName = _toUser == null ? "" : _toUser.Name,
                Text = messageText,
                ByUserId = _byUser.Id,
                ToUserId = _toUser.Id,
                AccountId = _account == null ? 0 : _account.Id,
                EntryId = _entry == null ? 0 : _entry.Id,
                DateTimeStamp = commentMessage.Timestamp,
            };

            sendMessage(chatMessage);

            _messageBoardAdapter.AddMessage(commentMessage);
            _messageBoardAdapter.NotifyDataSetChanged();

            chatMessageTextView.Text = "";
        }

        private Comment saveComment(string message)
        {
            var comment = new Comment
            {
                ByUserId = _byUser.Id,
                ToUserId = _toUser.Id,
                CommentText = message,
                DateTimeStamp = DateTime.Now,
            };

            return IntuneService.AddComment(comment);
        }

        private async void connectAsync()
        {
            const string chatServerUri = @"http://intunechat-1.apphb.com/";

            _hubConnection = new HubConnection(chatServerUri);
            fillHubConnectionHeaderInfo();
            _hubProxy = _hubConnection.CreateHubProxy("ChatHub");
            _hubProxy.On<ChatMessage>("AddComment", (chatMessage) =>
                            Parallel.Invoke((Action)(() => processReceivedMessage(chatMessage))));

            await _hubConnection.Start();
        }

        private void fillHubConnectionHeaderInfo()
        {
            var userName = new KeyValuePair<string, string>("UserName", _byUser.Name);
            var userEmail = new KeyValuePair<string, string>("UserEmail", _byUser.Email);
            _hubConnection.Headers.Add(userName);
            _hubConnection.Headers.Add(userEmail);
        }

        private void processReceivedMessage(ChatMessage chatMessage)
        {
            var commentMessage = new CommentMessage
            {
                Direction = CommentMessageDirection.Received,
                Message = chatMessage.Text,
                Timestamp = chatMessage.DateTimeStamp,
            };

            _messageBoardAdapter.AddMessage(commentMessage);
            _messageBoardAdapter.NotifyDataSetChanged();

            //TODO: indate on main window saying that a new message is arrived contact, account, entry...
            //if (!processed)
            //    displayMessageIndicator(chatMessage);
        }

        private void sendMessage(ChatMessage chatMessage)
        {
            _hubProxy.Invoke("SendComment", chatMessage);
        }

        private void setMessageBoardListAdapter(List<CommentMessage> chatMessages)
        {
            _messageBoardAdapter = new ChatMessageBoardAdapter(this, chatMessages);
            var messageBoardListView = FindViewById<ListView>(Resource.Id.chatMessageBoardListView);
            messageBoardListView.Adapter = _messageBoardAdapter;
        }
    }
}