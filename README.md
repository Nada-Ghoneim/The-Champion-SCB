# The-Champion-SCB
This project is a Chess League System built using ASP.NET Core Web API and SQL Server. It manages participants, matches, and leagues, supporting up to 12 players with automated group assignments and match scheduling.
Features
Participant Management: Add and manage participants in the league.
Group Assignments: Randomly assigns participants into 3 groups based on league requirements.
Match Scheduling: Automatically schedules matches starting the next day after the last match, with up to 3 matches per day.
Results Management: Update the match results and track winners.
SQL Database: Stores all participants, groups, matches, and rounds in a SQL Server database.


Prerequisites
.NET Core SDK (6.0 or later)
SQL Server
Visual Studio or VS Code for development
Postman or Swagger for testing API endpoints


Getting Started
Step 1: Clone the repository
git clone https://github.com/Nada-Ghoneim/The-Champion-SCB.git

Step 2: Set up the database
Update the connection string in the appsettings.json file to point to your local SQL Server instance.
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-V2D9KIV\\MSSQLSERVER01;Database=ChessLeagueDB;Trusted_Connection=True;"
}

Step 4: Test the API
You can test the API using Postman or Swagger. After running the application, navigate to https://localhost:5001/swagger to view and test all the available endpoints.

Key API Endpoints
Submit Participant: POST /api/Participant/SubmitParticipant
Get Participants: GET /api/Participant/GetParticipants
Assign Groups: POST /api/Participant/AssignGroups
Schedule Matches: Automatically schedules matches after groups are assigned.
Update Match Results: To mark a match as completed and update the winner.

Project Structure
Controllers/: Contains the API controllers for managing participants, groups, and matches.
Models/: Defines the models representing the database tables (e.g., Participant, Match, Group).
DatabaseHelper/: A utility class that handles database queries and commands.
appsettings.json: Configuration file for connection strings and application settings.
Database Schema
Participants: Stores participant information (Name, Email).
Groups: Stores the group assignments for participants.
Matches: Stores details about each match (Player1, Player2, Winner, MatchTime, etc.).
Rounds: Defines rounds for the matches.
Leagues: Stores the overall league information.
Future Improvements
Email Notifications: Send congratulatory emails to the champion.
League History: Track past leagues and champions.
User Authentication: Add user authentication for managing participants.
Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, feel free to open an issue or submit a pull request.

License
This project is licensed under the MIT License.
