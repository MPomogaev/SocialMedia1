using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMedia1.Data;
using SocialMedia1.Models;


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
            var accounts = _context.Account.Where(x => x.Id != selfAccId);
            if (searchLine != "") {
                accounts = accounts
                    .Where(x => EF.Functions.Like(x.Name, $"%{searchLine}%"));
            }
            var friends = GetSelfFriends(searchLine);
            List<MarkedAccount> markedAccounts = new List<MarkedAccount>();
            foreach (var account in accounts) {
                bool isNotFriend(Account friendAcc) {
                    return friendAcc.Id != account.Id;
                };
                bool notFriend = friends.TrueForAll(isNotFriend);
                markedAccounts.Add(new MarkedAccount(account, !notFriend));
                _logger.Log(LogLevel.Information, $"{account.Name} is not friend {notFriend}");
            }
            Clients.Caller.SendAsync("GetAllAccounts", markedAccounts);
        }

        public async Task GetMineAccounts(string searchLine = "") {
            _logger.Log(LogLevel.Information, "searchLine: " + searchLine);
            var friends = GetSelfFriends(searchLine);
            foreach (var account in friends) {
                _logger.Log(LogLevel.Information, "friend " + account.Name);
            }
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
            var chatSearchResult = _context.GetAccountChatsIds(selfAccId)
                .Intersect(_context.GetAccountChatsIds(otherAccId));
            int chatId;
            if (chatSearchResult.Count() == 0) {
                chatId = _context.CreatePersonalChat(selfAccId, otherAccId);
            } else {
                var chats = _context.Chat.Join(chatSearchResult,
                    ch => ch.Id,
                    chAcc => chAcc,
                    (ch, chAcc) => new {
                        Id = ch.Id,
                        ChatTypeId = ch.ChatTypeId
                    }).Where(ch => ch.ChatTypeId == ChatTypes.personal)
                    .ToList();
                if (chats.Count == 0) {
                    chatId = _context.CreatePersonalChat(selfAccId, otherAccId);
                } else {
                    chatId = chats.First().Id;
                }
            }
            _logger.Log(LogLevel.Information, "chat id: " + chatId);
            Clients.Caller.SendAsync("GoToChat", chatId);
        }

        private List<Account> GetSelfFriends(string searchLine) {
            int selfAccId = _context.GetSelfAccId();
            var query = _context.Account
                    .Join(_context.Friends,
                        acc => acc.Id,
                        friend => friend.FriendId,
                        (acc, friend) => new { Account = acc, Friends = friend})
                    .Where(join => join.Friends.AccountId == selfAccId);
            if (searchLine != "")
                query = query.Where(join => EF.Functions.Like(join.Account.Name, $"%{searchLine}%"));
            return query.Select(join => join.Account).ToList();
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
