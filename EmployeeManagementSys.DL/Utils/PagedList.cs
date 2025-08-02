using System.Collections;
using System.Text.Json.Serialization;

namespace EmployeeManagementSys.DL
{
    public class PagedList<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; }

        [JsonPropertyName("totalPages")]
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage => PageNumber > 1;

        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

     
    }
}