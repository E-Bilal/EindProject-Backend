namespace BackendAPI.Model.DTO
{
    public class GetTweetDto
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        public string Post { get; set; }

        public DateTime currentTime { get; set; }
        public bool? StatusLike { get; set; }

        public string Username { get; set; }

        public int? AmountLikes { get; set; }
    }
}
