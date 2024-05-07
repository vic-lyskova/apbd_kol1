using Kolokwium1.Models.DTOs;
using Kolokwium1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private IBooksRepository _booksRepository;

    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }

    [HttpGet("{id}/genres")]
    public async Task<IActionResult> GetGenres(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
        {
            return NotFound("Book with id {id} not found");
        }

        var bookWithGenres = await _booksRepository.GetGenres(id);

        return Ok(bookWithGenres);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(NewBookWithGenresDTO newBookWithGenresDto)
    {
        foreach (var genre in newBookWithGenresDto.Genres)
        {
            if (!await _booksRepository.DoesGenreExist(genre))
                return NotFound($"Genre with id  not found");
        }

        await _booksRepository.AddBookWithGenres(newBookWithGenresDto);

        return Created(Request.Path.Value ?? "api/books", newBookWithGenresDto);
    }
    
}