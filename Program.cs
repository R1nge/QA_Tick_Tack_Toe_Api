using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();

if (!app.Environment.IsDevelopment())
{
    // app.UseHttpsRedirection();
}


var board = new Team[3, 3];
Team currentTeam = Team.O;
Team nextTeam = Team.X;
var lastTurn = new LastTurn(-1, -1);


app.MapPost("/maketurn{x:int},{y:int}", async (HttpContext context, int x, int y) =>
    {
        Console.WriteLine("Making turn at " + x + " " + y);

        object response = null;

        if ((x == 6 && y == 9) || (x == 69 && y == 69) || (x == 69) || (y == 69))
        {
            response = new { error = "Oh, my", error_code = "69" };
            lastTurn = new LastTurn(x, y);
        }
        else if ((x == 666) || (y == 666))
        {
            response = new { error = "Hallelujah", error_code = "666" };
            lastTurn = new LastTurn(x, y);
        }
        else if (x == 999 || y == 999)
        {
            response = new { error = "Devil", error_code = "999" };
            lastTurn = new LastTurn(x, y);
        }
        else if ((x == 1337 || y == 1337) || (x == 13 && y == 37))
        {
            response = new { error = "Leet", error_code = "1337" };
            lastTurn = new LastTurn(x, y);
        }
        else if (x < 0 || x > 2 || y < 0 || y > 2)
        {
            response = new { error = "Invalid coordinates", error_code = "400 Bad Request" };
        }
        else if (board[x, y] != Team.None)
        {
            response = new { error = "Invalid move, cell already taken", error_code = "400 Bad Request" };
        }
        else
        {
            board[x, y] = currentTeam;
            lastTurn = new LastTurn(x, y);

            if (CheckWin(board) || CheckDraw(board))
            {
                var boardList2 = ConvertBoardToList(board);
                response = boardList2;
            }
            else
            {
                if (currentTeam == Team.None)
                {
                    currentTeam = Team.O;
                    nextTeam = Team.X;
                }
                else if (currentTeam == Team.O)
                {
                    currentTeam = Team.X;
                    nextTeam = Team.O;
                }
                else if (currentTeam == Team.X)
                {
                    currentTeam = Team.O;
                    nextTeam = Team.X;
                }

                var boardList = ConvertBoardToList(board);
                response = boardList;
            }
        }


        await context.Response.WriteAsJsonAsync(response);
    })
    .WithName("Make Turn")
    .WithDescription("Make a turn on the board")
    .WithOpenApi();

static bool CheckWin(Team[,] board)
{
    for (int i = 0; i < 3; i++)
    {
        if (board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2] && board[i, 0] != Team.None)
        {
            return true;
        }
    }

    // Check columns
    for (int i = 0; i < 3; i++)
    {
        if (board[0, i] == board[1, i] && board[1, i] == board[2, i] && board[0, i] != Team.None)
        {
            return true;
        }
    }

    // Check diagonals
    if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2] && board[0, 0] != Team.None)
    {
        return true;
    }

    if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0] && board[0, 2] != Team.None)
    {
        return true;
    }

    return false;
}

static bool CheckDraw(Team[,] board)
{
    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            if (board[i, j] == Team.None)
            {
                return false;
            }
        }
    }

    return true;
}

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
    .WithName("Get Board")
    .WithDescription("Get current board")
    .WithOpenApi();

app.MapGet("/resetboard", async (HttpContext context) =>
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = Team.None;
                }
            }

            currentTeam = Team.O;
            nextTeam = Team.X;
            lastTurn = new LastTurn(-1, -1);
            var boardList = ConvertBoardToList(board);
            return context.Response.WriteAsJsonAsync(boardList);
        }
    )
    .WithName("Reset Board")
    .WithDescription("Reset the board")
    .WithOpenApi();

app.MapGet("/getlastteam", context => { return context.Response.WriteAsJsonAsync(currentTeam); })
    .WithName("Get Last Team")
    .WithDescription("Returns the last team")
    .WithOpenApi();

app.MapGet("/getlastturn", context => { return context.Response.WriteAsJsonAsync(lastTurn); })
    .WithName("Get Last Turn")
    .WithDescription("Returns the last turn")
    .WithOpenApi();

app.Run();

[Serializable]
public class MakeTurnRequest
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Team { get; set; }
}

[Serializable]
public class LastTurn
{
    public int X { get; set; }
    public int Y { get; set; }

    public LastTurn(int x, int y)
    {
        X = x;
        Y = y;
    }
}

[Serializable]
enum Team : byte
{
    None = 0,
    O = 1,
    X = 2
}