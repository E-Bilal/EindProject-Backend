using BackendAPI.Configurations;
using BackendAPI.Model.DTO;
using Dapper;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MimeKit;



namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly MailSettings _settings;



        public SettingsController(UserManager<IdentityUser> userManager, IConfiguration configuration,IOptions<MailSettings> settings, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _connectionString = configuration.GetConnectionString("Lite");
            _configuration = configuration;
            _settings = settings.Value;

        }

        private IActionResult SendMailAsync(string body, string email)
        {
            MimeMessage mail = new MimeMessage();
            mail.From.Add(new MailboxAddress(_settings.Displayname, _settings.From));
            mail.To.Add(new MailboxAddress("Excited User", $"{email}"));
            mail.Subject = "Password Recovery";
            mail.Body = new TextPart("plain")
            {
                Text = "Use this token to reset your password by copying this in the 'Token' Section:" + "\n\n" + body
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_settings.Host, _settings.Port, _settings.UseSSL);
                }
                catch (SmtpCommandException ex)
                {
                    return BadRequest(ex.Message);

                }
                catch (SmtpProtocolException ex)
                {
                    return BadRequest(ex.Message);
                }

                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_settings.UserName,_settings.Password);

                try
                {
                    client.Send(mail);
                }
                catch (SmtpCommandException ex)
                {
                    return BadRequest(ex.Message);


                }
                catch (SmtpProtocolException ex)
                {
                    return BadRequest(ex.Message);
                }

                client.Disconnect(true);
            }

            return Ok();
        }


        [HttpGet]
        [Route("GetRecoveryToken")]

        public async Task<IActionResult> GetRecoveryToken(string email)
        {

            var user_exists = await _userManager.FindByEmailAsync(email);

            if (user_exists == null)
            {
                return BadRequest("Invalid Credentials");

            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user_exists);
            return SendMailAsync(token, email);
        }

        [HttpPost]
        [Route("ResetPassword")]

        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetpass)
        {

            var user_exists = await _userManager.FindByEmailAsync(resetpass.Email);

            if (user_exists == null)
            {
                return BadRequest("Invalid Credentials");
            }
            
            var resetPassResult = await _userManager.ResetPasswordAsync(user_exists, resetpass.Password, resetpass.RecoveryToken);

            if (resetPassResult.Succeeded)
            {
                return Ok();
            }

            else return BadRequest("Something went wrong , Try again later.");


        }

        [HttpDelete]
        [Route("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            if(ModelState.IsValid)
            {

                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {

                        try
                        {
                            string? sqlStatement = $"DELETE FROM Friends where ApplicationUserId = '{id}' OR ApplicationFriendId = '{id}'";
                            connection.Execute(sqlStatement, transaction: transaction);

                            sqlStatement = $"DELETE FROM TweetLikes where ApplicationUserId = '{id}'";
                            connection.Execute(sqlStatement, transaction: transaction);

                            sqlStatement = $"DELETE FROM Tweets where ApplicationUserId = '{id}'";
                            connection.Execute(sqlStatement, transaction: transaction);

                            sqlStatement = $"DELETE FROM Chats where ApplicationUserId = '{id}' OR ApplicationFriendId = '{id}'";
                            connection.Execute(sqlStatement, transaction: transaction);

                            sqlStatement = $"DELETE FROM AspNetUserRoles where UserId = '{id}'";
                            connection.Execute(sqlStatement, transaction: transaction);

                            sqlStatement = $"DELETE FROM AspNetUsers WHERE Id = '{id}'";
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
           
            return BadRequest();
        }



        [HttpPatch]
        [Authorize]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePassword)
        {
            var user = await _userManager.FindByIdAsync(changePassword.Id);
            var result = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);

            if (result.Succeeded)
            {
                return Ok();
            }

            else
            {
                return BadRequest();
            }
        }

        [HttpPatch]
        [Authorize]
        [Route("ChangeUsername")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto changeUsername)
        {
            var user = await _userManager.FindByIdAsync(changeUsername.Id);
            var result = await _userManager.SetUserNameAsync(user, changeUsername.NewUsername);

            if (result.Succeeded)
            {
                return Ok (await _userManager.GetUserNameAsync(user));
            }

            else
            {
                return BadRequest();
            }
        }

        [HttpPatch]
        [Authorize]
        [Route("ChangeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto changeEmail)
        {
            var user = await _userManager.FindByIdAsync(changeEmail.Id);
            var result = await _userManager.SetEmailAsync(user, changeEmail.NewEmail);
            

            if (result.Succeeded)
            {
                return Ok(await _userManager.GetEmailAsync(user));
            }

            else
            {
                return BadRequest();
            }
        }



        [HttpPatch]
        [Authorize(Roles = "User")]
        [Route("RequestAdminRole")]
        public async Task<IActionResult> RequestAdmin(string id)
        {
            var sqlQuery = $"UPDATE AspNetUsers SET RoleRequest = 'Pending' WHERE Id = '{id}'";
            using (var connection = new SqliteConnection(_connectionString))
            {
                return Ok(await connection.ExecuteAsync(sqlQuery));
            }
        }




        [HttpPatch]
        [Authorize(Roles = "Admin")]
        [Route("RevokeAdminRole")]
        public async Task<IActionResult> RevokeAdmin(string id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string? sqlStatement = $"UPDATE AspNetUserRoles Set RoleId = '76077f11-e35f-423e-bb92-763d2b568572' WHERE UserId = '{id}'";
                        connection.Execute(sqlStatement, transaction: transaction);

                        sqlStatement = $"UPDATE AspNetUsers SET RoleRequest = 'NoRequest' WHERE Id = '{id}'";
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

    }
}

