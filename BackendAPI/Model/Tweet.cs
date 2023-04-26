using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Model
{
    public class Tweet
    {
        public int Id { get; set; }
        public string Post { get; set; }
        public DateTime currentTime { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public ICollection<TweetLike> TweetLike { get; set; }
    }
}
