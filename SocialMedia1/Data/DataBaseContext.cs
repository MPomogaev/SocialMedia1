using SocialMedia1.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMedia1.Data
{
    public class DataBaseContext: DbContext
    {
        public DbSet<Account> Account { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<ChatAccount> ChatAccount { get; set; }
        public DbSet<LoginModel> LoginModel { get; set; }
        public DbSet<Friends> Friends { get; set; }
        public DbSet<ChatType> ChatType { get; set; }

        public DataBaseContext(DbContextOptions<DataBaseContext> options)
        : base(options){
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .HasData(new ChatType((int)ChatTypes.personal, "perconal"));
            modelBuilder.Entity<ChatType>()
                .HasData(new ChatType((int)ChatTypes.group, "group"));
        }

        public int createChat()
        {
            Chat chat = new Chat();
            this.Chat.Add(chat);
            this.SaveChanges();
            return chat.Id;
        }
    }
}
