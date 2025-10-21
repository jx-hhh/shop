using CatalogService.Data;
using CatalogService.Models;
using Shared.DTOs;

namespace CatalogService.Services;

public interface IBookService
{
    Task<ApiResponse<BookDto>> GetByIdAsync(int id);
    Task<ApiResponse<PagedResponse<BookDto>>> SearchAsync(SearchBooksRequest request);
    Task<ApiResponse<BookDto>> CreateAsync(CreateBookRequest request);
    Task<ApiResponse<BookDto>> UpdateAsync(UpdateBookRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class BookService : IBookService
{
    private readonly IBookRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<BookService> _logger;

    public BookService(
        IBookRepository repository,
        ICacheService cacheService,
        ILogger<BookService> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ApiResponse<BookDto>> GetByIdAsync(int id)
    {
        // 尝试从缓存获取
        var cacheKey = $"book:{id}";
        var cachedBook = await _cacheService.GetAsync<BookDto>(cacheKey);
        if (cachedBook != null)
        {
            _logger.LogInformation("Book {BookId} retrieved from cache", id);
            return ApiResponse<BookDto>.SuccessResponse(cachedBook);
        }

        // 从数据库获取
        var book = await _repository.GetByIdAsync(id);
        if (book == null)
        {
            return ApiResponse<BookDto>.ErrorResponse("Book not found");
        }

        var bookDto = MapToDto(book);

        // 存入缓存
        await _cacheService.SetAsync(cacheKey, bookDto);

        return ApiResponse<BookDto>.SuccessResponse(bookDto);
    }

    public async Task<ApiResponse<PagedResponse<BookDto>>> SearchAsync(SearchBooksRequest request)
    {
        var (books, totalCount) = await _repository.GetPagedAsync(request);

        var pagedResponse = new PagedResponse<BookDto>
        {
            Items = books.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResponse<BookDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<BookDto>> CreateAsync(CreateBookRequest request)
    {
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            ISBN = request.ISBN,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category,
            ImageUrl = request.ImageUrl
        };

        var bookId = await _repository.CreateAsync(book);
        book.Id = bookId;

        var bookDto = MapToDto(book);

        // 存入缓存
        await _cacheService.SetAsync($"book:{bookId}", bookDto);

        return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book created successfully");
    }

    public async Task<ApiResponse<BookDto>> UpdateAsync(UpdateBookRequest request)
    {
        var existingBook = await _repository.GetByIdAsync(request.Id);
        if (existingBook == null)
        {
            return ApiResponse<BookDto>.ErrorResponse("Book not found");
        }

        existingBook.Title = request.Title;
        existingBook.Author = request.Author;
        existingBook.ISBN = request.ISBN;
        existingBook.Description = request.Description;
        existingBook.Price = request.Price;
        existingBook.Stock = request.Stock;
        existingBook.Category = request.Category;
        existingBook.ImageUrl = request.ImageUrl;

        var success = await _repository.UpdateAsync(existingBook);
        if (!success)
        {
            return ApiResponse<BookDto>.ErrorResponse("Failed to update book");
        }

        // 清除缓存
        await _cacheService.RemoveAsync($"book:{request.Id}");

        var bookDto = MapToDto(existingBook);
        return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse("Failed to delete book");
        }

        // 清除缓存
        await _cacheService.RemoveAsync($"book:{id}");

        return ApiResponse<bool>.SuccessResponse(true, "Book deleted successfully");
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Description = book.Description,
            Price = book.Price,
            Stock = book.Stock,
            Category = book.Category,
            ImageUrl = book.ImageUrl,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };
    }
}
