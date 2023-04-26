using BackendAPI.Model.DTO;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using BackendAPI.Configurations;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendsController : Controller
    {
        
        private readonly string _connectionString;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public FriendsController(AppDbContext context, IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("Lite");
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        [Route("SearchFriends")]

        public async Task<IActionResult> SearchFriends(string id)
        {

            IEnumerable<FriendDto> listOfUsers;
            IEnumerable<FriendDto> listOfUsers2;

            var sqlQuery = $"Select Id , Username, Status from Friends Join AspNetUsers On Friends.ApplicationFriendId = AspNetUsers.Id Where Friends.ApplicationUserId = '{id}' AND status ='NotFriends'; Select Id , Username, Status from Friends Join AspNetUsers On Friends.ApplicationUserId = AspNetUsers.Id Where Friends.ApplicationFriendId = '{id}' AND status ='NotFriends' ";


            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var multi = connection.QueryMultiple(sqlQuery, null))
                {
                    listOfUsers =  multi.Read<FriendDto>().ToList();
                    listOfUsers2 = multi.Read<FriendDto>().ToList();
                }
                return Ok(listOfUsers.Concat(listOfUsers2).ToList());

            }


        }

        [HttpPatch]
        [Authorize]
        [Route("SendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest(string id,string friendId)
        {
            var sqlQuery = $"UPDATE Friends SET STATUS = '{id}' WHERE ApplicationUserId = '{id}' AND  ApplicationFriendId = '{friendId}' OR ApplicationUserId = '{friendId}' AND  ApplicationFriendId = '{id}'";
            using (var connection = new SqliteConnection(_connectionString))
            {
                return Ok(await connection.ExecuteAsync(sqlQuery));
            }
        }


        [HttpPatch]
        [Authorize]
        [Route("AcceptFriendRequest")]
        public async Task<IActionResult> AcceptFriendRequest(string id, string friendId)
        {
            var sqlQuery = $"UPDATE Friends SET STATUS = 'Friends' WHERE ApplicationUserId = '{id}' AND  ApplicationFriendId = '{friendId}' OR ApplicationUserId = '{friendId}' AND  ApplicationFriendId = '{id}'";
            using (var connection = new SqliteConnection(_connectionString))
            {
                return Ok(await connection.ExecuteAsync(sqlQuery));
            }
        }

        [HttpPatch]
        [Authorize]
        [Route("RejectFriendRequest")]
        public async Task<IActionResult> RejectFriendRequest(string id, string friendId)
        {
            var sqlQuery = $"UPDATE Friends SET STATUS = 'NotFriends' WHERE ApplicationUserId = '{id}' AND  ApplicationFriendId = '{friendId}' OR  ApplicationUserId = '{friendId}' AND  ApplicationFriendId = '{id}'";
            using (var connection = new SqliteConnection(_connectionString))
            {
                return Ok(await connection.ExecuteAsync(sqlQuery));
            }
        }


        [HttpGet]
        [Authorize]
        [Route("GetFriends")]

        public async Task<IActionResult> GetFriends(string id)
        {

            IEnumerable<FriendDto> listOfUsers;
            IEnumerable<FriendDto> listOfUsers2;
            var sqlQuery = $"Select Id , Username, Status from Friends Join AspNetUsers On Friends.ApplicationFriendId = AspNetUsers.Id Where Friends.ApplicationUserId = '{id}' AND status !='NotFriends'; " +
                $"Select Id , Username, Status from Friends Join AspNetUsers On Friends.ApplicationUserId = AspNetUsers.Id Where Friends.ApplicationFriendId = '{id}' AND status !='NotFriends' ";


            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var multi = connection.QueryMultiple(sqlQuery, null))
                {
                    listOfUsers = multi.Read<FriendDto>().ToList();
                    listOfUsers2 = multi.Read<FriendDto>().ToList();
                }

                return Ok(listOfUsers.Concat(listOfUsers2).ToList());
            }

        }

        [HttpGet]
        [Authorize]
        [Route("GetAcceptedFriends")]

        public async Task<IActionResult> GetAcceptedFriends(string id)
        {

            IEnumerable<FriendDto> listOfUsers;
            IEnumerable<FriendDto> listOfUsers2;
            var sqlQuery = $"Select Id , Username, Status from Friends Join AspNetUsers On Friends.ApplicationFriendId = AspNetUsers.Id Where Friends.ApplicationUserId = '{id}' AND status ='Friends'; " +
                $"Select Id , Username, Status from Friends Join AspNetUsers On Friends.ApplicationUserId = AspNetUsers.Id Where Friends.ApplicationFriendId = '{id}' AND status = 'Friends' ";


            using (var connection = new SqliteConnection(_connectionString))
            {
                using (var multi = connection.QueryMultiple(sqlQuery, null))
                {
                    listOfUsers = multi.Read<FriendDto>().ToList();
                    listOfUsers2 = multi.Read<FriendDto>().ToList();
                }


                return Ok(listOfUsers.Concat(listOfUsers2).ToList());
            }

        }


    }
}
