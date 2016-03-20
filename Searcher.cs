using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using System;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace LuceneIndexer
{
    public class Searcher : IDisposable
    {
        protected readonly Directory LuceneDirectory;
        protected readonly Analyzer LuceneAnalyzer;

        private IndexWriter _Writer;
        private IndexWriter Writer {
            get {
                return this._Writer = this._Writer ?? this.GetIndexWriter();
            }
        }

        private IndexReader Reader
        {
            get
            {
                return this.Writer.GetReader();
            }
        }

        public Searcher(Directory directory, Analyzer analyzer)
        {
            this.LuceneDirectory = directory;
            this.LuceneAnalyzer = analyzer;
        }

        public void AddDocument(IndexedDocument document)
        {
            this.Writer.AddDocument(BuildDocument(document));
            this.Writer.Commit();
        }

        public SearchResult Search(string keywords, string[] fields, int maxHits)
        {
            QueryParser queryParser = null;

            if (fields.Length > 1)
            {
                queryParser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields, this.LuceneAnalyzer);
            }
            else
            {
                queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, fields[0], this.LuceneAnalyzer);
            }

            // queryParser.AllowLeadingWildcard = true;
            // queryParser.DefaultOperator = QueryParser.Operator.OR;
            queryParser.LowercaseExpandedTerms = true;

            var query = queryParser.Parse(keywords);

            return ExecuteQuery(query, maxHits);
        }

        public SearchResult Search(string keywords, string[] fields, string categoryField, Guid[] categories, int maxHits)
        {
            QueryParser queryParser = null;

            if (fields.Length > 1)
            {
                queryParser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields, this.LuceneAnalyzer);
            }
            else
            {
                queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, fields[0], this.LuceneAnalyzer);
            }

            queryParser.LowercaseExpandedTerms = true;

            var finalQuery = new BooleanQuery();
            var termsQuery = new BooleanQuery();
            var categoriesQuery = new BooleanQuery();

            var categoriesParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, categoryField, this.LuceneAnalyzer);

            foreach (var category in categories)
            {
                var clause = new BooleanClause(categoriesParser.Parse(category.ToString()), Occur.SHOULD);
                categoriesQuery.Add(clause);
            }

            foreach (var word in keywords.Split(' '))
            {
                termsQuery.Add(queryParser.Parse(word), Occur.SHOULD);
            }

            finalQuery.Add(termsQuery, Occur.MUST);
            finalQuery.Add(categoriesQuery, Occur.MUST);
           
            return ExecuteQuery(finalQuery, maxHits);
        }

        private SearchResult ExecuteQuery(Query query, int maxHits)
        {
            int totalDocuments = 0;

            var result = new SearchResult();

            result.Expression = query.ToString();

            using (var searcher = new IndexSearcher(this.Reader))
            {
                var documents = searcher.Search(query, maxHits);
                totalDocuments = documents.TotalHits;

                if (documents.ScoreDocs != null)
                {
                    foreach (var document in documents.ScoreDocs)
                    {
                        var hit = searcher.Doc(document.Doc);
                        result.Documents.Add(hit);
                    }
                }

                result.TotalDocumentsMatching = totalDocuments;
            }

            return result;
        }

        private IndexWriter GetIndexWriter()
        {
            if (IndexWriter.IsLocked(this.LuceneDirectory))
            {
                IndexWriter.Unlock(this.LuceneDirectory);
            }
            var writer = new IndexWriter(this.LuceneDirectory, this.LuceneAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            writer.SetMergePolicy(new LogDocMergePolicy(writer));
            return writer;
        }

        private IndexReader GetIndexReader()
        {
            return this.Writer.GetReader();
        }

        private Document BuildDocument(IndexedDocument document)
        {
            var luceneDocument = new Document();
            luceneDocument.Add(new Field("id", document.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            luceneDocument.Add(new Field("displayvalue", document.DisplayValue, Field.Store.YES, Field.Index.ANALYZED));
            foreach (var category in document.Categories)
            {
                luceneDocument.Add(new Field("categoryid", category.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            }

            return luceneDocument;
        }

        public void Dispose()
        {
            if (this._Writer != null)
            {
                this._Writer.Optimize(true);
                this._Writer.Commit();
                this._Writer.Dispose(true);
            }
        }
    }
}
