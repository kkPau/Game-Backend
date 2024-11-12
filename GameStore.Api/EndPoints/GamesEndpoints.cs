using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.EndPoints;

public static class GamesEndpoints 
{
    const string GetGameEndPointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app){
        var group = app.MapGroup("games").WithParameterValidation();

        // GET all games in route /games
        group.MapGet("/", async (GameStoreContext dbContext) => 
            await dbContext.Games
                .Include(game => game.Genre)
                .Select(game => game.ToGameSummaryDto())
                .AsNoTracking()
                .ToListAsync()
        );

        // GET a game by ID in route /games/{id}
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) => {
            Game? game = await dbContext.Games.FindAsync(id);

            return game is null ? Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
        });

        // POST a new game in route /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) => {
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);

            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetGameEndPointName, new {id = game.Id}, game.ToGameDetailsDto());
        });

        // PUT update a game by ID in route /games/{id}
        group.MapPut("/{id}", async (int id, UpdateGameDto UpdatedGame, GameStoreContext dbContext)=>{
            var existingGame = await dbContext.Games.FindAsync(id);

            if(existingGame is null) return Results.NotFound();

            dbContext.Entry(existingGame)
                .CurrentValues
                .SetValues(UpdatedGame.ToEntity(id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE a game by ID in route /games/{id}
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) => {
            await dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}
