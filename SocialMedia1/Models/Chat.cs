using System.Diagnostics.CodeAnalysis;

namespace SocialMedia1.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public List<ChatAccount> ChatAccounts { get; set; } = [];
        public string? Name { get; set; }
        public ChatTypes ChatTypeId { get; set; }
        public ChatType ChatType { get; set; }

        public Chat() { }

        public Chat(ChatTypes type) {
            ChatTypeId = type;
        }
    }
}
