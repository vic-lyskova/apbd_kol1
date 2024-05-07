using System.Data;
using Kolokwium1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Kolokwium1.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM books WHERE PK = @ID";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<BookWithGenresDTO> GetGenres(int id)
    {
        var query = "SELECT books.PK, books.title, genres.name " +
                        "FROM books " +
                        "JOIN books_genres ON books.PK = books_genres.FK_book " +
                        "JOIN genres ON books_genres.FK_genre = genres.PK " +
                        "WHERE books.PK = @ID";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var bookIdOrdinal = reader.GetOrdinal("PK");
        var titleOrdinal = reader.GetOrdinal("title");
        var nameOrdinal = reader.GetOrdinal("name");

        BookWithGenresDTO bookWithGenresDTO = null;
        while (await reader.ReadAsync())
        {
            if (bookWithGenresDTO is not null)
            {
                bookWithGenresDTO.Genres.Add(reader.GetString(nameOrdinal));
            }
            else
            {
                bookWithGenresDTO = new BookWithGenresDTO()
                {
                    Id = reader.GetInt32(bookIdOrdinal),
                    Title = reader.GetString(titleOrdinal)
                };
            }
        }

        if (bookWithGenresDTO is null)
        {
            throw new Exception();
        }

        return bookWithGenresDTO;
    }

    public async Task<bool> DoesGenreExist(int id)
    {
        var query = "SELECT 1 FROM genres WHERE PK = @ID";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task AddBookWithGenres(NewBookWithGenresDTO newBookWithGenresDto)
    {
        {
            var insert = "INSERT INTO books VALUES(@Title); SELECT @@IDENTITY AS ID";
	    
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();
	    
            command.Connection = connection;
            command.CommandText = insert;
	    
            command.Parameters.AddWithValue("@Title", newBookWithGenresDto.Title);
	    
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();
            command.Transaction = transaction as SqlTransaction;
	    
            try
            {
                var id = await command.ExecuteScalarAsync();
    
                foreach (var genre in newBookWithGenresDto.Genres)
                {
                    command.Parameters.Clear();
                    command.CommandText = "INSERT INTO books_genres VALUES(@BookId, @GenreID)";
                    command.Parameters.AddWithValue("@BookId", id);
                    command.Parameters.AddWithValue("@GenreId", genre);

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}