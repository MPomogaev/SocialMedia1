using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMedia1.Data;
using SocialMedia1.Models;


namespace SocialMedia1.hubs
{
    public class FriendsHub: Hub
    {
        DataBaseContext _context;
        ILogger<FriendsHub> _logger;

        public FriendsHub(DataBaseContext context, ILogger<FriendsHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task GetAllAccounts()
        {
            int selfAccId = GetSelfAccId();
            var accounts = _context.Account.Where(x => x.Id != selfAccId).ToList();
            var friends = GetSelfFriends();
            List<MarkedAccount> markedAccounts = new List<MarkedAccount>();
            foreach (var account in accounts) {
                bool isNotFriend(Account acc) {
                    return acc.Id != account.Id;
                };
                bool notFriend = friends.TrueForAll(isNotFriend);
                if (notFriend) {
                    markedAccounts.Add(new MarkedAccount(account, false));
                }
                else {
                    markedAccounts.Add(new MarkedAccount(account, true));
                }
                _logger.Log(LogLevel.Information, $"{account.Name} is not friend {notFriend}");
            }
            Clients.Caller.SendAsync("GetAllAccounts", markedAccounts);
        }

        public async Task GetMineAccounts()
        {
            var friends = GetSelfFriends();
            Clients.Caller.SendAsync("GetMineAccounts", friends);
        }

        public async Task AddFriend(int otherAccId)
        {
            int selfAccId = GetSelfAccId();
            _logger.Log(LogLevel.Information, "self acc id: " + selfAccId + ", other acc id: " + otherAccId);

            _context.Friends.Add(new Friends(selfAccId, otherAccId));
            _context.SaveChanges();
        }

        private List<Account> GetSelfFriends()
        {
            int selfAccId = GetSelfAccId();
            return _context.Account
                .FromSql($"select Account.Id, Account.Name from Account inner join Friends on Account.Id = Friends.FriendId where Friends.AccountId = {selfAccId}")
                .ToList();
        }

        [Authorize]
        private int GetSelfAccId()
        {
            string email = Context.User.Identity.Name;
            return _context.LoginModel.FirstOrDefault(x => x.Email == email).AccountId;
        }

    }

    class MarkedAccount
    {
        public Account Account { get; set; }
        public bool IsFriend {  get; set; }

        public MarkedAccount() { }
        public MarkedAccount(Account account, bool isFriend)
        {
            Account = account;
            IsFriend = isFriend;
        }
    }

}
