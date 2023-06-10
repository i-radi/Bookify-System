namespace Bookify.Web.Extensions;

public static class FormCollectionExtensions
{
    public static GetFilteredDto GetFilters(this IFormCollection form)
    {
        var sortColumnIndex = form["order[0][column]"];

        return new GetFilteredDto(
                Skip: int.Parse(form["start"]!),
                PageSize: int.Parse(form["length"]!),
                SearchValue: form["search[value]"]!,
                SortColumnIndex: sortColumnIndex!,
                SortColumn: form[$"columns[{sortColumnIndex}][name]"]!,
                SortColumnDirection: form["order[0][dir]"]!
            );
    }
}