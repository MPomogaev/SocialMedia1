using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMedia1.Data;
using SocialMedia1.Models;

namespace SocialMedia1.hubs {
    public class PostsHub: Hub {
        ILogger<PostsHub> _logger;
        DataBaseContext _context;

        public PostsHub(ILogger<PostsHub> logger, DataBaseContext context) {
            _logger = logger;
            _context = context;
        }

        public async Task GetPosts() {
            _logger.LogInformation("getting posts");
            int accId = _context.GetSelfAccId();
            var friends = _context.GetFriends(accId).Select(friend => friend.Id).ToList();
            var postAccounts = _context.Post
                .Where(post => friends.Contains(post.AccountId))
                .Join(_context.Account,
                (p) => p.AccountId,
                (a) => a.Id,
                (p, a) => new {
                    Post = p,
                    Account = a
                })
                .OrderByDescending(userPost => userPost.Post.CreatedDate).ToList();
            List<ConcreteUserPost> concreteUserPosts = [];
            foreach(var postAccount in postAccounts) {
                postAccount.Account.SetPhotoOrDefault();
                concreteUserPosts.Add(new ConcreteUserPost{
                    Post = postAccount.Post,
                    Account = new ParsedAccountData(postAccount.Account),
                });
            }
            Clients.Caller.SendAsync("GetPosts", concreteUserPosts);
        }

        class ConcreteUserPost {
            public ParsedAccountData Account { get; set; }
            public Post Post { get; set; }
        }
    }
}
