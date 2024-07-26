using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialMedia1.Data;
using SocialMedia1.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SocialMedia1.Controllers;
using Microsoft.AspNetCore.Connections;


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

        public async Task GetFriends()
        {
            List<Account> accounts = _context.Account.ToList();
            foreach (var account in accounts)
            {
                _logger.Log(LogLevel.Information, account.Name);
            }
            Clients.Caller.SendAsync("GetFriends", accounts);
        }

        [Authorize]
        public async Task AddFriend(int id, HubConnectionContext connection)
        {
            string email = connection.User.Identity.Name;
            _logger.Log(LogLevel.Information, "Email: " + email, " id: " + id);
        }

    }
}
