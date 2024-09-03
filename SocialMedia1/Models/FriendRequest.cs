namespace SocialMedia1.Models {
    public class FriendRequest {
        public int RequesterId { get; set; }
        public Account Requester {  get; set; }
        public int RequestedId { get; set; }
        public Account Requested {  get; set; }
        public FriendRequestStatuses StatusId { get; set; }
        public FriendRequestStatus Status { get; set; }

        public FriendRequest() { }

        public FriendRequest(int requesterId, int requestedId, FriendRequestStatuses statusId) {
            RequesterId = requesterId;
            RequestedId = requestedId;
            StatusId = statusId;
        }

    }

}
