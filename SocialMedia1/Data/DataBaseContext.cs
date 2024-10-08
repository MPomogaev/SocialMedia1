﻿using SocialMedia1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SocialMedia1.Data {
    public class DataBaseContext: DbContext {
        IHttpContextAccessor _context;

        public DbSet<Account> Account { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<ChatAccount> ChatAccount { get; set; }
        public DbSet<LoginModel> LoginModel { get; set; }
        public DbSet<Friends> Friends { get; set; }
        public DbSet<ChatType> ChatType { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<FriendRequestStatus> FriendRequestStatus { get; set; }
        public DbSet<FriendRequest> FriendRequest { get; set; }

        public DataBaseContext(DbContextOptions<DataBaseContext> options, IHttpContextAccessor context)
        : base(options) {
            _context = context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Message>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<ChatAccount>()
                .HasKey(e => new { e.ChatId, e.AccountId });
            modelBuilder.Entity<LoginModel>()
                .HasIndex(e => e.Email);
            modelBuilder.Entity<Friends>()
                .HasKey(e => new { e.AccountId, e.FriendId });
            modelBuilder.Entity<Friends>()
                .HasOne(e => e.Friend)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Friends>()
                .HasIndex(e => e.FriendId)
                .IsUnique(false);
            modelBuilder.Entity<ChatType>()
                .HasKey(ct => ct.Id);
            modelBuilder.Entity<ChatType>()
                .Property(ct => ct.Id).HasConversion<int>();
            modelBuilder.Entity<Chat>()
                .Property(ch => ch.ChatTypeId).HasConversion<int>();
            modelBuilder.Entity<Post>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<FriendRequestStatus>()
                .HasKey(status => status.Id);
            modelBuilder.Entity<FriendRequestStatus>()
                .Property(status => status.Id).HasConversion<int>();
            modelBuilder.Entity<FriendRequest>()
                .Property(request => request.StatusId).HasConversion<int>();
            modelBuilder.Entity<FriendRequest>()
                .HasKey(request => new {request.RequesterId, request.RequestedId});
            modelBuilder.Entity<FriendRequest>()
                .HasOne(request => request.Requested)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FriendRequest>()
                .HasIndex(request => request.RequestedId)
                .IsUnique(false);

            foreach (var item in GlobalVariebles.ChatTypesList) {
                modelBuilder.Entity<ChatType>()
                    .HasData(new ChatType(item.id, item.type));
            }

            foreach(var item in GlobalVariebles.FriendRequestStatusesList) {
                modelBuilder.Entity<FriendRequestStatus>()
                    .HasData(new FriendRequestStatus(item.id, item.status));
            }
        }

        public IQueryable<int> GetAccountChatsIds(int accId) {
            return this.ChatAccount
                .Where(ch => ch.AccountId == accId)
                .Select(ch => ch.ChatId);
        }

        public IQueryable<Chat> GetAccountChats(int accId) {
            return this.Chat.Join(this.ChatAccount,
                ch => ch.Id,
                chatAcc => chatAcc.ChatId,
                (ch, chatAcc) => new {
                    Id = ch.Id,
                    Name = ch.Name,
                    ChatTypeId = ch.ChatTypeId,
                    AccountId = chatAcc.AccountId
                })
                .Where(ch => ch.AccountId == accId)
                .Select(ch => new Models.Chat(ch.Id, ch.Name, ch.ChatTypeId));
        }

        public int CreatePersonalChat(int firstAcc, int otherAcc) {
            int chatId = CreateChat(ChatTypes.personal);
            this.ChatAccount.Add(new ChatAccount(chatId, firstAcc));
            this.ChatAccount.Add(new ChatAccount(chatId, otherAcc));
            this.SaveChanges();
            return chatId;
        }

        public int CreateGroupChat(string name, List<int> members) {
            int chatId = CreateChat(ChatTypes.group, name);
            foreach(int member in members) {
                this.ChatAccount.Add(new ChatAccount(chatId, member));
            }
            this.SaveChanges();
            return chatId;
        }

        public IQueryable<int> GetChatsMembers(int chatId) {
            return this.ChatAccount
                .Where(chatAcc => chatAcc.ChatId == chatId)
                .Select(chatAcc => chatAcc.AccountId);
        }

        [Authorize]
        public int GetSelfAccId() {
            string email = _context.HttpContext.User.Identity.Name;
            return this.LoginModel.FirstOrDefault(x => x.Email == email).AccountId;
        }

        public Chat GetChat(int id) {
            return this.Chat.FirstOrDefault(ch => ch.Id == id);
        }

        public IQueryable<Account> GetFriends(int accId, string searchLine = "") {
            var friendsIds = this.Friends
                .Where(friend => friend.FriendId == accId)
                .Select(friend => friend.AccountId)
                .Union(this.Friends
                    .Where(friend => friend.AccountId == accId)
                    .Select(friend => friend.FriendId));
            var friends = this.Account
                .Where(account => friendsIds.Contains(account.Id));
            AddWhereLikeStatement(ref friends, searchLine);
            return friends;
        }

        public List<ParsedAccountData> GetParsedFriendsData(int accId) {
            var friendsList = new List<ParsedAccountData>();
            var accounts = this.GetFriends(accId).ToList();
            accounts.ForEach(account => {
                account.SetPhotoOrDefault();
                var friend = new ParsedAccountData(account);
                friendsList.Add(friend);
            });
            return friendsList;
        }

        public void AddWhereLikeStatement(ref IQueryable<Account> query, string searchLine) {
            if (searchLine != "") {
                query = query
                    .Where(acc => EF.Functions.Like(acc.Name, $"%{searchLine}%"));
            }
        }

        private int CreateChat(ChatTypes type, string name = "") {
            Chat chat = new Chat(type);
            if (!String.IsNullOrWhiteSpace(name)) {
                chat.Name = name;
            }
            this.Chat.Add(chat);
            this.SaveChanges();
            return chat.Id;
        }
    }
}
