using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMedia1.Models
{
    public class Message
    {
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public DateTime? CreatedDate { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public int ChatId { get; set; }
        public Chat Chat { get; set; }

        public Message() { }

        public Message(int accountId, string text, int chatId) {
            AccountId = accountId;
            Text = text;
            ChatId = chatId;
        }
    }
}
