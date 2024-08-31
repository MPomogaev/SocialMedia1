using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialMedia1.Data;
using SocialMedia1.Models;

namespace SocialMedia1.hubs {
    public class ChatHub: Hub {
        DataBaseContext _context;
        ILogger<ChatHub> _logger;

        public ChatHub(DataBaseContext context, ILogger<ChatHub> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task GetChats() {
            _logger.Log(LogLevel.Information, "getting chats");
            int selfAccId = _context.GetSelfAccId();
            List<Chat> chats = _context.GetAccountChats(selfAccId).ToList();
            foreach (var chat in chats) {
                if (chat.Name == null) {
                    SetChatName(chat, selfAccId);
                }
            }
            Clients.Caller.SendAsync("GetChats", chats);
        }

        public async Task GetChatType(string chatId) {
            int id = int.Parse(chatId);
            Clients.Caller.SendAsync("ReceiveChatType", _context.GetChat(id).ChatTypeId);
        }

        public async Task GetChatMessages(string id, string searchStr) {
            int _id = int.Parse(id);
            _logger.Log(LogLevel.Information, "getting chat msgs search str: " + searchStr);
            IQueryable<Message> query = _context.Message.Where(msg => msg.ChatId == _id);
            if (searchStr != "") {
                query = query.Where(msg => EF.Functions.Like(msg.Text, $"%{searchStr}%"));
            }
            Clients.Caller.SendAsync("GetChatMessages", query.ToList());
        }

        public async Task SendMessage(string text, string chatId) {
            int selfAccId = _context.GetSelfAccId();
            _logger.Log(LogLevel.Information, "sending msg " + text + " from " + selfAccId + " to chat " + chatId);
            Message msg = new Message(selfAccId, text, int.Parse(chatId));
            _context.Message.Add(msg);
            _context.SaveChanges();
            Clients.Group(chatId).SendAsync("ReceiveMessage", msg);
        }

        [Authorize]
        public async Task ConnectToChat(string chatId) {
            Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        [Authorize]
        public async Task GetSelfAccountId() {
            _logger.Log(LogLevel.Information, "getting self account id");
            Clients.Caller.SendAsync("GetSelfAccountId", _context.GetSelfAccId());
        }

        public async Task GetFriends() {
            int selfAccId = _context.GetSelfAccId();
            Clients.Caller.SendAsync("GetFriends", _context.GetParsedFriendsData(selfAccId));
        }

        public async Task CreateChat(string name, List<int> members) {
            _logger.LogInformation("chat name " + name);
            int selfAccountId = _context.GetSelfAccId();
            members.Add(selfAccountId);
            int chatId = _context.CreateGroupChat(name, members);
            Clients.Caller.SendAsync("ChatCreated", chatId);
        }

        public async Task EditChat(string chatIdStr, string name, List<int> curentMembers) {
            _logger.LogInformation("chat name " + name);
            int selfAccountId = _context.GetSelfAccId();
            curentMembers.Add(selfAccountId);
            int chatId = int.Parse(chatIdStr);
            var oldMembers = _context.GetChatsMembers(chatId).ToHashSet();
            List<int> toAdd = new();
            curentMembers.ForEach(member => {
                if (!oldMembers.Contains(member)) {
                    toAdd.Add(member);
                } else {
                    oldMembers.Remove(member);
                }
            });
            toAdd.ForEach(memberId => {
                _context.ChatAccount.Add(new ChatAccount(chatId, memberId));
            });
            _context.SaveChanges();
            oldMembers.ToList().ForEach(memberId => {
                DeleteAccountFromChat(chatId, memberId);
            });
            Clients.Caller.SendAsync("ChatEdited", chatId);
        }

        public async Task LeaveChat(string chatId) {
            int id = int.Parse(chatId);
            int selfAccId = _context.GetSelfAccId();
            DeleteAccountFromChat(id, selfAccId);
            Clients.Caller.SendAsync("LeftChat");
        }

        public async Task GetChatInfo(int id) {
            string chatName = _context.GetChat(id).Name;
            List<int> membersIds = _context.GetChatsMembers(id).ToList();
            Clients.Caller.SendAsync("SetChatInfo", chatName, membersIds);
        }

        private void SetChatName(Chat chat, int selfAccId) {
            int otherAccId = selfAccId;
            foreach(var acc in _context.GetChatsMembers(chat.Id)) {
                if (acc != selfAccId) {
                    otherAccId = acc;
                    break;
                }
            }
            var account = _context.Account.FirstOrDefault(acc => acc.Id == otherAccId);
            chat.Name = account.Name + " " + account.LastName;
        }

        private void DeleteAccountFromChat(int chatId, int accId) {
            var row = _context.ChatAccount.Find(chatId, accId);
            _logger.LogInformation("deleting acc " + row.AccountId + " from chat " + row.ChatId);
            _context.ChatAccount.Remove(row);
            _context.SaveChanges();
            var chatMembers = _context.GetChatsMembers(chatId);
            if (chatMembers.IsNullOrEmpty()) {
                var chat = _context.Chat.Find(chatId);
                _context.Chat.Remove(chat);
                _context.SaveChanges();
                _logger.LogInformation("chat deleted");
            }
        }
    }
}
