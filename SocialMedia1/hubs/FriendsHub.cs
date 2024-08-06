using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMedia1.Data;
using SocialMedia1.Models;
using System.Runtime.CompilerServices;


namespace SocialMedia1.hubs {
    public class FriendsHub : Hub {
        DataBaseContext _context;
        ILogger<FriendsHub> _logger;

        public FriendsHub(DataBaseContext context, ILogger<FriendsHub> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task GetAllAccounts(string searchLine = "") {
            _logger.Log(LogLevel.Information, "searchLine: " + searchLine);
            int selfAccId = _context.GetSelfAccId();
            List<Account> accounts;
            if (searchLine == "") {
                accounts = _context.Account.Where(x => x.Id != selfAccId).ToList();
            } else {
                accounts = _context.Account
                    .Where(x => x.Id != selfAccId && EF.Functions.Like(x.Name, $"%{searchLine}%"))
                    .ToList();
            }
            var friends = GetSelfFriends(searchLine);
            List<MarkedAccount> markedAccounts = new List<MarkedAccount>();
            foreach (var account in accounts) {
                bool isNotFriend(Account acc) {
                    return acc.Id != account.Id;
                };
                bool notFriend = friends.TrueForAll(isNotFriend);
                if (notFriend) {
                    markedAccounts.Add(new MarkedAccount(account, false));
                } else {
                    markedAccounts.Add(new MarkedAccount(account, true));
                }
                _logger.Log(LogLevel.Information, $"{account.Name} is not friend {notFriend}");
            }
            Clients.Caller.SendAsync("GetAllAccounts", markedAccounts);
        }

        public async Task GetMineAccounts(string searchLine = "") {
            _logger.Log(LogLevel.Information, "searchLine: " + searchLine);
            var friends = GetSelfFriends(searchLine);
            Clients.Caller.SendAsync("GetMineAccounts", friends);
        }

        public async Task AddFriend(int otherAccId) {
            int selfAccId = _context.GetSelfAccId();
            _logger.Log(LogLevel.Information, "add friendship self acc id: " + selfAccId + ", other acc id: " + otherAccId);

            _context.Friends.Add(new Friends(selfAccId, otherAccId));
            _context.SaveChanges();
        }

        public async Task DeleteFriend(int otherAccId) {
            int selfAccId = _context.GetSelfAccId();
            _logger.Log(LogLevel.Information, "delete friendship self acc id: " + selfAccId + ", other acc id: " + otherAccId);

            _context.Friends.Remove(new Friends(selfAccId, otherAccId));
            _context.SaveChanges();
        }

        public async Task GoToChat(int otherAccId) {
            _logger.Log(LogLevel.Information, "going to chat");
            int selfAccId = _context.GetSelfAccId();
            var chatSearchResult = _context.GetChats(selfAccId)
                .Intersect(_context.GetChats(otherAccId));
            int chatId;
            if (chatSearchResult.Count() == 0) {
                _logger.Log(LogLevel.Information, "first search = 0");
                chatId = _context.CreatePersonalChat(selfAccId, otherAccId);
            } else {
                _logger.Log(LogLevel.Information, "first search != 0");
                var chats = _context.Chat.Join(chatSearchResult,
                    ch => ch.Id,
                    chAcc => chAcc,
                    (ch, chAcc) => new {
                        Id = ch.Id,
                        ChatTypeId = ch.ChatTypeId
                    }).Where(ch => ch.ChatTypeId == ChatTypes.personal)
                    .ToList();
                if (chats.Count == 0) {
                    _logger.Log(LogLevel.Information, "second search = 0");
                    chatId = _context.CreatePersonalChat(selfAccId, otherAccId);
                } else {
                    _logger.Log(LogLevel.Information, "second search != 0");
                    chatId = chats.First().Id;
                }
            }
            _logger.Log(LogLevel.Information, "chat id: " + chatId);
            Clients.Caller.SendAsync("GoToChat", chatId);
        }

        private List<Account> GetSelfFriends(string searchLine) {
            int selfAccId = _context.GetSelfAccId();
            if (searchLine != "")
                return _context.Account
                .FromSql($"select Account.Id, Account.Name from Account inner join Friends on Account.Id = Friends.FriendId where Friends.AccountId = {selfAccId} and Account.Name like {"%" + searchLine + "%"}")
                .ToList();
            return _context.Account
                .FromSql($"select Account.Id, Account.Name from Account inner join Friends on Account.Id = Friends.FriendId where Friends.AccountId = {selfAccId}")
                .ToList();
        }

    }

    class MarkedAccount {
        public Account Account { get; set; }
        public bool IsFriend { get; set; }

        public MarkedAccount() { }
        public MarkedAccount(Account account, bool isFriend) {
            Account = account;
            IsFriend = isFriend;
        }
    }

}
