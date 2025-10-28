# MazeChessHighScores

> Azure Functions application for managing high scores for the MazeChess game

## Overview

MazeChessHighScores is a serverless Azure Function application that provides a REST API for managing and retrieving daily high scores for the MazeChess game. Built with .NET 8 and Azure Functions v4, it leverages Azure Cosmos DB for persistent storage and efficient querying of player scores.

## Features

- **Submit High Scores** - POST endpoint to record new player scores
- **Retrieve Daily Leaderboard** - GET endpoint that returns top 10 scores for the current day
- **Health Check** - Ping endpoint for monitoring
- **Serverless Architecture** - Scalable and cost-effective Azure Functions hosting
- **Cosmos DB Integration** - Fast NoSQL database for score persistence

## Tech Stack

- **.NET 8** - Target framework
- **Azure Functions v4** - Serverless compute
- **Azure Cosmos DB** - NoSQL database
- **Application Insights** - Monitoring and telemetry

## API Endpoints

### GET - Retrieve High Scores
```
GET /api/HttpScores
```
Returns the top 10 scores for the current day, ordered by number of moves (ascending).

**Response:**
```json
[
  {
    "id": "guid",
    "username": "player1",
    "moves": 42,
    "time": 123.45,
    "timestamp": "2025-10-28T12:00:00Z"
  }
]
```

### POST - Submit High Score
```
POST /api/HttpScores
```

**Request Body:**
```json
{
  "username": "player1",
  "moves": 42,
  "time": 123.45
}
```

**Response:**
```json
{
  "message": "High score submitted successfully",
  "score": { /* submitted score object */ }
}
```

### GET - Health Check
```
GET /api/ping
```

**Response:**
```json
{
  "message": "pong"
}
```

## Local Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Azure Cosmos DB Emulator](https://docs.microsoft.com/azure/cosmos-db/local-emulator) (optional)

### Configuration

Create a `local.settings.json` file with the following structure:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "COSMOS_DB_ENDPOINT": "your-cosmos-endpoint",
    "COSMOS_DB_KEY": "your-cosmos-key"
  }
}
```

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the function locally
func start
```

The API will be available at:
```
http://localhost:7071/api/HttpScores
```

## Deployment

### Azure Deployment

The function is currently deployed manually using Visual Studio Code:

1. Open the project in VS Code
2. Navigate to the Azure extension pane
3. Click "Deploy to Function App"
4. Select the target Azure Function App

### Production Endpoint

```
https://mazechesschallengehighscorefunction.azurewebsites.net/api/httpscores
```

## Database Schema

### Cosmos DB Configuration

- **Database:** `MazechessScoreDB`
- **Container:** `scores`
- **Partition Key:** `/username`

### HighScore Model

```csharp
{
  "id": "string",           // Unique identifier
  "username": "string",     // Player username (partition key)
  "moves": "int",          // Number of moves taken
  "time": "double",        // Time taken in seconds
  "timestamp": "string"    // ISO 8601 timestamp
}
```

## Project Structure

```
MazeChessHighScores/
├── HttpScores.cs           # Main function with API endpoints
├── Program.cs              # Application entry point
├── host.json              # Function host configuration
├── local.settings.json    # Local environment settings (gitignored)
├── MazeChessHighScores.csproj
└── README.md
```

## Contributing

This is a personal project for the MazeChess game. If you'd like to contribute or report issues, please reach out to the repository owner.

## License

This project is private and proprietary.

## Notes

- High scores are filtered by day (UTC timezone)
- Only the top 10 scores per day are returned
- Scores are ordered by moves (ascending) - fewer moves is better
- Anonymous authentication is enabled for easy game integration