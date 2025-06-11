using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class PagedResult<T>
    {

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("page_index")]
        public int PageIndex { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("items")]
        public List<T> Items { get; set; }

        public PagedResult(List<T> items, int totalCount, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = items.Count;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}
