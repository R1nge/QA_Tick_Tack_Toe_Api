using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var board = new Team[3, 3];

app.MapPost("/maketurn{x:int},{y:int},{team:int}", async (HttpContext context, int x, int y, int team) =>
    {
        if (team <= 0 || team > Enum.GetNames(typeof(Team)).Length)
        {
            return context.Response.WriteAsJsonAsync(new { error = "Invalid team", error_code = "400 Bad Request" });    
        }
        
        if (x < 0 || x > 2 || y < 0 || y > 2)
        {
            return context.Response.WriteAsJsonAsync(new { error = "Invalid coordinates", error_code = "400 Bad Request" });    
        }
        
        if (board[x, y] != Team.None)
        {
            return context.Response.WriteAsJsonAsync(new { error = "Invalid move, cell already taken", error_code = "400 Bad Request" });    
        }
        
        board[x, y] = (Team)team;
        
        var boardList = ConvertBoardToList(board);
        return context.Response.WriteAsJsonAsync(boardList);
    })
    .WithName("MakeTurn")
    .WithDescription("Make a turn on the board")
    .WithOpenApi();

static List<List<Team>> ConvertBoardToList(Team[,] board)
{
    var result = new List<List<Team>>();
    for (int i = 0; i < board.GetLength(0); i++)
    {
        var row = new List<Team>();
        for (int j = 0; j < board.GetLength(1); j++)
        {
            row.Add(board[i, j]);
        }
        result.Add(row);
    }
    return result;
}

app.MapGet("/getboard", () =>
    {
        var boardList = ConvertBoardToList(board);
        return boardList;
    })
    .WithName("GetRandomTeam")
    .WithOpenApi();

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

[Serializable]
public class MakeTurnRequest
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Team { get; set; }
}

[Serializable]
enum Team
{
    None,
    X,
    Y
}