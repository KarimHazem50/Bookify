namespace Bookify.Domain.Dtos
{
    public record FiltrationDto(
        int Skip,
        int PageSize,
        string SortColumnIndex,
        string SortColumnDirection,
        string SortColumnName,
        string? SearchValue
    );
}
