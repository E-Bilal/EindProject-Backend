namespace BackendAPI.Model
{
    public class TweetLike
    {
        public int Id { get; set; }
        public bool StatusLike { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Tweet Tweet { get; set; }
    }
}
