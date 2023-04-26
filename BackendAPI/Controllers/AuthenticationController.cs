using BackendAPI.Model;
using BackendAPI.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;


namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;



        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _connectionString = configuration.GetConnectionString("Lite");
            _configuration = configuration;
            _roleManager = roleManager;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto request)
        {
            if (ModelState.IsValid)
            {
                var email_exist = await _userManager.FindByEmailAsync(request.Email);
                var user_exist = await _userManager.FindByNameAsync(request.UserName);

                if (email_exist != null)
                {
                    return BadRequest("Email already exists");
                }

                else if (user_exist != null)
                {
                    return BadRequest("Username already exists");
                }

                var new_user = new IdentityUser()
                {
                    Email = request.Email,
                    UserName = request.UserName                    
                };

               
                var is_created = await _userManager.CreateAsync(new_user, request.Password);
                if (!is_created.Succeeded)
                {
                    return BadRequest(is_created.Errors);

                }
                var addRole = await _userManager.AddToRoleAsync(new_user, "User");

                if (addRole.Succeeded)
                {

                    var listOfUsers = new List<string>();
                    var currentUserId = await _userManager.GetUserIdAsync(new_user);

                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        listOfUsers = (await connection.QueryAsync<string>($"SELECT Id  FROM AspNetUsers WHERE Id != '{currentUserId}'")).ToList();
                    }



                    foreach (var user in listOfUsers)
                    {

                        var sqlQuery = $"INSERT INTO Friends (ApplicationUserId , ApplicationFriendId) VALUES ('{currentUserId}','{user}')";
                       
                        using (var connection = new SqliteConnection(_connectionString))
                        {
                            await connection.ExecuteAsync(sqlQuery);
                        }
                    }

                    return Ok();
                }
            }
            
            return BadRequest("Server Error");

        }






        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            if (ModelState.IsValid)
            {
                var user_exists = await _userManager.FindByEmailAsync(request.Email);

                if (user_exists == null)
                {
                    return BadRequest(new AuthResult
                    {
                        Error = "Invalid Credentials"
                    }); 

                }

                var is_correct = await _userManager.CheckPasswordAsync(user_exists, request.Password);

                if (!is_correct)
                {

                    return BadRequest(new AuthResult
                    {
                        Error = "Invalid Credentials"
                    });

                }

                var jwtToken = await GenerateJwtToken(user_exists);
                return Ok(new AuthResult()
                {
                    Token = jwtToken,
                });
            }

            return BadRequest(new AuthResult
            {
                Error = "Server Error"
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCurrenUser()
        {
            var id = _userManager.GetUserId(User);
            
            var sqlQuery = $"SELECT Id , Email , UserName , RoleRequest FROM AspNetUsers WHERE Id='{id}'";
            var sqlQuery2 = $"SELECT AspNetRoles.Name FROM AspNetUserRoles JOIN  AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId JOIN AspNetUsers ON AspNetUsers.Id = AspNetUserRoles.UserId WHERE AspNetUsers.Id = '{id}'";


            using (var connection = new  SqliteConnection(_connectionString))
            {
               var result = (await connection.QuerySingleAsync<UserInfoDto>(sqlQuery));
               result.Role = (await connection.ExecuteScalarAsync<string>(sqlQuery2));
               return Ok(result);

            }            
        }



        [HttpPost]
        [Route("Create Roles")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> CreateRoles(string roleName)
        {
            var role_exists = await _roleManager.RoleExistsAsync(roleName);
            if (!role_exists)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                return Ok("Roles Succesfully created");
            }
            return BadRequest("Role already exists");
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, value: user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
            };
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }


    }
}
