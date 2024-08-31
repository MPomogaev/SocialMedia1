using SocialMedia1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Diagnostics;

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
                .OnDelete(DeleteBehavior.NoAction);
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

            modelBuilder.Entity<ChatType>()
                .HasData(new ChatType(ChatTypes.personal, "personal"));
            modelBuilder.Entity<ChatType>()
                .HasData(new ChatType(ChatTypes.group, "group"));
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
            var query = this.Account
                .Join(this.Friends,
                acc => acc.Id,
                friend => friend.FriendId,
                (acc, friend) => new { Account = acc, Friends = friend })
                .Where(join => join.Friends.AccountId == accId);
            if (searchLine != "")
                query = query.Where(join => EF.Functions.Like(join.Account.Name, $"%{searchLine}%"));
            return query.Select(join => join.Account);
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
