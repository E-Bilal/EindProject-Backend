using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;
using Dapper;
using BackendAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BackendAPI.Model.DTO;

using Microsoft.Data.Sqlite;
using BackendAPI.Configurations;

namespace BackendAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TweetController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly AppDbContext _context;

        public TweetController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("Lite");

        }


        [HttpPost]
        [Authorize]
        [Route("PostTweet")]
        public async Task<IActionResult> PostTweet([FromBody] TweetDto tweet)
        {

            var tweetId = new List<dynamic>();
            var sqlStatement = "";

            var time = DateTime.Now;
            string timeString = time.ToString("yyyy-MM-dd HH:mm:ss");

            using (var connection = new SqliteConnection(_connectionString))
            {
                

                sqlStatement = $@"INSERT INTO Tweets(Post ,currentTime, ApplicationUserId) VALUES (@Post ,'{timeString}',@UserId)";
                await connection.ExecuteAsync(sqlStatement, tweet);

                sqlStatement = $@"SELECT Id FROM Tweets Where currentTime = '{timeString}' AND ApplicationUserId = @UserId";
                tweetId = (await connection.QueryAsync(sqlStatement, tweet)).ToList();

                sqlStatement = $@"Insert INTO TweetLikes (StatusLike, ApplicationUserId, TweetId) VALUES ( 0 , @UserId , {tweetId[0].Id} )";
                await connection.ExecuteAsync(sqlStatement, tweet);

            }

            return Ok();

        }

        [HttpPatch]
        [Authorize]
        [Route("LikeDislike")]
        public async Task<IActionResult> LikeDislike([FromBody] LikeDislikeDto likedislike)
        {

            var likedislikeStatus = new List<dynamic>();
            var sqlStatement = "";



            using (var connection = new SqliteConnection(_connectionString))
            {
                

                sqlStatement = $@"SELECT StatusLike , Id FROM TweetLikes Where tweetId = @TweetId AND ApplicationUserId = @UserId";
                likedislikeStatus = (await connection.QueryAsync(sqlStatement, likedislike)).ToList();


                if (likedislikeStatus.Count == 0 )
                {
                    sqlStatement = $@"Insert INTO TweetLikes (StatusLike, ApplicationUserId, TweetId) VALUES ( 1 , @UserId , @TweetId)";
                    await connection.ExecuteAsync(sqlStatement, likedislike);
                    return Ok();
  
                }

                else if (likedislikeStatus[0].StatusLike == 1)
                {
                    sqlStatement = $@"UPDATE  TweetLikes SET StatusLike = 0 Where tweetId = @TweetId AND ApplicationUserId = @UserId";
                    await connection.ExecuteAsync(sqlStatement, likedislike);
                    return Ok();

                }

                else if (likedislikeStatus[0].StatusLike == 0)
                {
                    sqlStatement = $@"UPDATE  TweetLikes SET StatusLike = 1 Where tweetId = @TweetId AND ApplicationUserId = @UserId";
                    await connection.ExecuteAsync(sqlStatement, likedislike);
                    return Ok();
                }



            }

            return BadRequest(new AuthResult()
                {
                    Error = "Server Error"
                });        
            
        }


        [HttpGet]
        [Authorize]
        [Route("GetTweets")]
        public async Task<IActionResult> GetTweets(string id)
        {
            if (ModelState.IsValid)
            {

                var sqlStatement = $@" Select Tweets.Id ,Tweets.ApplicationUserId , Post , currentTime , UserName from Tweets JOIN Friends ON Tweets.ApplicationUserId = Friends.ApplicationUserId JOIN AspNetUsers ON Tweets.ApplicationUserId = AspNetUsers.Id  Where Status = 'Friends' AND Friends.ApplicationFriendId='{id}'  ;
                                       Select Tweets.Id, Tweets.ApplicationUserId , Post , currentTime , UserName from Tweets JOIN Friends ON Tweets.ApplicationUserId = Friends.ApplicationFriendId  JOIN AspNetUsers ON Tweets.ApplicationUserId = AspNetUsers.Id Where Status = 'Friends' AND Friends.ApplicationUserId='{id}'  ; 
                                     Select Tweets.Id, Tweets.ApplicationUserId , Post , currentTime ,UserName from Tweets  JOIN AspNetUsers ON Tweets.ApplicationUserId = AspNetUsers.Id where  Tweets.ApplicationUserId = '{id}'  ";

                IEnumerable<GetTweetDto> listOfTweets;
                IEnumerable<GetTweetDto> listOfTweets2;
                IEnumerable<GetTweetDto> listOfTweets3;
                IEnumerable<GetTweetDto> Tweets;

                using (var connection = new SqliteConnection(_connectionString))
                {
                    using (var multi = connection.QueryMultiple(sqlStatement))
                    {
                        listOfTweets = multi.Read<GetTweetDto>().ToList();
                        listOfTweets2 = multi.Read<GetTweetDto>().ToList();
                        listOfTweets3 = multi.Read<GetTweetDto>().ToList();
                    }
                    Tweets = listOfTweets.Concat(listOfTweets2).Concat(listOfTweets3).ToList();

                    foreach (var tweet in Tweets)
                    {
                        var sqlStatement2 = $" Select StatusLike  from Tweets JOIN TweetLikes ON TweetLikes.TweetId = Tweets.Id Where TweetLikes.ApplicationUserId = '{id}'  And TweetId = {tweet.Id} ";
                        tweet.StatusLike = (await connection.ExecuteScalarAsync<bool>(sqlStatement2));

                        var sqlStatement3 = $"Select COUNT (*) from Tweets Join TweetLikes on Tweets.Id = TweetLikes.TweetId Where TweetId = {tweet.Id} AND StatusLike = 1";                    
                        tweet.AmountLikes = (await connection.ExecuteScalarAsync<int>(sqlStatement3));

                    }
                    Tweets = Tweets.OrderByDescending(x => x.currentTime);
                
                    return Ok(Tweets);

                }
            }

            return BadRequest(new AuthResult()
            {
                Error = "Server Error"
            });

        }

        [HttpGet]
        [Authorize]
        [Route("GetAllTweets")]
        public async Task<IActionResult> GetAllTweets(string id)
        {
            if (ModelState.IsValid)
            {

                var sqlStatement = $@" Select Tweets.Id ,Tweets.ApplicationUserId , Post , currentTime , UserName from Tweets JOIN Friends ON Tweets.ApplicationUserId = Friends.ApplicationUserId JOIN AspNetUsers ON Tweets.ApplicationUserId = AspNetUsers.Id;
                                       Select Tweets.Id, Tweets.ApplicationUserId , Post , currentTime , UserName from Tweets JOIN Friends ON Tweets.ApplicationUserId = Friends.ApplicationFriendId  JOIN AspNetUsers ON Tweets.ApplicationUserId = AspNetUsers.Id; 
                                     Select Tweets.Id, Tweets.ApplicationUserId , Post , currentTime ,UserName from Tweets  JOIN AspNetUsers ON Tweets.ApplicationUserId = AspNetUsers.Id where  Tweets.ApplicationUserId = '{id}'  ";

                IEnumerable<GetTweetDto> listOfTweets;
                IEnumerable<GetTweetDto> listOfTweets2;
                IEnumerable<GetTweetDto> listOfTweets3;
                IEnumerable<GetTweetDto> Tweets;

                using (var connection = new SqliteConnection(_connectionString))
                {
                    using (var multi = connection.QueryMultiple(sqlStatement))
                    {
                        listOfTweets = multi.Read<GetTweetDto>().ToList();
                        listOfTweets2 = multi.Read<GetTweetDto>().ToList();
                        listOfTweets3 = multi.Read<GetTweetDto>().ToList();
                    }
                    Tweets = listOfTweets.Concat(listOfTweets2).Concat(listOfTweets3).ToList();

                    foreach (var tweet in Tweets)
                    {
                        var sqlStatement2 = $" Select StatusLike  from Tweets JOIN TweetLikes ON TweetLikes.TweetId = Tweets.Id Where TweetLikes.ApplicationUserId = '{id}'  And TweetId = {tweet.Id} ";
                        tweet.StatusLike = (await connection.ExecuteScalarAsync<bool>(sqlStatement2));

                        var sqlStatement3 = $"Select COUNT (*) from Tweets Join TweetLikes on Tweets.Id = TweetLikes.TweetId Where TweetId = {tweet.Id} AND StatusLike = 1";                    
                        tweet.AmountLikes = (await connection.ExecuteScalarAsync<int>(sqlStatement3));

                    }
                    Tweets = Tweets.OrderByDescending(x => x.currentTime);
                
                    return Ok(Tweets);

                }
            }

            return BadRequest(new AuthResult()
            {
                Error = "Server Error"
            });

        }


        [HttpDelete]
        [Authorize]
        [Route("DeleteTweet")]
        public async Task<IActionResult> DeleteTweet(int tweetId)
        {
            if (ModelState.IsValid)
            {

                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {

                        try
                        {

                            string? sqlStatement = $"DELETE FROM TweetLikes where TweetId = '{tweetId}'";
                            connection.Execute(sqlStatement, transaction: transaction);

                            sqlStatement = $"DELETE FROM Tweets where Id = '{tweetId}'";
                            int rowsaffected = connection.Execute(sqlStatement, transaction: transaction);
                             

                            if (rowsaffected == 0)
                            {
                                throw new Exception("Something went wrong try again later");
                            }

                            transaction.Commit();
                            return Ok("Tweet has successfully been deleted");

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







    }
}
