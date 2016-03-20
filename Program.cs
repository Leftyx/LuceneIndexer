using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LuceneIndexer
{
    class Program
    {
        private static Guid category1 = Guid.Parse("5576CE1A-773C-4F4F-BD13-FE24A301B159");
        private static Guid category2 = Guid.Parse("A2D55CB2-015F-4394-B17D-640EC9DE8347");
        private static Guid category3 = Guid.Parse("2907145A-3BC6-4007-9B27-6BDEE3861C39");

        static void Main(string[] args)
        {
            var directoryInfo = new DirectoryInfo(@"C:\Lucene\Objects");
            ClearIndexes(directoryInfo);

            SearchResult firstResult = null;
            SearchResult secondResult = null;
            SearchResult thirdResult = null;

            string[] searchFields = { "displayvalue" };
            Guid[] categories = { category1, category2 };
            Guid[] notFoundCategory = { category3 };

            using (var directory = Lucene.Net.Store.FSDirectory.Open(directoryInfo))
            using (var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            {
                using (var searcher = new Searcher(directory, analyzer))
                {
                    foreach (var document in FetchDocuments())
                    {
                        searcher.AddDocument(document);
                    }

                    firstResult = searcher.Search("First Secon*", searchFields, 10);
                    secondResult = searcher.Search("First Secon*", searchFields, "categoryid", categories, 10);
                    thirdResult = searcher.Search("First Secon*", searchFields, "categoryid", notFoundCategory, 10);
                };
            }

            PrintResult(firstResult);
            PrintResult(secondResult);
            PrintResult(thirdResult);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void ClearIndexes(DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                directory.Delete(true);
            }
            directory.Create();
            System.Threading.Thread.Sleep(1000);
        }

        private static void PrintResult(SearchResult result)
        {
            if (result != null)
            {
                Console.WriteLine($"query: {result.Expression}");
                foreach (var document in result.Documents)
                {
                    Console.WriteLine("".PadLeft(40, '-'));
                    foreach (var field in document.GetFields())
                    {
                        Console.WriteLine($"{field.Name}: {field.StringValue}");
                    }
                    Console.WriteLine("".PadLeft(40, '-'));
                }
            }
        }

        private static IEnumerable<IndexedDocument> FetchDocuments()
        {
            return new List<IndexedDocument>
            {
                new IndexedDocument { Id = Guid.NewGuid(), Categories = {
                        category1, category2
                    },
                    DisplayValue = "First Document" },

                new IndexedDocument { Id = Guid.NewGuid(), Categories = {
                        category1
                    }, DisplayValue = "Second Document" },

                new IndexedDocument { Id = Guid.NewGuid(), Categories = {
                        category2
                    }, DisplayValue = "Third Document" },

                new IndexedDocument { Id = Guid.NewGuid(), Categories = {
                        category2, category3
                    }, DisplayValue = "Forth Document" },
            };
        }
        
    }
}
