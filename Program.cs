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

var board = new Team[3, 3];
Team currentTeam = Team.O;
Team nextTeam = Team.X;

app.MapPost("/maketurn{x:int},{y:int}", async (HttpContext context, int x, int y) =>
    {
        if (x < 0 || x > 2 || y < 0 || y > 2)
        {
            return context.Response.WriteAsJsonAsync(new
                { error = "Invalid coordinates", error_code = "400 Bad Request" });
        }

        if (board[x, y] != Team.None)
        {
            return context.Response.WriteAsJsonAsync(new
                { error = "Invalid move, cell already taken", error_code = "400 Bad Request" });
        }

        board[x, y] = currentTeam;

        if (CheckWin(board) || CheckDraw(board))
        {
            var boardList2 = ConvertBoardToList(board);
            return context.Response.WriteAsJsonAsync(boardList2);
        }
        
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
        return context.Response.WriteAsJsonAsync(boardList);
    })
    .WithName("MakeTurn")
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
    .WithName("GetRandomTeam")
    .WithOpenApi();

app.MapGet("/resetboard", context =>
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
        var boardList = ConvertBoardToList(board);
        return context.Response.WriteAsJsonAsync(boardList);
    }
);

app.MapGet("/getlastteam", context => { return context.Response.WriteAsJsonAsync(currentTeam); });

app.Run();

[Serializable]
public class MakeTurnRequest
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Team { get; set; }
}

[Serializable]
enum Team : byte
{
    None = 0,
    O = 1,
    X = 2
}