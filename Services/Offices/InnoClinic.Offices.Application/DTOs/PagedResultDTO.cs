namespace InnoClinic.Offices.Application.DTOs
{
    public record PagedResult<T>(IEnumerable<T> Items, int PageNumber, int PageSize, long TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}