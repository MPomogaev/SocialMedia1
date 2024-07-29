namespace SocialMedia1.Models
{
    public class ChatType
    {
        public int Id { get; set; }
        public string Type { get; set; }

        public ChatType(int id, string type) {
            Id = id;
            Type = type;
        }
    }
}
