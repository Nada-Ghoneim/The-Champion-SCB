using Microsoft.AspNetCore.Mvc;
using SCB.Models;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Data.SqlClient;
using System.Xml.Linq;

[ApiController]
[Route("api/[controller]")]
public class ParticipantController : ControllerBase
{
    private readonly DatabaseHelper _dbHelper;

    public ParticipantController(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    [HttpPost("SubmitParticipant")]
    public IActionResult SubmitParticipant(Participant participant)
    {
        // Count the existing participants
        string countQuery = "SELECT COUNT(*) FROM Participants";
        int participantCount = (int)_dbHelper.ExecuteScalar(countQuery);

        // Check if the count exceeds 12
        if (participantCount >= 12)
        {
            return BadRequest("Cannot submit participant: The limit of 12 participants has been reached.");
        }
        else
        {
            // Proceed to insert the new participant
            string insertQuery = $"INSERT INTO Participants (Name, Email) VALUES ('{participant.Name}', '{participant.Email}')";
            int result = _dbHelper.ExecuteNonQuery(insertQuery);

            return Ok(result > 0 ? "Participant added successfully." : "Error occurred.");
        }
    }

    [HttpGet("GetParticipants")]
    public IActionResult GetParticipants()
    {
        string query = "SELECT * FROM Participants";
        DataTable result = _dbHelper.ExecuteQuery(query);

        var participants = new List<dynamic>();
        foreach (DataRow row in result.Rows)
        {
            var participant = new
            {
                ID = row["ID"], 
                Name = row["Name"],
                Email = row["Email"]
            };
            participants.Add(participant);
        }

        return Ok(participants);
    }

    [HttpPost("AssignGroups")]
    public IActionResult AssignGroups()
    {
        string query = "SELECT ID FROM Participants";
        DataTable result = _dbHelper.ExecuteQuery(query);

        List<int> participantIds = new List<int>();
        foreach (DataRow row in result.Rows)
        {
            participantIds.Add(Convert.ToInt32(row["ID"]));
        }

        // Shuffle the list of participant IDs
        Random random = new Random();
        participantIds = participantIds.OrderBy(x => random.Next()).ToList();

        // Calculate total participants and group sizes
        int totalParticipants = participantIds.Count;
        int groupCount = 3;
        int baseGroupSize = totalParticipants / groupCount;
        int remainder = totalParticipants % groupCount;

        // Create a list to hold the group assignments
        var groupAssignments = new List<(int GroupNum, int ParticipantID)>();

        int currentParticipantIndex = 0;

        // Distribute participants into groups
        for (int groupNum = 1; groupNum <= groupCount; groupNum++)
        {
            // Determine the size of the current group
            int currentGroupSize = baseGroupSize + (groupNum <= remainder ? 1 : 0);

            for (int j = 0; j < currentGroupSize; j++)
            {
                if (currentParticipantIndex < totalParticipants)
                {
                    groupAssignments.Add((groupNum, participantIds[currentParticipantIndex]));
                    currentParticipantIndex++;
                }
            }
        }

        // Insert group assignments into the database
        foreach (var assignment in groupAssignments)
        {
            string insertGroupQuery = $"INSERT INTO Groups (GroupNum, ParticipantID) VALUES ({assignment.GroupNum}, {assignment.ParticipantID})";
            _dbHelper.ExecuteNonQuery(insertGroupQuery);
        }

        return Ok("Participants assigned to groups successfully.");
    }



    [HttpGet("GetGroups")]
    public IActionResult GetGroups()
    {
        string query = "SELECT * FROM Groups";
        DataTable result = _dbHelper.ExecuteQuery(query);

        var Groups = new List<dynamic>();
        foreach (DataRow row in result.Rows)
        {
            var Group = new
            {
                ID = row["ID"],
                GroupNum = row["GroupNum"],
                ParticipantID = row["ParticipantID"]
            };
            Groups.Add(Group);
        }

        return Ok(Groups);
    }
    [HttpPost("CreateLeagueMatches")]
    public IActionResult CreateLeagueMatches(string leagueName)
    {
        // Step 2: Set the start date to the current date and time
        DateTime startDate = DateTime.UtcNow; // Or use DateTime.Now for local time

        // Step 3: Insert a new league entry into the League table
        string insertLeagueQuery = $@"
    INSERT INTO League (Name, StartDate) 
    VALUES ('{leagueName}', '{startDate:yyyy-MM-dd HH:mm:ss}')";

        // Execute the query to insert the league
        _dbHelper.ExecuteNonQuery(insertLeagueQuery);

        // Get participants grouped
        string groupQuery = "SELECT GroupNum, ParticipantID FROM Groups";
        DataTable groupResult = _dbHelper.ExecuteQuery(groupQuery);

        var groups = new Dictionary<int, List<int>>();

        // Organize participants into groups
        foreach (DataRow row in groupResult.Rows)
        {
            int groupNum = Convert.ToInt32(row["GroupNum"]);
            int participantId = Convert.ToInt32(row["ParticipantID"]);

            if (!groups.ContainsKey(groupNum))
            {
                groups[groupNum] = new List<int>();
            }

            groups[groupNum].Add(participantId);
        }

        int maxMatchesPerDay = 3;
        int roundNumber = 1;
        int matchCounter = 0;
        int dayCounter = 0;

        // Create matches and rounds
        List<(int player1, int? player2, int roundNumber, int groupNum)> matchDetails = new List<(int, int?, int, int)>();

        foreach (var group in groups)
        {
            var participants = group.Value;
            int groupSize = participants.Count;

            // Check if group size is even
            if (groupSize % 2 == 0)
            {
                // Schedule all matches in round 1 for even participants
                for (int i = 0; i < participants.Count; i += 2)
                {
                    int player1 = participants[i];
                    int player2 = participants[i + 1];
                    matchDetails.Add((player1, player2, roundNumber, group.Key));
                }
            }
            else
            {
                // If odd, last participant moves to next round, but others stay in Round 1
                for (int i = 0; i < groupSize - 1; i += 2)
                {
                    int player1 = participants[i];
                    int player2 = participants[i + 1];
                    matchDetails.Add((player1, player2, roundNumber, group.Key));
                }

                // The last lonely participant gets moved to Round 2
                int lonelyParticipant = participants.Last();
                matchDetails.Add((lonelyParticipant, null, roundNumber, group.Key));
            }
        }

        // Call the method to schedule matches
        ScheduleMatches(matchDetails, ref dayCounter, ref matchCounter, maxMatchesPerDay);

        return Ok("Matches created successfully.");
    }



    private void ScheduleMatches(List<(int player1, int? player2, int roundNumber, int groupNum)> matchDetails,
                                  ref int dayCounter, ref int matchCounter, int maxMatchesPerDay)
    {
        // Step 1: Retrieve the last match date from the Matches table
        string lastMatchQuery = "SELECT MAX(MatchTime) FROM Matches";
        object result = _dbHelper.ExecuteScalar(lastMatchQuery);

        // Step 2: Determine the start date
        DateTime startDate;

        if (result != DBNull.Value)
        {
            // If matches exist, start from the next day after the last match
            startDate = Convert.ToDateTime(result).AddDays(1);
        }
        else
        {
            // If no matches exist, start from today's date
            startDate = DateTime.UtcNow.Date;
        }

        // Step 3: Loop through the match details and schedule matches
        foreach (var match in matchDetails)
        {
            int player1 = match.player1;
            int? player2 = match.player2;
            int roundNumber = match.roundNumber;

            // Check if we reached the max matches for the day
            DateTime matchDate = startDate.AddDays(dayCounter); // Initial match date based on dayCounter

            if (matchCounter >= maxMatchesPerDay)
            {
                dayCounter++; // Move to the next day
                matchCounter = 0; // Reset match counter for the new day
                matchDate = startDate.AddDays(dayCounter); // Update matchDate for the new day
            }

            // Step 4: Insert the match into the database
            if (player2 == null)
            {
                // Solo match for odd participants (player2 is NULL)
                string soloMatchQuery = $"INSERT INTO Matches (Player1, Player2, MatchTime, RoundNumber, Winner) " +
                                        $"VALUES ({player1}, NULL, '{matchDate:yyyy-MM-dd}', {roundNumber}, {player1})";
                _dbHelper.ExecuteNonQuery(soloMatchQuery);
            }
            else
            {
                // Regular match between two players
                string matchQuery = $"INSERT INTO Matches (Player1, Player2, MatchTime, RoundNumber) " +
                                    $"VALUES ({player1}, {player2}, '{matchDate:yyyy-MM-dd}', {roundNumber})";
                _dbHelper.ExecuteNonQuery(matchQuery);
            }

            // Step 5: Increment the match counter
            matchCounter++;
        }
    }





    [HttpGet("Winnertonextround")]



    public IActionResult Winnertonextround()
    {
        
        string query = "SELECT * FROM Matches where IsClosed = 0 ";
        DataTable result = _dbHelper.ExecuteQuery(query);

        var Matches = new List<dynamic>();
        foreach (DataRow row in result.Rows)
        {
            var match1 = new
            {
                Player1 = row["Player1"],
                Player2 = row["Player2"],
                //MatchTime = row["MatchTime "],
                RoundNumber = row["RoundNumber"],
            };
            Matches.Add(match1);
        }
        return Ok(Matches);


    }
    [HttpPost("UpdateOpenMatchesbyid")]


    public IActionResult UpdateOpenMatchesbyid(int winnerId,int matchid)
    {
      
            // Update only matches where IsClosed = 0
            string query = $"UPDATE Matches SET Winner = {winnerId}, IsClosed = 1 WHERE ID = {matchid} and IsClosed = 0 ";

            // Create parameters for the SQL query
            var parameters = new List<SqlParameter>
        {
            new SqlParameter("@WinnerId", SqlDbType.Int) { Value = winnerId }
        };

            // Execute the query
            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);

            // Check if the update was successful
            if (rowsAffected > 0)
            {
                return Ok($"{rowsAffected} open matches updated successfully.");
            }
            else
            {
                return BadRequest("No open matches were updated.");
            }
        }

    [HttpPost("CreateLeagueMatcheswnewround")]
    public IActionResult CreateLeagueMatchesnewround()
    {
        // Get the maximum round number from Matches table
        string maxRoundQuery = "SELECT MAX(RoundNumber) AS MaxRound FROM Matches";
        DataTable maxRoundResult = _dbHelper.ExecuteQuery(maxRoundQuery);

        // Initialize roundNumber
        int roundNumber = 1; // Default value if no rounds are found
        if (maxRoundResult.Rows.Count > 0)
        {
            roundNumber = Convert.ToInt32(maxRoundResult.Rows[0]["MaxRound"]);
        }

        // Get the winners from matches in the previous round
        string winnersQuery = $"SELECT Winner FROM Matches WHERE RoundNumber = {roundNumber}";
        DataTable winnersResult = _dbHelper.ExecuteQuery(winnersQuery);

        var winnersList = new List<int>();

        // Organize winners into a list
        foreach (DataRow row in winnersResult.Rows)
        {
            // Check if the Winner field is not DBNull
            if (row["Winner"] != DBNull.Value)
            {
                int winnerId = Convert.ToInt32(row["Winner"]);
                winnersList.Add(winnerId);
            }
        }

        // Check if there is only one winner in the maximum round
        if (winnersList.Count == 1)
        {
            string maxMatchDateQuery = "SELECT MAX(MatchTime) FROM Matches";
            object maxDateResult = _dbHelper.ExecuteScalar(maxMatchDateQuery);
            DateTime endDate = Convert.ToDateTime(maxDateResult);

            string leagueId = "SELECT MAX(Id) FROM League";
            object lid = _dbHelper.ExecuteScalar(leagueId);

            int idl = Convert.ToInt32(lid);

            // Get the winner's name from the Participants table
            string winnerId = winnersList[0].ToString();

            string updateLeagueQuery = $@"UPDATE League SET Champion = {winnerId}, EndDate = '{endDate:yyyy-MM-dd HH:mm:ss}' WHERE ID = {idl}";
            int rowsAffected = _dbHelper.ExecuteNonQuery(updateLeagueQuery);

            string winnerNameQuery = $"SELECT Name FROM Participants WHERE ID = {winnerId}";
            DataTable winnerNameResult = _dbHelper.ExecuteQuery(winnerNameQuery);

            if (winnerNameResult.Rows.Count > 0)
            {
                string winnerName = winnerNameResult.Rows[0]["Name"].ToString();
                return Ok($"The winner is {winnerName}.");
            }
            else
            {
                return NotFound("Winner's name not found.");
            }
        }

        // Prepare to schedule matches for the next round
        int dayCounter = 1;
        int matchCounter = 0;
        int maxMatchesPerDay = 3;
        int newRoundNumber = roundNumber + 1; // Increment the round number for the new round

        // Create matches for the winners
        int winnersCount = winnersList.Count;

        // Prepare a list to hold match details
        var matchDetails = new List<(int player1, int? player2, int roundNumber, int groupNum)>();

        // Check if the number of winners is even or odd
        if (winnersCount % 2 == 0)
        {
            // Schedule all matches for even winners
            for (int i = 0; i < winnersCount; i += 2)
            {
                int player1 = winnersList[i];
                int player2 = winnersList[i + 1];
                matchDetails.Add((player1, player2, newRoundNumber, 1));
            }
        }
        else
        {
            // If odd, last winner gets a bye
            for (int i = 0; i < winnersCount - 1; i += 2)
            {
                int player1 = winnersList[i];
                int player2 = winnersList[i + 1];
                matchDetails.Add((player1, player2, newRoundNumber, 1));
            }

            // The last lonely participant (odd one) gets moved to Round 2
            int lonelyParticipant = winnersList.Last();
            matchDetails.Add((lonelyParticipant, null, newRoundNumber, 1));
        }

        // Call the ScheduleMatches method to schedule matches
        ScheduleMatches(matchDetails, ref dayCounter, ref matchCounter, maxMatchesPerDay);

        return Ok("New round matches created successfully.");
    }








}


