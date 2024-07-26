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
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public string Text { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}
