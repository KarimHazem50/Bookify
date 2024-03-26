using Bookify.Domain.Dtos;

namespace Bookify.Web.Extensions
{
    public static class FormExtensions
    {
        public static FiltrationDto GetFilters(this IFormCollection form)
        {
            var sortColumnIndex = form["order[0][column]"];

            return new FiltrationDto(
               Skip: int.Parse(form["start"]!),
               PageSize: int.Parse(form["length"]!),
               SortColumnIndex: sortColumnIndex!,
               SortColumnDirection: form["order[0][dir]"]!,
               SortColumnName: form[$"columns[{sortColumnIndex}][name]"]!,
               SearchValue: form["search[value]"]
           );
        }
    }
}
