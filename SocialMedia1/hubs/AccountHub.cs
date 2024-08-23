using Microsoft.AspNetCore.SignalR;
using SocialMedia1.Data;
using SocialMedia1.Models;

namespace SocialMedia1.hubs {
    public class AccountHub: Hub {
        DataBaseContext _context;
        ILogger<AccountHub> _logger;

        public AccountHub(DataBaseContext context, ILogger<AccountHub> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task GetFriends(string accIdStr) {
            int accId = accIdStr == null ? _context.GetSelfAccId(): int.Parse(accIdStr);
            var friendsList = new List<ParsedAccountData> ();
            var accounts = _context.GetFriends(accId).ToList();
            accounts.ForEach(account => {
                account.SetPhotoOrDefault();
                var friend = new ParsedAccountData(account);
                friendsList.Add(friend);
            });
            Clients.Caller.SendAsync("GetFriends", friendsList);
        }

        public async Task GetPosts(string accIdStr) {
            int accId = accIdStr == null ? _context.GetSelfAccId() : int.Parse(accIdStr);
            var posts = _context.Post.
                Where(post => post.AccountId == accId)
                .OrderByDescending(post => post.CreatedDate).ToList();
            Clients.Caller.SendAsync("GetPosts", posts);
        }

        public async Task Post(string text, string title) {
            if (string.IsNullOrWhiteSpace(text)) {
                return;
            }
            int selfAccId = _context.GetSelfAccId();
            if (string.IsNullOrWhiteSpace(title)) {
                _context.Post.Add(new Post(selfAccId, text));
            } else {
                _context.Post.Add(new Post(selfAccId, text, title));
            }
            _context.SaveChanges();
            Clients.Caller.SendAsync("Post");
        }
    }
}
