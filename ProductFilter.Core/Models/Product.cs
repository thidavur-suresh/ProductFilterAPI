using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductFilter.Core.Models
{
    public class Product
    {
        public string? title { get; set; }
        public decimal price { get; set; }
        public List<string>? sizes { get; set; }
        public string? description { get; set; }
    }
    public class APIKeys
    {
        public string? primary { get; set; }
        public string? secondary { get; set; }
    }

    public class SourceData
    {
        public List<Product>? products { get; set; }
        public APIKeys? apiKeys { get; set; }
    }
}
