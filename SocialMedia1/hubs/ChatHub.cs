using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
                SetChatName(chat, selfAccId);
            }
            Clients.Caller.SendAsync("GetChats", chats);
        }

        public async Task GetChatMessages(string id, string searchStr) {
            int _id = int.Parse(id);
            _logger.Log(LogLevel.Information, "getting chat messages " + _id);
            _logger.Log(LogLevel.Information, "search str: " + searchStr);
            IQueryable<Message> query;
            if (searchStr == "") {
                query = _context.Message.Where(msg => msg.ChatId == _id);
            } else {
                query = _context.Message.Where(msg => msg.ChatId == _id && EF.Functions.Like(msg.Text, $"%{searchStr}%"));
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
            _logger.Log(LogLevel.Information, "connecting to chat " + chatId);
            Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        [Authorize]
        public async Task GetSelfAccountId() {
            _logger.Log(LogLevel.Information, "getting self account id");
            Clients.Caller.SendAsync("GetSelfAccountId", _context.GetSelfAccId());
        }

        private void SetChatName(Chat chat, int selfAccId) {
            int otherAccId = selfAccId;
            foreach(var acc in _context.GetChatsMembers(chat.Id)) {
                if (acc != selfAccId) {
                    otherAccId = acc;
                    break;
                }
            }
            chat.Name = _context.Account.FirstOrDefault(acc => acc.Id == otherAccId).Name;
        }
    }
}
