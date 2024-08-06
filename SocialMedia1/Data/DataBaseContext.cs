using SocialMedia1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

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

        public DataBaseContext(DbContextOptions<DataBaseContext> options, IHttpContextAccessor context)
        : base(options) {
            _context = context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Message>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<ChatAccount>()
                .HasKey(e => new { e.AccountId, e.ChatId });
            modelBuilder.Entity<LoginModel>()
                .HasIndex(e => e.Email);
            modelBuilder.Entity<Friends>()
                .HasKey(e => new { e.AccountId, e.FriendId });
            modelBuilder.Entity<Friends>()
                .HasOne(e => e.Friend)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ChatType>()
                .HasKey(ct => ct.Id);
            modelBuilder.Entity<ChatType>()
                .Property(ct => ct.Id).HasConversion<int>();
            modelBuilder.Entity<Chat>()
                .Property(ch => ch.ChatTypeId).HasConversion<int>();

            modelBuilder.Entity<ChatType>()
                .HasData(new ChatType(ChatTypes.personal, "personal"));
            modelBuilder.Entity<ChatType>()
                .HasData(new ChatType(ChatTypes.group, "group"));
        }

        public IQueryable<int> GetChats(int accId) {
            return this.ChatAccount
                .Where(ch => ch.AccountId == accId)
                .Select(ch => ch.ChatId);
        }

        public int CreatePersonalChat(int firstAcc, int otherAcc) {
            int chatId = CreateChat(ChatTypes.personal);
            AddAccountToChat(chatId, firstAcc);
            AddAccountToChat(chatId, otherAcc);
            return chatId;
        }

        public void AddAccountToChat(int chatId, int accId) {
            this.ChatAccount.Add(new ChatAccount(chatId, accId));
            this.SaveChanges();
        }

        [Authorize]
        public int GetSelfAccId() {
            string email = _context.HttpContext.User.Identity.Name;
            return this.LoginModel.FirstOrDefault(x => x.Email == email).AccountId;
        }

        private int CreateChat(ChatTypes type) {
            Chat chat = new Chat(type);
            this.Chat.Add(chat);
            this.SaveChanges();
            return chat.Id;
        }
    }
}
