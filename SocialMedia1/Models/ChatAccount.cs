namespace SocialMedia1.Models {
    public class ChatAccount {
        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        public ChatAccount() { }

        public ChatAccount(int chatId, int accountId) {
            ChatId = chatId;
            AccountId = accountId;
        }

    }
}
