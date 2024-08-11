using System.ComponentModel.DataAnnotations;

namespace SocialMedia1.Models
{
    public class Account
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string LastName { get; set; }
        public string? Location {  get; set; }
        public List<ChatAccount> ChatAccounts { get; set;  } = [];
        public byte[]? ProfilePhoto { get; set; }

        public Account() { }

        public Account(string name, string lastName) {
            Name = name;
            LastName = lastName;
        }
    }
}
