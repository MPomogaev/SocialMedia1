using Azure.Core;
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
            int selfAccId = _context.GetSelfAccId();
            var accounts = _context.Account.Where(x => x.Id != selfAccId);
            _context.AddWhereLikeStatement(ref accounts, searchLine);
            var friends = GetSelfFriends(searchLine).ToList();
            var requestedAccountsIds = GetRequestsFromAccount(selfAccId, searchLine)
                .Select(request => request.Id).ToHashSet();
            List<MarkedAccount> markedAccounts = new();
            bool inFriends(Account account) {
                return friends.FirstOrDefault(friend => friend.Id == account.Id) != null;
            } 
            foreach (var account in accounts) {
                bool cannotRequest = inFriends(account);
                cannotRequest |= requestedAccountsIds.Contains(account.Id);
                markedAccounts.Add(new MarkedAccount(account, !cannotRequest));
            }
            Clients.Caller.SendAsync("GetAllAccounts", markedAccounts);
        }

        public async Task GetMineAccounts(string searchLine = "") {
            var friends = GetSelfFriends(searchLine);
            Clients.Caller.SendAsync("GetMineAccounts", friends.ToList());
        }

        public async Task GetRequestsToMe(string searchLine = "") {
            int selfAccountId = _context.GetSelfAccId();
            var requests = GetRequestsToAccount(selfAccountId, searchLine);
            Clients.Caller.SendAsync("GetRequestsToMe", requests.ToList());
        }

        public async Task GetRequestsFromMe(string searchLine = "") {
            int selfAccountId = _context.GetSelfAccId();
            var requests = GetRequestsFromAccount(selfAccountId, searchLine);
            Clients.Caller.SendAsync("GetRequestsFromMe", requests.ToList());
        }

        public async Task AddFriend(int otherAccId) {
            int selfAccId = _context.GetSelfAccId();
            var request = _context.FriendRequest.Find(otherAccId, selfAccId);
            if (request == null) {
                _logger.LogInformation("creating request");
                _context.FriendRequest.Add(new FriendRequest(
                    selfAccId, otherAccId, FriendRequestStatuses.unanswered));
            }  else {
                AproveRequest(request);
            }
            _context.SaveChanges();
        }

        public async Task DeleteFriend(int otherAccId) {
            int selfAccId = _context.GetSelfAccId();
            _logger.Log(LogLevel.Information, "delete friendship self acc id: " + selfAccId + ", other acc id: " + otherAccId);

            _context.Friends.Remove(new Friends(selfAccId, otherAccId));
            _context.SaveChanges();
        }

        public async Task RejectRequest(int otherAccId) {
            int selfAccId = _context.GetSelfAccId();
            RemoveRequest(otherAccId, selfAccId);
        }

        public async Task RecallRequest(int otherAccId) {
            int selfAccId = _context.GetSelfAccId();
            RemoveRequest(selfAccId, otherAccId);
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

        private IQueryable<Account> GetSelfFriends(string searchLine) {
            int selfAccId = _context.GetSelfAccId();
            return _context.GetFriends(selfAccId, searchLine);
        }

        private IQueryable<Account> GetRequestsFromAccount(int accountId, string searchLine) {
            var requests = _context.FriendRequest
            .Where(request => request.RequesterId == accountId
                && request.StatusId == FriendRequestStatuses.unanswered)
            .Join(_context.Account,
                request => request.RequestedId,
                account => account.Id,
                (request, account) => account);
            _context.AddWhereLikeStatement(ref requests, searchLine);
            return requests;
        }

        private IQueryable<Account> GetRequestsToAccount(int accountId, string searchLine) {
            var requests = _context.FriendRequest
            .Where(request => request.RequestedId == accountId
                && request.StatusId == FriendRequestStatuses.unanswered)
            .Join(_context.Account,
                request => request.RequesterId,
                account => account.Id,
                (request, account) => account);
            _context.AddWhereLikeStatement(ref requests, searchLine);
            return requests;
        }

        private void AproveRequest(FriendRequest request) {
            _logger.LogInformation("adding friendship");
            _context.Friends.Add(new Friends(request.RequesterId, request.RequestedId));
            _context.FriendRequest.Remove(request);
        }

        private void RemoveRequest(int requesterId, int requestedId) {
            var request = _context.FriendRequest.Find(requesterId, requestedId);
            if (request != null) {
                _context.FriendRequest.Remove(request);
                _context.SaveChanges();
            }
        }

    }

    class MarkedAccount: Account {
        public bool CanRequest { get; set; }

        public MarkedAccount() { }
        public MarkedAccount(Account account, bool canRequest) {
            Id = account.Id;
            Name = account.Name;
            LastName = account.LastName;
            CanRequest = canRequest;
        }
    }

}
