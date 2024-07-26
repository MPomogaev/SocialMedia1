using System.ComponentModel.DataAnnotations;

namespace SocialMedia1.Models
{
    public class Account
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<ChatAccount> ChatAccounts { get; set;  } = [];
    }
}
