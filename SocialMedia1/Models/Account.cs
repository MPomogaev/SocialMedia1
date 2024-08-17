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
        public byte[]? ProfilePhoto { get; set; } = [];

        static private string defaultPhotoPath = Path.Combine
            (Directory.GetCurrentDirectory(), "wwwroot", "files", "defaultProfilePhoto.bmp");

        public Account() { }

        public Account(string name, string lastName) {
            Name = name;
            LastName = lastName;
        }

        public void SetPhotoOrDefault() {
            if (this.ProfilePhoto == null || this.ProfilePhoto.Length == 0) {
                this.ProfilePhoto = File.ReadAllBytes(defaultPhotoPath);
            }
        }
    }
}
