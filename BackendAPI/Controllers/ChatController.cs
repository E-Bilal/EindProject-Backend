using BackendAPI.Configurations;
using BackendAPI.Model;
using BackendAPI.Model.DTO;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace BackendAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ChatController: ControllerBase
    {
    
        
        private readonly string _connectionString;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<TwitterHub> _hubContext;


        public ChatController(AppDbContext context, IConfiguration configuration, UserManager<IdentityUser> userManager,IHubContext<TwitterHub> hubContext )
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("Lite");
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpPost]
        //[Authorize]
        [Route("SendMessage")]

        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto sendMessage)
        {
            var time = DateTime.Now;
            string timeString = time.ToString("yyyy-MM-dd HH:mm:ss");


            var sqlStatement = $@"INSERT INTO Chats ( ApplicationUserId, ApplicationFriendId ,Message , currentTime) VALUES (@UserId,@FriendId ,@Message, '{timeString}' )";

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.ExecuteAsync(sqlStatement, sendMessage);
            }
            await _hubContext.Clients.All.SendAsync("Posted", "Testing stuff out");
            return Ok();

        }

        [HttpGet]
        [Route("GetMessages")]
        public async Task<IActionResult> GetMessages(string userId, string friendId)
        {

            var sqlStatement = $@"SELECT * from Chats WHERE ApplicationUserId = '{userId}' AND  ApplicationFriendId = '{friendId}';
                                  SELECT * from Chats WHERE ApplicationUserId ='{friendId}'  AND  ApplicationFriendId = '{userId}'";

            IEnumerable<GetMessagesDto> listOfMessages;
            IEnumerable<GetMessagesDto> listOfMessages2;

            try
            {

                using (var connection = new SqliteConnection(_connectionString))
                {
                    using (var multi = connection.QueryMultiple(sqlStatement))
                    {
                        listOfMessages = multi.Read<GetMessagesDto>().ToList();
                        listOfMessages2 = multi.Read<GetMessagesDto>().ToList();

                    }

                    listOfMessages = listOfMessages.Concat(listOfMessages2).ToList();

                    listOfMessages = listOfMessages.OrderBy(x => x.currentTime);

                    return Ok(listOfMessages);

                }
            }

            catch
            {
                return BadRequest(new AuthResult()
                {
                    Error = "Server Error"
                });

            }

        }

    }
}
