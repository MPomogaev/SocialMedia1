namespace SocialMedia1.Models
{
    public enum ChatTypes {
        personal = 1,
        group,
    }

    public class ChatType
    {
        public ChatTypes Id { get; set; }
        public string Type { get; set; }

        public ChatType(ChatTypes id, string type) {
            Id = id;
            Type = type;
        }
    }
}
