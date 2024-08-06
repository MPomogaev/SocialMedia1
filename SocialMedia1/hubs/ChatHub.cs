using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialMedia1.Data;
using SocialMedia1.Models;
using System.Security.Cryptography;

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
            _logger.Log(LogLevel.Information, "id: " + selfAccId);
            var chats = _context.GetChats(selfAccId).ToList();
            foreach (var chat in chats) {
                _logger.Log(LogLevel.Information, "chats: " + chat);
            }
            Clients.Caller.SendAsync("GetChats", chats);
        }

        public async Task GetChatMessages(string id) {
            int _id = int.Parse(id);
            _logger.Log(LogLevel.Information, "getting chat messages " + _id);
            var msgList = _context.Message.Where(msg => msg.ChatId == _id).ToList();
            foreach (var chat in msgList) {
                _logger.Log(LogLevel.Information, chat.Text);
            }
            Clients.Caller.SendAsync("GetChatMessages", msgList);
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
    }
}
