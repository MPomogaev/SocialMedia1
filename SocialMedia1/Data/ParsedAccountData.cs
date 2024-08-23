using SocialMedia1.Models;

namespace SocialMedia1.Data {
    public class ParsedAccountData {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName {  get; set; }
        public string ProfilePhoto { get; set; }

        public ParsedAccountData(Account acc) {
            Id = acc.Id;
            Name = acc.Name;
            LastName = acc.LastName;
            ProfilePhoto = Convert.ToBase64String(acc.ProfilePhoto);
        }
    }
}
