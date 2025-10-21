using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService bookService, ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    /// <summary>
    /// 获取图书详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BookDto>>> GetById(int id)
    {
        _logger.LogInformation("Getting book with ID: {BookId}", id);
        var result = await _bookService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 搜索图书（支持分页、关键词、分类、价格筛选）
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResponse<BookDto>>>> Search([FromQuery] SearchBooksRequest request)
    {
        _logger.LogInformation("Searching books with keyword: {Keyword}, category: {Category}",
            request.Keyword, request.Category);

        var result = await _bookService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// 创建图书（需要管理员权限）
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<BookDto>>> Create([FromBody] CreateBookRequest request)
    {
        _logger.LogInformation("Creating book: {Title}", request.Title);
        var result = await _bookService.CreateAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// 更新图书（需要管理员权限）
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<BookDto>>> Update(int id, [FromBody] UpdateBookRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<BookDto>.ErrorResponse("ID mismatch"));
        }

        _logger.LogInformation("Updating book with ID: {BookId}", id);
        var result = await _bookService.UpdateAsync(request);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 删除图书（需要管理员权限）
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        _logger.LogInformation("Deleting book with ID: {BookId}", id);
        var result = await _bookService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
