using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SocialMedia1.Models {
    public class Post {
        public int Id { get; set; }
        public string? Title { get; set; }
        [MaxLength(2000)]
        [Required]
        public string Text { get; set; }
        [Required]
        public DateTime? CreatedDate { get; set; }
        [Required]
        public int AccountId {  get; set; }
        public Account? Account { get; set; }

        public Post() { }

        public Post(int accountId, string text, string title = null) {
            AccountId = accountId;
            Text = text;
            Title = title;
        }
    }
}
