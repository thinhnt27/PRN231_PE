using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN231.TrialTest.Library.Repo;
using PRN231.TrialTest.API.Helper;
using PRN231.TrialTest.Library.Models;

namespace PRN231.TrialTest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FootballPlayersController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;

        public FootballPlayersController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "2,1")]
        public async Task<IActionResult> GetPlayers([FromQuery] GetPlayersReq req)
        {
            string achive = req.Achivements?.Trim() ?? "";
            string nomi = req.Nomination?.ToString() ?? "";
            //int year = req.PublishYear ?? -1;

            var players = await _unitOfWork
                .PlayerRepo
                .GetQueryable();

            if (achive != "")
            {
                players = players.Where(p => p.Achievements!.ToUpper().Contains(achive.ToUpper()));
            }
            if (nomi != "")
            {
                players = players.Where(p => p.Nomination!.ToUpper().Contains(nomi.ToUpper()));
            }
            //if (year != -1)
            //{
            //    paints = paints.Where(p => p.PublishYear == year);
            //}

            players = players.Include(p => p.FootballClub);


            var listPaint = await players.ToListAsync();

            var result = listPaint
                .Select(p => new GetPlayersRes(p.FootballPlayerId, p.FullName, p.Achievements, p.Birthday, p.PlayerExperiences, p.Nomination, p.FootballClubId, p.FootballClub?.ClubName));

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "2,1")]
        public async Task<IActionResult> GetPlayer([FromRoute] string id)
        {
            var player = await _unitOfWork.PlayerRepo.GetByIdAsync(id);

            if (player is null)
            {
                return NotFound(new { msg = "Player not found!" });
            }

            player.FootballClub = await _unitOfWork.ClubRepo.GetByIdAsync(player.FootballClubId!);

            return Ok(player);
        }


        [HttpDelete]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeletePlayer([FromQuery] string id)
        {
            var player = await _unitOfWork.PlayerRepo.GetByIdAsync(id);

            if (player is null)
            {
                return NotFound(new { msg = "Player not found!" });
            }

            _unitOfWork.PlayerRepo.Delete(player);
            await _unitOfWork.SaveAsync();

            return Ok(new { msg = "Player deleted!" });
        }

        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AddPaint([FromBody] AddPlayerReq player)
        {
            if (!ValidateAddPlayerRequest(player) || !ModelState.IsValid)
            {
                return BadRequest(new { msg = "Invalid input!" });
            }

            var isExitedStyle = await _unitOfWork.ClubRepo.GetByIdAsync(player.FootballClubID);

            if (isExitedStyle is null)
            {
                return BadRequest(new { msg = "Club not found!" });
            }

            var newPlayer = player.Adapt<FootballPlayer>();
            newPlayer.FootballPlayerId = StringHelper.GenerateRandomString();
            //newPlayer.CreatedDate = DateTime.Now;
            newPlayer.FootballClubId = player.FootballClubID;
            await _unitOfWork.PlayerRepo.InsertAsync(newPlayer);
            await _unitOfWork.SaveAsync();

            return Ok(newPlayer);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdatePaint([FromRoute] string id, [FromBody] AddPlayerReq player)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { msg = "Invalid input!" });
            }

            var playerToUpdate = await _unitOfWork.PlayerRepo.GetByIdAsync(id);
            if (playerToUpdate is null)
            {
                return NotFound(new { msg = "Player not found!" });
            }

            if (!ValidateAddPlayerRequest(player) || !ModelState.IsValid)
            {
                return BadRequest(new { msg = "Invalid input!" });
            }

            var isExitedStyle = await _unitOfWork.ClubRepo.GetByIdAsync(player.FootballClubID);

            if (isExitedStyle is null)
            {
                return BadRequest(new { msg = "FootballClubID not found!" });
            }

            playerToUpdate.FullName = player.FullName;
            playerToUpdate.Achievements = player.Achievements;
            playerToUpdate.Birthday = player.Birthday;
            playerToUpdate.PlayerExperiences = player.PlayerExperiences;
            playerToUpdate.Nomination = player.Nomination;
            playerToUpdate.FootballClubId = player.FootballClubID;
            

            await _unitOfWork.SaveAsync();
            return Ok(playerToUpdate);
        }

        private bool ValidateAddPlayerRequest(AddPlayerReq player)
        {
            // Check all fields are present
            if (string.IsNullOrWhiteSpace(player.FullName) ||
                string.IsNullOrWhiteSpace(player.Achievements) ||
                string.IsNullOrWhiteSpace(player.Nomination) ||
                string.IsNullOrWhiteSpace(player.PlayerExperiences) ||
                string.IsNullOrWhiteSpace(player.FootballClubID))
            {
                return false;
            }

            // Validate FullName format
            //Fullname includes a-z, A-Z, space, @, # and digit 0-9. Each word of the Fullname must begin with the capital letter.
            var words = player.FullName.Split(' ');
            var fullnameRegex = new Regex(@"^[A-Z][a-zA-Z0-9@#]*$");
            foreach (var word in words)
            {
                if (!fullnameRegex.IsMatch(word))
                {
                    return false;
                }
            }
            //////Fullname includes a-z, A-Z, space, @, # and digit 0-9. Each word of the Fullname must begin with the capital letter.And the Fullname must have 10 words.
            //var regex = new Regex(@"^([A-Z][a-zA-Z0-9@#]*\s?){10}$");

            // Validate Birthday
            if (player.Birthday >= new DateTime(2007, 1, 1))
            {
                return false;
            }

            // Validate length for Achievements and Nomination
            if (player.Achievements.Length < 9 || player.Achievements.Length > 100 ||
                player.Nomination.Length < 9 || player.Nomination.Length > 100)
            {
                return false;
            }

            return true;
        }


        //private bool PaintValidate(AddPaintReq paint)
        //{
        //    // Value for PaintingName includes a-z, A-Z, space and digit 0-9. Each word of the PaintingName must begin with the capital letter.
        //    var regex = new Regex(@"^([A-Z][a-z0-9]*\s?)+$");

        //    if (!regex.IsMatch(paint.PaintingName))
        //    {
        //        return false;
        //    }

        //    if (paint.PublishYear < 1000)
        //    {
        //        return false;
        //    }

        //    if (paint.Price < 0)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        ////valid PaintingName must have 10 characters and begin with the capital letter and end with the special character is ! or #
        //private bool PaintingNameValidate(string paintingName)
        //{
        //    var regex = new Regex(@"^[A-Z][a-zA-Z0-9]{8}[!#]$");

        //    return regex.IsMatch(paintingName);
        //}

    }

    public record GetPlayersReq(string? Achivements, int? Nomination);

    public record GetPlayersRes(string FootballPlayerID, string? FullName, string? Achievements, DateTime? Birthday, string? PlayerExperiences, string? Nomination,  string? FootballClubID, string? FootballClubName);

    public record AddPlayerReq(string FullName, string Achievements, DateTime Birthday, string PlayerExperiences, string Nomination, string FootballClubID);
}
