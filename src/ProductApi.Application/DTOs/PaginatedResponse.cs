namespace ProductApi.Application.DTOs;

/// <summary>
/// Generic paginated response DTO.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// The list of items on this page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
