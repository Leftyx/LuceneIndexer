using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneIndexer
{
    public class SearchResult
    {
        public SearchResult()
        {
            this.Documents = new List<Lucene.Net.Documents.Document>();
        }

        public string Expression { get; set; }
        public int TotalDocumentsMatching { get; set; }
        public IList<Lucene.Net.Documents.Document> Documents { get; set; }
    }
}
