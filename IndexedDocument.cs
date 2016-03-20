using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneIndexer
{
    public class IndexedDocument
    {
        public IndexedDocument()
        {
            this.Categories = new List<Guid>();
        }

        public Guid Id { get; set; }
        public string DisplayValue { get; set; }
        public IList<Guid> Categories { get; set; }
    }
}
