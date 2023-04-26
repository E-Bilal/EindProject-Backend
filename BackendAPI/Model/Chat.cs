namespace BackendAPI.Model
{
    public class Chat
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public ApplicationUser ApplicationFriend { get; set; }

        public DateTime currentTime { get; set; }
    }
}
