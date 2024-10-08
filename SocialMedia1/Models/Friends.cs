﻿using Microsoft.Identity.Client;

namespace SocialMedia1.Models
{
    public class Friends
    {
        public int AccountId {  get; set; }
        public Account Account { get; set; } = null!;
        public int FriendId { get; set; }
        public Account Friend {  get; set; } = null!;

        public Friends() { }

        public Friends(int accountId, int friendId)
        {
            AccountId = accountId;
            FriendId = friendId;
        }
    }
}
