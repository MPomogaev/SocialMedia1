namespace SocialMedia1.Models {

    public enum FriendRequestStatuses {
        unanswered = 1,
        rejected,
    }

    public class FriendRequestStatus {

        public FriendRequestStatuses Id { get; set; }
        public string Status { get; set; }

        public FriendRequestStatus(FriendRequestStatuses id, string status) {
            Id = id;
            Status = status;
        }
    }
}
