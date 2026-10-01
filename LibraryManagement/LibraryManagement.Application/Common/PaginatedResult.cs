namespace LibraryManagement.Application.Common;
public class PaginatedResult<T> {
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;
}
