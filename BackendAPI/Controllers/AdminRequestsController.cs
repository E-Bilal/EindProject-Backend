using BackendAPI.Configurations;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackendAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdminRequestsController:ControllerBase
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AdminRequestsController(UserManager<IdentityUser> userManager, IConfiguration configuration ,RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _connectionString = configuration.GetConnectionString("Lite");


        }


        [HttpGet]
        [Authorize(Roles = "SuperUser")]
        [Route("GetAdminRequests")]

        public async Task<IActionResult> GetAdminRequests()
        {

            var sqlQuery = $"SELECT Id , UserName , RoleRequest FROM AspNetUsers WHERE RoleRequest = 'Pending' OR RoleRequest = 'Accepted'";
            
            using (var connection = new SqliteConnection(_connectionString))
            {
                return Ok(await connection.QueryAsync(sqlQuery));
            }
        }



        [HttpPatch]
        [Authorize(Roles = "SuperUser")]
        [Route("AcceptAdminRole")]
        public async Task<IActionResult> AcceptAdminRole(string id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {

                    try
                    {                        
                        string? sqlStatement = $"UPDATE AspNetUserRoles Set RoleId = '12b2f473-defb-445a-b807-6bd5942b1046' WHERE UserId = '{id}'";
                        connection.Execute(sqlStatement, transaction: transaction);

                        
                        sqlStatement = $"UPDATE AspNetUsers SET RoleRequest = 'Accepted' WHERE Id = '{id}'";
                        int rowsaffected = connection.Execute(sqlStatement, transaction: transaction);

                        if (rowsaffected == 0)
                        {
                            throw new Exception("Something went wrong try again later");
                        }

                        transaction.Commit();
                        return Ok("User has successfully been deleted");

                    }

                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
           

        }

        [HttpPatch]
        [Authorize(/*AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,*/ Roles = "SuperUser")]
        [Route("RejectAdminRole")]
        public async Task<IActionResult> RejectFriendRequest(string id)
        {
            var sqlQuery = $"UPDATE AspNetUsers SET RoleRequest = 'NoRequest' WHERE Id = '{id}'";
            using (var connection = new SqliteConnection(_connectionString))
            {
                return Ok(await connection.ExecuteAsync(sqlQuery));
            }
        }


    }
}
