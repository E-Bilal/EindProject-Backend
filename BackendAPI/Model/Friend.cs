using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Model
{
    public class Friend
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string ApplicationFriendId{ get; set; }
        public ApplicationUser ApplicationFriend { get; set; }
        public string Status { get; set; }

    }
}
