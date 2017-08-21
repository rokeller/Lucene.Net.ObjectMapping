using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.ObjectMapping.Tests.Model;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using System;

namespace Lucene.Net.ObjectMapping.Tests
{
    [TestFixture]
    public class SearchExtensionTest
    {
        private Directory dir;
        private IndexWriter writer;

        [Test]
        public void Search()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 0;
            const int MaxNumberExclusive = 8;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search(
                        typeof(TestObject),
                        NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                        NumObjects);

                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive);                
            }
        }

        [Test]
        public void SearchWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 2;
            const int MaxNumberExclusive = 7;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search(
                    typeof(TestObject),
                    DocumentObjectTypeKind.Static,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects);

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, 0, 0);

                topDocs = searcher.Search(
                    typeof(TestObject),
                    DocumentObjectTypeKind.Actual,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects);

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive);
            }
        }

        [Test]
        public void SearchGeneric()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 3;
            const int MaxNumberExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search<TestObject>(
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects);

                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive);
            }
        }

        [Test]
        public void SearchGenericWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 1;
            const int MaxNumberExclusive = 9;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search<TestObject>(
                    DocumentObjectTypeKind.Static,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects);

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, 0, 0);

                topDocs = searcher.Search<TestObject>(
                    DocumentObjectTypeKind.Actual,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects);

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive);
            }
        }

        [Test]
        public void SearchWithSort()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 0;
            const int MaxNumberExclusive = 8;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search(
                    typeof(TestObject),
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects,
                    new Sort(new SortField("Number", SortFieldType.INT64, true)));

                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [Test]
        public void SearchWithSortWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 2;
            const int MaxNumberExclusive = 7;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search(
                    typeof(TestObject),
                    DocumentObjectTypeKind.Static,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects,
                    new Sort(new SortField("Number", SortFieldType.INT64, true)));

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, 0, 0, true);

                topDocs = searcher.Search(
                    typeof(TestObject),
                    DocumentObjectTypeKind.Actual,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects,
                    new Sort(new SortField("Number", SortFieldType.INT64, true)));

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [Test]
        public void SearchGenericWithSort()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 3;
            const int MaxNumberExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search<TestObject>(
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects,
                    new Sort(new SortField("Number", SortFieldType.INT64, true)));

                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [Test]
        public void SearchGenericWithSortWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 1;
            const int MaxNumberExclusive = 9;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopDocs topDocs = searcher.Search<TestObject>(
                    DocumentObjectTypeKind.Static,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects,
                    new Sort(new SortField("Number", SortFieldType.INT64, true)));

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, 0, 0, true);

                topDocs = searcher.Search<TestObject>(
                    DocumentObjectTypeKind.Actual,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    NumObjects,
                    new Sort(new SortField("Number", SortFieldType.INT64, true)));

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [Test]
        public void SearchWithCollector()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 0;
            const int MaxNumberExclusive = 8;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopFieldCollector collector = TopFieldCollector.Create(
                    new Sort(new SortField("Number", SortFieldType.INT64, true)),
                    NumObjects,
                    false,
                    false,
                    false,
                    false);
                searcher.Search(
                    typeof(TestObject),
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    collector);

                TopDocs topDocs = collector.GetTopDocs();

                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [Test]
        public void SearchWithCollectorWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 2;
            const int MaxNumberExclusive = 7;

            TopFieldCollector collector;
            TopDocs topDocs;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                collector = TopFieldCollector.Create(
                    new Sort(new SortField("Number", SortFieldType.INT64, true)),
                    NumObjects,
                    false,
                    false,
                    false,
                    false);
                searcher.Search(
                    typeof(TestObject),
                    DocumentObjectTypeKind.Static,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    collector);

                topDocs = collector.GetTopDocs();

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, 0, 0, true);

                collector = TopFieldCollector.Create(
                    new Sort(new SortField("Number", SortFieldType.INT64, true)),
                    NumObjects,
                    false,
                    false,
                    false,
                    false);
                searcher.Search(
                    typeof(TestObject),
                    DocumentObjectTypeKind.Actual,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    collector);

                topDocs = collector.GetTopDocs();

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [Test]
        public void SearchGenericWithCollector()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 0;
            const int MaxNumberExclusive = 8;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                TopFieldCollector collector = TopFieldCollector.Create(
                    new Sort(new SortField("Number", SortFieldType.INT64, true)),
                    NumObjects,
                    false,
                    false,
                    false,
                    false);
                searcher.Search<TestObject>(
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    collector);

                TopDocs topDocs = collector.GetTopDocs();

                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [Test]
        public void SearchGenericWithCollectorWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 2;
            const int MaxNumberExclusive = 7;

            TopFieldCollector collector;
            TopDocs topDocs;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader); //fixme: readonly flag ignored
                collector = TopFieldCollector.Create(
                    new Sort(new SortField("Number", SortFieldType.INT64, true)),
                    NumObjects,
                    false,
                    false,
                    false,
                    false);
                searcher.Search<TestObject>(
                    DocumentObjectTypeKind.Static,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    collector);

                topDocs = collector.GetTopDocs();

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, 0, 0, true);

                collector = TopFieldCollector.Create(
                    new Sort(new SortField("Number", SortFieldType.INT64, true)),
                    NumObjects,
                    false,
                    false,
                    false,
                    false);
                searcher.Search<TestObject>(
                    DocumentObjectTypeKind.Actual,
                    NumericRangeQuery.NewInt64Range("Number", MinNumberInclusive, MaxNumberExclusive, true, false),
                    collector);

                topDocs = collector.GetTopDocs();

                // The static type of none of the documents matches 'TestObject'.
                VerifyTopDocsTestObjects(searcher, topDocs, MinNumberInclusive, MaxNumberExclusive, true);
            }
        }

        [SetUp]
        public void SetUp()
        {
            dir = new RAMDirectory();
            writer = new IndexWriter(dir, new IndexWriterConfig(Util.LuceneVersion.LUCENE_48, new StandardAnalyzer(Util.LuceneVersion.LUCENE_48)));
        }

        [TearDown]
        public void TearDown()
        {
            writer.Dispose();
            dir.Dispose();
        }

        private void WriteTestObjects(int count, Func<TestObject, Document> converter)
        {
            for (int i = 0; i < count; i++)
            {
                TestObject obj = new TestObject()
                {
                    Number = i,
                    String = String.Format("Test Object {0}", i),
                };

                writer.AddDocument(converter(obj));
            }

            writer.Commit();
        }

        private void VerifyTopDocsTestObjects(IndexSearcher searcher, TopDocs topDocs, int startNumberInclusive, int endNumberExclusive)
        {
            VerifyTopDocsTestObjects(searcher, topDocs, startNumberInclusive, endNumberExclusive, false);
        }

        private void VerifyTopDocsTestObjects(IndexSearcher searcher, TopDocs topDocs, int startNumberInclusive, int endNumberExclusive, bool descending)
        {
            Assert.AreEqual(endNumberExclusive - startNumberInclusive, topDocs.TotalHits);

            int number;

            if (descending)
            {
                number = endNumberExclusive - 1;
            }
            else
            {
                number = startNumberInclusive;
            }

            foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
            {
                Document doc = searcher.Doc(scoreDoc.Doc);
                TestObject obj = doc.ToObject<TestObject>();
                Assert.AreEqual(number, obj.Number);
                Assert.AreEqual(String.Format("Test Object {0}", number), obj.String);

                if (descending)
                {
                    number--;
                }
                else
                {
                    number++;
                }
            }
        }
    }
}
