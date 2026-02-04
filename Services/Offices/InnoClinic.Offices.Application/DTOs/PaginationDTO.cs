namespace InnoClinic.Offices.Application.DTOs
{
    public record PaginationParameters
    {
        public int PageNumber { get; init; } = 1;

        public const int PageSize = 10;
    }
}