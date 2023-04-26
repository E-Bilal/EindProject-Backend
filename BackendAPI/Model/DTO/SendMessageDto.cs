namespace BackendAPI.Model.DTO
{
    public class SendMessageDto
    {
        public string UserId { get; set; }
        public string  FriendId { get; set; }
        public string Message { get; set; }
    }
}
