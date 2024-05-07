using Kolokwium1.Models.DTOs;

namespace Kolokwium1.Repositories;

public interface IBooksRepository
{
    public Task<bool> DoesBookExist(int id);
    public Task<BookWithGenresDTO> GetGenres(int id);
    public Task<bool> DoesGenreExist(int id);
    public Task AddBookWithGenres(NewBookWithGenresDTO newBookWithGenresDto);
}