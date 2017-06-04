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
using System.Globalization;

namespace Intune.Android
{
    [Activity(Label = "Chat Board - Intune", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class ChatBoardActivity : Activity
    {
        ChatBoardAdapter _chatBoardAdapter = null;
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
            var accountId = Intent.GetIntExtra("AccountId", 0);
            var accountName = Intent.GetStringExtra("AccountName");
            _account = new Account { Id = accountId, Name = accountName };
            var entryId = Intent.GetIntExtra("EntryId", 0);
            _entry = IntuneService.GetAccountEntry(entryId);

            var chatBoardTitleBar = FindViewById<TextView>(Resource.Id.chatBoardTitleBarTextView);

            List<CommentMessage> chatMessages = null;

            if (!_entry.IsNew)
            {
                Title = string.Format("{0} Entry", accountName);
                chatBoardTitleBar.Text = string.Format("{0} on {1} of and {2}",
                        _entry.Notes, _entry.TxnDate.ToString("dd-MM-yyyy"),
                        _entry.Amount.ToString("C2", CultureInfo.CurrentCulture));
                chatMessages = getChatMessages(IntuneService.GetEntryComments(_entry.Id, byUserId), _byUser.Name);
            }
            else if (!_account.IsNew)
            {
                Title = _account.Name;
                chatBoardTitleBar.Text = string.Format("Comments on {0}", _account.Name);
                chatMessages = getChatMessages(IntuneService.GetAccountComments(_account.Id, byUserId), _byUser.Name);
            }
            else
            {
                Title = string.Format("{0}@Intune", _toUser.Name);
                chatBoardTitleBar.Text = string.Format("Conversation between you and {0}", _toUser.Name);
                chatMessages = getChatMessages(IntuneService.GetContactComments(byUserId, toUserId), _byUser.Name);
            }

            setMessageBoardListAdapter(chatMessages);

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

        public List<CommentMessage> getChatMessages(List<Comment> messages, string byUserName)
        {
            var dates = messages.Select(c => c.DateTimeStamp.Date).Distinct().ToArray();
            var result = new List<CommentMessage>();

            foreach (var date in dates)
            {
                result.Add(new CommentMessage
                {
                    Direction = CommentMessageDirection.None,
                    Timestamp = date,
                });

                foreach (var message in messages
                    .Where(c => c.DateTimeStamp.Date == date)
                    .OrderBy(c => c.Id).Select(c => c))
                {
                    result.Add(new CommentMessage
                    {
                        Id = message.Id,
                        Direction = message.ByUserName == byUserName
                                    ? CommentMessageDirection.Sent
                                    : CommentMessageDirection.Received,
                        Username = getByUserName(message.ByUserName),
                        Message = message.CommentText,
                        Timestamp = message.DateTimeStamp,
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

            var comment = saveComment(messageText);

            var message = new CommentMessage
            {
                Id = comment.Id,
                Direction = CommentMessageDirection.Sent,
                Message = messageText,
                Username = getByUserName(_byUser.Name),
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
                DateTimeStamp = message.Timestamp,
            };

            sendMessage(chatMessage);

            _chatBoardAdapter.AddMessage(message);
            _chatBoardAdapter.NotifyDataSetChanged();

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
            var message = new CommentMessage
            {
                Direction = CommentMessageDirection.Received,
                Message = chatMessage.Text,
                Username = getByUserName(chatMessage.ByName),
                Timestamp = chatMessage.DateTimeStamp,
            };

            _chatBoardAdapter.AddMessage(message);
            _chatBoardAdapter.NotifyDataSetChanged();

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
            _chatBoardAdapter = new ChatBoardAdapter(this, chatMessages);
            var chatBoardMessagesListView = FindViewById<ListView>(Resource.Id.chatBoardMessagesListView);
            chatBoardMessagesListView.Adapter = _chatBoardAdapter;
        }

        private string getByUserName(string byUserName)
        {
            return _entry.IsNew || _account.IsNew ? "" : byUserName;
        }
    }
}