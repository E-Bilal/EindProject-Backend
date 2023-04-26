

namespace BackendAPI.Model.DTO
{
    public class GetMessagesDto
    {

        public string Message { get; set; }
        public string ApplicationUserId { get; set; }
        public string ApplicationFriendId{ get; set; }

        public DateTime currentTime { get; set; }
    }
}
