using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Query;
using PRN231.TrialTest.Library.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PRN231.TrialTest.API.Helper;

namespace PRN231.TrialTest.API.Controllers
{
    [Route("odata/Players")]
    [ApiController]
    public class FootballPlayersController : ODataController
    {
        private readonly EnglishPremierLeague2024DBContext _context;

        public FootballPlayersController(EnglishPremierLeague2024DBContext context)
        {
            _context = context;
        }

        // GET: odata/FootballPlayers
        [HttpGet]
        [EnableQuery]
        [Authorize(Roles = "2,1")]
        public IActionResult GetPlayers()
        {
            var players = _context.FootballPlayers.Include(p => p.FootballClub);
            return Ok(players);
        }

        // GET: odata/FootballPlayers/{id}
        [HttpGet("{id}")]
        [EnableQuery]
        [Authorize(Roles = "2,1")]
        public async Task<IActionResult> GetPlayer([FromRoute] string id)
        {
            var player = await _context.FootballPlayers
                .Include(p => p.FootballClub)
                .FirstOrDefaultAsync(p => p.FootballPlayerId == id);

            if (player is null)
            {
                return NotFound(new { msg = "Player not found!" });
            }

            return Ok(player);
        }

        // DELETE: odata/FootballPlayers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeletePlayer([FromRoute] string id)
        {
            var player = await _context.FootballPlayers.FindAsync(id);

            if (player is null)
            {
                return NotFound(new { msg = "Player not found!" });
            }

            _context.FootballPlayers.Remove(player);
            await _context.SaveChangesAsync();

            return Ok(new { msg = "Player deleted!" });
        }

        // POST: odata/FootballPlayers
        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AddPlayer([FromBody] AddPlayerReq player)
        {
            if (!ValidateAddPlayerRequest(player) || !ModelState.IsValid)
            {
                return BadRequest(new { msg = "Invalid input!" });
            }

            var clubExists = await _context.FootballClubs.AnyAsync(c => c.FootballClubId == player.FootballClubID);

            if (!clubExists)
            {
                return BadRequest(new { msg = "Club not found!" });
            }
            
            var newPlayer = new FootballPlayer
            {
                FootballPlayerId = StringHelper.GenerateRandomString(),
                FullName = player.FullName,
                Achievements = player.Achievements,
                Birthday = player.Birthday,
                PlayerExperiences = player.PlayerExperiences,
                Nomination = player.Nomination,
                FootballClubId = player.FootballClubID
            };

            _context.FootballPlayers.Add(newPlayer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlayer), new { id = newPlayer.FootballPlayerId }, newPlayer);
        }

        // PUT: odata/FootballPlayers/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdatePlayer([FromRoute] string id, [FromBody] AddPlayerReq player)
        {
            if (!ModelState.IsValid || !ValidateAddPlayerRequest(player))
            {
                return BadRequest(new { msg = "Invalid input!" });
            }

            var playerToUpdate = await _context.FootballPlayers.FindAsync(id);
            if (playerToUpdate is null)
            {
                return NotFound(new { msg = "Player not found!" });
            }

            var clubExists = await _context.FootballClubs.AnyAsync(c => c.FootballClubId == player.FootballClubID);
            if (!clubExists)
            {
                return BadRequest(new { msg = "Club not found!" });
            }

            playerToUpdate.FullName = player.FullName;
            playerToUpdate.Achievements = player.Achievements;
            playerToUpdate.Birthday = player.Birthday;
            playerToUpdate.PlayerExperiences = player.PlayerExperiences;
            playerToUpdate.Nomination = player.Nomination;
            playerToUpdate.FootballClubId = player.FootballClubID;

            await _context.SaveChangesAsync();
            return Ok(playerToUpdate);
        }

        private bool ValidateAddPlayerRequest(AddPlayerReq player)
        {
            if (string.IsNullOrWhiteSpace(player.FullName) ||
                string.IsNullOrWhiteSpace(player.Achievements) ||
                string.IsNullOrWhiteSpace(player.Nomination) ||
                string.IsNullOrWhiteSpace(player.PlayerExperiences) ||
                string.IsNullOrWhiteSpace(player.FootballClubID))
            {
                return false;
            }

            var words = player.FullName.Split(' ');
            var fullnameRegex = new Regex(@"^[A-Z][a-zA-Z0-9@#]*$");
            foreach (var word in words)
            {
                if (!fullnameRegex.IsMatch(word))
                {
                    return false;
                }
            }

            if (player.Birthday >= new DateTime(2007, 1, 1))
            {
                return false;
            }

            if (player.Achievements.Length < 9 || player.Achievements.Length > 100 ||
                player.Nomination.Length < 9 || player.Nomination.Length > 100)
            {
                return false;
            }

            return true;
        }
    }

    public record GetPlayersReq(string? Achivements, int? Nomination);

    public record GetPlayersRes(string FootballPlayerID, string? FullName, string? Achievements, DateTime? Birthday, string? PlayerExperiences, string? Nomination, string? FootballClubID, string? FootballClubName);

    public record AddPlayerReq(string FullName, string Achievements, DateTime Birthday, string PlayerExperiences, string Nomination, string FootballClubID);
}
