using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductFilter.Core.Models
{
    public class FilteredResponse
    {
        public List<Product> Products { get; set; }
        public FilterMetadata Filter { get; set; }
    }

    public class FilterMetadata
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public List<string> Sizes { get; set; }
        public string[] CommonWords { get; set; }
    }
}
