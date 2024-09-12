using SocialMedia1.Models;

namespace SocialMedia1.Data {
    public static class GlobalVariebles {
        public const long maxFileSize = 10 * 1024 * 1024;

        public static readonly List<(FriendRequestStatuses id, string status)> 
            FriendRequestStatusesList = new() { 
                (FriendRequestStatuses.unanswered, "unanswered"),
                (FriendRequestStatuses.rejected, "rejected")
            };

        public static readonly List<(ChatTypes id, string type)>
            ChatTypesList = new() {
                (ChatTypes.personal, "personal"),
                (ChatTypes.group, "group")
            };

    }
}
