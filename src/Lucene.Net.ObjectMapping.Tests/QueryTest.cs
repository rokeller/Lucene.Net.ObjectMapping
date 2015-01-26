using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Linq;
using Lucene.Net.ObjectMapping.Tests.Model;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;

namespace Lucene.Net.ObjectMapping.Tests
{
    [TestFixture]
    public class QueryTest
    {
        private Directory dir;
        private IndexWriter writer;

        [Test]
        public void QueryWithCount()
        {
            const int NumObjects = 10;
            const int MaxNumberExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                Assert.AreEqual(NumObjects, searcher.Query<TestObject>().Count());
                Assert.AreEqual(NumObjects, searcher.Query<NestedTestObjectC>().Count());
                Console.WriteLine("Query: {0}", searcher.Query<TestObject>());
                Console.WriteLine("Query: {0}", searcher.Query<NestedTestObjectC>());

                IQueryable<TestObject> query = searcher.Query<TestObject>()
                    .Where(t => t.Number.InRange(null, MaxNumberExclusive, false, false));
                Console.WriteLine("Query: {0}", query);

                IOrderedQueryable<TestObject> orderedQuery = query
                    .OrderByDescending(t => t.Number).ThenBy(t => t.Long);
                Console.WriteLine("OrderedQuery: {0}", orderedQuery);

                Assert.AreEqual(MaxNumberExclusive, query.Count());
                Assert.AreEqual(MaxNumberExclusive, orderedQuery.Count());
            }
        }

        [Test]
        public void QueryWithNumberRange()
        {
            const int NumObjects = 10;
            const int MinNumberInclusive = 2;
            const int MaxNumberExclusive = 9;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                IQueryable<TestObject> query = searcher.Query<TestObject>()
                    .Where(t => t.Number.InRange(MinNumberInclusive, MaxNumberExclusive, true, false))
                    .OrderByDescending(t => t.Number);
                Console.WriteLine("Query: {0}", query);

                TestObject[] results = query.ToArray();
                Assert.AreEqual(MaxNumberExclusive - MinNumberInclusive, results.Length);

                for (int i = 0; i < MaxNumberExclusive - MinNumberInclusive; i++)
                {
                    Assert.AreEqual(MaxNumberExclusive - 1 - i, results[i].Number);
                }
            }
        }

        [Test]
        public void QueryWithMatchTerm()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                // None of the objects have the term 'Test', they only have 'test'.
                Assert.AreEqual(0, searcher.Query<TestObject>().Where(t => t.String.MatchesTerm("Test")).Count());

                IQueryable<TestObject> query = searcher.Query<TestObject>()
                    .Where(t => t.String.MatchesTerm("test"))
                    .OrderBy(t => t.Number);
                Console.WriteLine("Query: {0}", query);

                TestObject[] results = query.ToArray();
                Assert.AreEqual(NumObjects, results.Length);

                for (int i = 0; i < NumObjects; i++)
                {
                    Assert.AreEqual(i, results[i].Number);
                }

                // Look for TestObject with '7' in the String.
                query = from t in searcher.Query<TestObject>()
                        where t.String.MatchesTerm("7")
                        select t;
                Console.WriteLine("Query: {0}", query);
                results = query.ToArray();
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(7, results[0].Number);
                Assert.AreEqual("Test Object 7", results[0].String);
            }
        }

        [Test]
        public void QueryWithGuidMatch()
        {
            const int NumObjects = 100;
            Guid guid = WriteTestObjectsWithGuid(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                IQueryable<TestObject> query = searcher.Query<TestObject>()
                    .Where(t => t.Guid.MatchesTerm(guid.ToString()));
                Console.WriteLine("Query: {0}", query);

                TestObject[] results = query.ToArray();
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(guid, results[0].Guid);
            }
        }

        [Test]
        public void QueryWithMultipleSort()
        {
            const int NumObjects = 1000;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                IQueryable<TestObject> query = searcher.Query<TestObject>().OrderBy(t => t.Long).ThenByDescending(t => t.Number);
                Console.WriteLine("Query: {0}", query);

                Stopwatch watch = Stopwatch.StartNew();
                int count = query.Count();
                watch.Stop();

                Console.WriteLine("Determining the count of the query ({0}) took {1}ms.", query, watch.ElapsedMilliseconds);
                Assert.AreEqual(NumObjects, count);

                watch = Stopwatch.StartNew();
                TestObject[] results = query.ToArray();
                watch.Stop();

                Console.WriteLine("Determining the count of the query ({0}) took {1}ms. That's {2}ms per item.",
                    query, watch.ElapsedMilliseconds, 1f * watch.ElapsedMilliseconds / results.Length);
                Assert.AreEqual(NumObjects, results.Length);
                long lastVal = results[0].Long;

                for (int i = 1; i < NumObjects; i++)
                {
                    Assert.LessOrEqual(lastVal, results[i].Long);
                    lastVal = results[i].Long;
                }
            }
        }

        [Test]
        public void QueryWithPaging()
        {
            const int NumObjects = 100;
            const int PageSize = 40;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                IQueryable<TestObject> query = searcher.Query<TestObject>()
                    .OrderBy(t => t.Long)
                    .ThenByDescending(t => t.Number)
                    .Take(PageSize);
                Assert.AreEqual(PageSize, query.Count());

                TestObject[] results = query.ToArray();
                Assert.AreEqual(PageSize, results.Length);

                long lastVal = results[PageSize - 1].Long;

                query = (from t in searcher.Query<TestObject>()
                         orderby t.Long
                         select t).Skip(PageSize);
                Assert.AreEqual(NumObjects - PageSize, query.Count());
                results = query.ToArray();
                Assert.AreEqual(NumObjects - PageSize, results.Length);
                Assert.LessOrEqual(lastVal, results[0].Long);
                Assert.LessOrEqual(results[0].Long, results[NumObjects - PageSize - 1].Long);

                query = (from t in searcher.Query<TestObject>()
                         orderby t.Long
                         select t).Take(PageSize).Skip(2 * PageSize);
                Assert.AreEqual(NumObjects - 2 * PageSize, query.Count());
                results = query.ToArray();
                Assert.AreEqual(NumObjects - 2 * PageSize, results.Length);
            }
        }

        [Test]
        public void QueryWithDescendingSort()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                IQueryable<TestObject> query = searcher.Query<TestObject>()
                    .Where(t => t.Number.InRange(null, 5, false, false))
                    .OrderByDescending(t => t.Number);
                Console.WriteLine("Query: {0}", query);

                TestObject[] results = query.ToArray();
                Assert.AreEqual(5, results.Length);

                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(4 - i, results[i].Number);
                }
            }
        }

        [Test]
        public void QueryWithNestedObjects()
        {
            const int NumObjects = 10;

            WriteB(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(2 * NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                // None of the objects have the term 'Number', they only have 'number'.
                Assert.AreEqual(0, searcher.Query<NestedTestObjectB>().Where(t => t.C.String.MatchesTerm("Number")).Count());

                IQueryable<NestedTestObjectB> query = searcher.Query<NestedTestObjectB>()
                    .Where(t => t.C.String.MatchesTerm("number"))
                    .OrderByDescending(t => t.Id);
                Console.WriteLine("Count: {0}, Query: {1}", query.Count(), query);

                NestedTestObjectB[] results = query.ToArray();
                Assert.AreEqual(NumObjects, results.Length);

                for (int i = 0; i < NumObjects; i++)
                {
                    Assert.AreEqual(NumObjects - 1 - i, results[i].Id);
                }

                // Look for NestedTestObjectB with '5' in the String of the nested C object.
                query = from b in searcher.Query<NestedTestObjectB>()
                        where b.C.String.MatchesTerm("5")
                        select b;
                Console.WriteLine("Count: {0}, Query: {1}", query.Count(), query);
                results = query.ToArray();
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(5, results[0].Id);
                Assert.AreEqual("Number 5", results[0].C.String);

                // Look for NestedTestObjectB with 'random' in the String of the nested C object array. We should get
                // all the objects back, since all of them have at least one string with 'Random' in the array.
                query = from b in searcher.Query<NestedTestObjectB>()
                        where b.C.Array.MatchesTerm("random")
                        orderby b.C.String
                        select b;
                Console.WriteLine("Count: {0}, Query: {1}", query.Count(), query);
                results = query.ToArray();
                Assert.AreEqual(NumObjects, results.Length);
            }
        }

        [Test]
        public void NumericRangeQueries()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs());

            using (Searcher searcher = new IndexSearcher(dir, true))
            {
                // Query on float range.
                IQueryable<TestObject> query = from t in searcher.Query<TestObject>()
                                               where t.Float.InRange(5, 8, false, false) // Include 5-7
                                               orderby t.Number descending
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(3, results.Length);

                for (int i = 0; i < results.Length; i++)
                {
                    Assert.AreEqual(7 - i, results[i].Number);
                }

                // Query on double range.
                query = from t in searcher.Query<TestObject>()
                        where t.Double.InRange(21.99, 28.28, false, true) // Include 7-9
                        orderby t.Float
                        select t;
                results = query.ToArray();
                Assert.IsNotNull(results);
                Assert.AreEqual(3, results.Length);

                for (int i = 0; i < results.Length; i++)
                {
                    Assert.AreEqual(7 + i, results[i].Number);
                }

                // Query on decimal range.
                query = from t in searcher.Query<TestObject>()
                        where t.Decimal.InRange(8, 16.31m, false, false) // Include 3-6
                        orderby t.Decimal
                        select t;
                results = query.ToArray();
                Assert.IsNotNull(results);
                Assert.AreEqual(4, results.Length);

                for (int i = 0; i < results.Length; i++)
                {
                    Assert.AreEqual(3 + i, results[i].Number);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            dir = new RAMDirectory();
            writer = new IndexWriter(dir, new StandardAnalyzer(Util.Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED);
        }

        [TearDown]
        public void TearDown()
        {
            writer.Dispose();
            dir.Dispose();
        }

        private void WriteTestObjects(int count, Func<TestObject, Document> converter)
        {
            Random rng = new Random();
            Decimal e = new Decimal(Math.E);

            for (int i = 0; i < count; i++)
            {
                TestObject obj = new TestObject()
                {
                    Number = i,
                    String = String.Format("Test Object {0}", i),
                    Long = rng.Next(1000),
                    Float = 1.0101010101f * i,
                    Double = Math.PI * i,
                    Decimal = e * i,
                };

                writer.AddDocument(converter(obj));
            }

            writer.Commit();
        }

        private Guid WriteTestObjectsWithGuid(int count, Func<TestObject, Document> converter)
        {
            Random rng = new Random();
            Guid result = Guid.Empty;
            int indexOfResult = rng.Next(count);

            for (int i = 0; i < count; i++)
            {
                TestObject obj = new TestObject()
                {
                    Number = i,
                    String = String.Format("Test Object {0}", i),
                    Long = rng.Next(1000),
                    Guid = Guid.NewGuid(),
                };

                if (i == indexOfResult)
                {
                    result = obj.Guid;
                }

                writer.AddDocument(converter(obj));
            }

            writer.Commit();

            return result;
        }

        private void WriteB(int count, Func<NestedTestObjectB, Document> converter)
        {
            Random rng = new Random();

            for (int i = 0; i < count; i++)
            {
                NestedTestObjectB obj = new NestedTestObjectB()
                {
                    Id = i,
                    C = new NestedTestObjectC()
                    {
                        String = String.Format("Number {0}", i),
                        Array = MakeArray(rng.Next(1, 10), rng),
                    },
                };

                writer.AddDocument(converter(obj));
            }

            writer.Commit();
        }

        private void WriteC(int count, Func<NestedTestObjectC, Document> converter)
        {
            for (int i = 0; i < count; i++)
            {
                NestedTestObjectC obj = new NestedTestObjectC()
                {
                    String = String.Format("Number{0}", i),
                };

                writer.AddDocument(converter(obj));
            }

            writer.Commit();
        }

        private static string[] MakeArray(int size, Random rng)
        {
            string[] result = new string[size];

            for (int i = 0; i < size; i++)
            {
                result[i] = String.Format("Random {0}", rng.Next());
            }

            return result;
        }

        private void VerifyTopDocsTestObjects(Searcher searcher, TopDocs topDocs, int startNumberInclusive, int endNumberExclusive)
        {
            VerifyTopDocsTestObjects(searcher, topDocs, startNumberInclusive, endNumberExclusive, false);
        }

        private void VerifyTopDocsTestObjects(Searcher searcher, TopDocs topDocs, int startNumberInclusive, int endNumberExclusive, bool descending)
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
