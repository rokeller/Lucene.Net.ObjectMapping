using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Linq;
using Lucene.Net.Mapping;
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
        public void Any()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>();
                IQueryable<NestedTestObjectA> queryA = searcher.AsQueryable<NestedTestObjectA>();

                Assert.IsTrue(query.Any());
                Assert.IsTrue(query.Where(t => t.String == "test").Any());
                Assert.IsFalse(query.Where(t => t.String != "test").Any());

                Assert.IsFalse(queryA.Any());
            }
        }

        [Test]
        public void AnyWithPredicate()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>();
                IQueryable<NestedTestObjectA> queryA = searcher.AsQueryable<NestedTestObjectA>();

                Assert.IsTrue(query.Any(t => t.String == "test"));
                Assert.IsFalse(query.Any(t => t.String != "test"));
                Assert.IsTrue(query.Any());

                Assert.IsFalse(queryA.Any());
            }
        }

        [Test]
        public void Count()
        {
            const int NumObjects = 10;
            const int MaxNumberExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                Assert.AreEqual(NumObjects, searcher.AsQueryable<TestObject>().Count());
                Assert.AreEqual(NumObjects, searcher.AsQueryable<NestedTestObjectC>().Count());
                Console.WriteLine("Query: {0}", searcher.AsQueryable<TestObject>());
                Console.WriteLine("Query: {0}", searcher.AsQueryable<NestedTestObjectC>());

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>()
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
        public void CountWithPredicate()
        {
            const int NumObjects = 10;
            const int MaxNumberExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            WriteC(NumObjects, obj => obj.ToDocument());

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>();
                IOrderedQueryable<TestObject> orderedQuery = query
                    .OrderByDescending(t => t.Number).ThenBy(t => t.Long);

                Assert.AreEqual(MaxNumberExclusive, query.Count(t => t.Number.InRange(null, MaxNumberExclusive, false, false)));
                Assert.AreEqual(MaxNumberExclusive, orderedQuery.Count(t => t.Number.InRange(MaxNumberExclusive, null, true, false)));

                // Now let's verify that the original query weren't modified, and still return the full number of results.
                Assert.AreEqual(NumObjects, query.Count());
                Assert.AreEqual(NumObjects, orderedQuery.Count());
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

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>()
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

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // None of the objects have the term 'Test', they only have 'test'.
                Assert.AreEqual(0, searcher.AsQueryable<TestObject>().Where(t => t.String.MatchesTerm("Test")).Count());

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>()
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
                query = from t in searcher.AsQueryable<TestObject>()
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

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>()
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

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>().OrderBy(t => t.Long).ThenByDescending(t => t.Number);
                Console.WriteLine("Query: {0}", query);

                Stopwatch watch = Stopwatch.StartNew();
                int count = query.Count();
                watch.Stop();

                Console.WriteLine("Determining the count of the query ({0}) took {1}ms.", query, watch.ElapsedMilliseconds);
                Assert.AreEqual(NumObjects, count);

                watch = Stopwatch.StartNew();
                TestObject[] results = query.ToArray();
                watch.Stop();

                Console.WriteLine("Converting the results of the query ({0}) to an array took {1}ms. That's {2}ms per item.",
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

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>()
                    .OrderBy(t => t.Long)
                    .ThenByDescending(t => t.Number)
                    .Take(PageSize);
                Assert.AreEqual(PageSize, query.Count());

                TestObject[] results = query.ToArray();
                Assert.AreEqual(PageSize, results.Length);

                long lastVal = results[PageSize - 1].Long;

                query = (from t in searcher.AsQueryable<TestObject>()
                         orderby t.Long
                         select t).Skip(PageSize);
                Assert.AreEqual(NumObjects - PageSize, query.Count());
                results = query.ToArray();
                Assert.AreEqual(NumObjects - PageSize, results.Length);
                Assert.LessOrEqual(lastVal, results[0].Long);
                Assert.LessOrEqual(results[0].Long, results[NumObjects - PageSize - 1].Long);

                query = (from t in searcher.AsQueryable<TestObject>()
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

            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<TestObject> query = searcher.AsQueryable<TestObject>()
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
            Assert.AreEqual(2 * NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // None of the objects have the term 'Number', they only have 'number'.
                Assert.AreEqual(0, searcher.AsQueryable<NestedTestObjectB>().Where(t => t.C.String.MatchesTerm("Number")).Count());

                IQueryable<NestedTestObjectB> query = searcher.AsQueryable<NestedTestObjectB>()
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
                query = from b in searcher.AsQueryable<NestedTestObjectB>()
                        where b.C.String.MatchesTerm("5")
                        select b;
                Console.WriteLine("Count: {0}, Query: {1}", query.Count(), query);
                results = query.ToArray();
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(5, results[0].Id);
                Assert.AreEqual("Number 5", results[0].C.String);

                // Look for NestedTestObjectB with 'random' in the String of the nested C object array. We should get
                // all the objects back, since all of them have at least one string with 'Random' in the array.
                query = from b in searcher.AsQueryable<NestedTestObjectB>()
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
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on float range.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
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
                query = from t in searcher.AsQueryable<TestObject>()
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
                query = from t in searcher.AsQueryable<TestObject>()
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

        [Test]
        public void BasicEquals()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on an exact number match.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Number == 6
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(6, results[0].Number);

                // Query on an exact float match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where 4.0404040404f == t.Float
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(4, results[0].Number);

                // Query on an exact double match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where (Math.PI * 8).Equals(t.Double)
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(8, results[0].Number);

                // Query on an exact decimal match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.Decimal.Equals(new Decimal(Math.E) * 3)
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(3, results[0].Number);

                // Query on string term match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.SecondString == "abcdef5"
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(5, results[0].Number);

                // Query on string term match - with Equals.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.String.Equals("test")
                        orderby t.Number
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(10, results.Length);

                for (int i = 0; i < results.Length; i++)
                {
                    Assert.AreEqual(i, results[i].Number);
                }
            }
        }

        [Test]
        public void BasicNotEquals()
        {
            const int NumObjects = 10;
            TestObject theOtherObject = new TestObject() { Number = 6 };

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on an exact number match.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Number != theOtherObject.Number
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(9, results.Length);
                foreach (TestObject result in results)
                {
                    Assert.AreNotEqual(6, result.Number);
                }

                // Query on an exact float match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where 4.0404040404f != t.Float
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(9, results.Length);
                foreach (TestObject result in results)
                {
                    Assert.AreNotEqual(4, result.Number);
                }

                // Query on an exact double match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where !(Math.PI * 8).Equals(t.Double)
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(9, results.Length);
                foreach (TestObject result in results)
                {
                    Assert.AreNotEqual(8, result.Number);
                }

                // Query on an exact decimal match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where !t.Decimal.Equals(new Decimal(Math.E) * 3)
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(9, results.Length);
                foreach (TestObject result in results)
                {
                    Assert.AreNotEqual(3, result.Number);
                }

                // Query on string term match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where !(t.String == "5")
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(9, results.Length);
                foreach (TestObject result in results)
                {
                    Assert.AreNotEqual(5, result.Number);
                }

                // Query on string term match - with Equals.
                query = from t in searcher.AsQueryable<TestObject>()
                        where !t.String.Equals("test")
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(0, results.Length);
            }
        }

        [Test]
        public void ComplexEqualsAndNotEquals()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on an exact number match.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.String == "object" && !(t.Number == 6)
                                               orderby GenericField.Timestamp
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(9, results.Length);
                int lastNum = -1;
                foreach (TestObject result in results)
                {
                    Assert.Greater(result.Number, lastNum);
                    lastNum = result.Number;
                    Assert.AreNotEqual(6, result.Number);
                }

                // Query on an exact number match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.String != "blah" && (t.Number == 6)
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(6, results[0].Number);
            }
        }

        [Test]
        public void ComplexEqualsOrNotEquals()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on an exact number match.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.String == "object" || !(t.Number == 6)
                                               orderby t.Number
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(NumObjects, results.Length);
                for (int i = 0; i < NumObjects; i++)
                {
                    Assert.AreEqual(i, results[i].Number);
                }

                // Query on an exact number match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.String == "blah" || (t.Number == 6)
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(6, results[0].Number);
            }
        }

        [Test]
        public void GuidEquals()
        {
            const int NumObjects = 10;
            Guid theGuid;

            theGuid = WriteTestObjectsWithGuid(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on an exact GUID match.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Guid == theGuid
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(theGuid, results[0].Guid);

                // Query on an exact GUID match.
                query = from t in searcher.AsQueryable<TestObject>()
                        where !t.Guid.Equals(theGuid)
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(NumObjects - 1, results.Length);
                foreach (TestObject result in results)
                {
                    Assert.AreNotEqual(theGuid, result.Guid);
                }
            }
        }

        [Test]
        public void EnumEquals()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on an exact GUID match.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Enum == MyEnum.First
                                               orderby t.Number
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(3, results.Length);
                Assert.AreEqual(0, results[0].Number);
                Assert.AreEqual(4, results[1].Number);
                Assert.AreEqual(8, results[2].Number);
            }
        }

        [Test]
        public void BoolEquals()
        {
            const int NumObjects = 10;
            bool myFlag;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Query on a boolean field, match true.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Boolean
                                               orderby t.Number descending
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(4, results.Length);
                Assert.AreEqual(9, results[0].Number);
                Assert.AreEqual(6, results[1].Number);
                Assert.AreEqual(3, results[2].Number);
                Assert.AreEqual(0, results[3].Number);

                // Query on a boolean field, match false.
                query = from t in searcher.AsQueryable<TestObject>()
                        where !t.Boolean
                        orderby t.Number
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                int pos = 0;
                Assert.AreEqual(6, results.Length);
                Assert.AreEqual(1, results[pos++].Number);
                Assert.AreEqual(2, results[pos++].Number);
                Assert.AreEqual(4, results[pos++].Number);
                Assert.AreEqual(5, results[pos++].Number);
                Assert.AreEqual(7, results[pos++].Number);
                Assert.AreEqual(8, results[pos++].Number);

                // Query on a boolean field, match a variable.
                myFlag = true;
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.Boolean == myFlag
                        orderby t.Number descending
                        select t;
                results = query.ToArray();
                Assert.NotNull(results);
                Assert.AreEqual(4, results.Length);
                Assert.AreEqual(9, results[0].Number);
                Assert.AreEqual(6, results[1].Number);
                Assert.AreEqual(3, results[2].Number);
                Assert.AreEqual(0, results[3].Number);
            }
        }

        [Test]
        public void ComparisionOfTwoMembers()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);
                // Try query comparing two members, which is not supported.
                Assert.Throws<NotSupportedException>(() => 
                    (from t in searcher.AsQueryable<TestObject>()
                     where (int)t.Enum == t.Number
                     orderby t.Number
                     select t).ToArray());
            }
        }

        [Test]
        public void ComparisionOfTwoConstants()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try query using a constant expression which evaluates to true, thus matching all.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where 1 != 2
                                               orderby t.Number
                                               select t;
                TestObject[] results = query.ToArray();
                Assert.AreEqual(NumObjects, results.Length);

                // Try query using a constant expression which evaluates to false, thus matching none.
                query = from t in searcher.AsQueryable<TestObject>()
                        where 1 == 2
                        orderby t.Number
                        select t;
                results = query.ToArray();
                Assert.AreEqual(0, results.Length);
            }
        }

        [Test]
        public void MemberMethodCall()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try query using a constant expression which evaluates to true, thus matching all.
                Assert.Throws<NotSupportedException>(() => 
                    (from t in searcher.AsQueryable<TestObject>()
                     where t.Number.ToString() == "2"
                     orderby t.Number
                     select t).ToArray());
            }
        }

        [Test]
        public void MemberArithmetic01()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try query using a constant expression which evaluates to true, thus matching all.
                Assert.Throws<NotSupportedException>(() =>
                    (from t in searcher.AsQueryable<TestObject>()
                     where t.Number % 2 == 0
                     orderby t.Number
                     select t).ToArray());
            }
        }

        [Test]
        public void MemberArithmetic02()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try query using a constant expression which evaluates to true, thus matching all.
                Assert.Throws<NotSupportedException>(() =>
                    (from t in searcher.AsQueryable<TestObject>()
                     where t.Number + 2 == 4
                     orderby t.Number
                     select t).ToArray());
            }
        }

        [Test]
        public void MemberArithmetic03()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try query using a constant expression which evaluates to true, thus matching all.
                Assert.Throws<NotSupportedException>(() =>
                    (from t in searcher.AsQueryable<TestObject>()
                     where t.Number + t.Long == 4
                     orderby t.Number
                     select t).ToArray());
            }
        }

        #region First and FirstOrDefault

        [Test]
        public void First()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try First() for a single existing item.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Number == 9
                                               select t;

                TestObject result = query.First();
                Assert.IsNotNull(result);
                Assert.AreEqual(9, result.Number);

                // Try First() for a non-existing item.
                try
                {
                    query = from t in searcher.AsQueryable<TestObject>()
                            where t.Number == 9999
                            select t;

                    result = query.First();
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has no results.", ex.Message);
                }

                // Try First() for multiple existing items.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.Number.InRange(3, null, false, true)
                        select t;

                result = query.First();
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.Number);
            }
        }

        [Test]
        public void FirstWithPredicate()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try First() for a single existing item.
                TestObject result = searcher.AsQueryable<TestObject>().First(t => t.Number == 0);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Number);

                // Try First() for a non-existing item.
                try
                {
                    result = searcher.AsQueryable<TestObject>().First(t => t.Number == 9999);
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has no results.", ex.Message);
                }

                // Try First() for multiple existing items.
                result = searcher.AsQueryable<TestObject>().First(t => t.String == "foo" || t.Number.InRange(5, 8, true, true));
                Assert.IsNotNull(result);
                Assert.GreaterOrEqual(result.Number, 5);
                Assert.LessOrEqual(result.Number, 8);
            }
        }

        [Test]
        public void FirstOrDefault()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try FirstOrDefault() for a single existing item.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Number == 8
                                               select t;

                TestObject result = query.FirstOrDefault();
                Assert.IsNotNull(result);
                Assert.AreEqual(8, result.Number);

                // Try FirstOrDefault() for a non-existing item.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.Number == 9999
                        select t;

                result = query.FirstOrDefault();
                Assert.IsNull(result);

                // Try FirstOrDefault() for multiple existing items.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.Number.InRange(null, 7, false, true)
                        orderby t.Number descending
                        select t;

                result = query.FirstOrDefault();
                Assert.IsNotNull(result);
                Assert.AreEqual(7, result.Number);
            }
        }

        [Test]
        public void FirstOrDefaultWithPredicate()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try FirstOrDefault() for a single existing item.
                TestObject result = searcher.AsQueryable<TestObject>().FirstOrDefault(t => t.Number == 0);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Number);

                // Try FirstOrDefault() for a non-existing item.
                result = searcher.AsQueryable<TestObject>().FirstOrDefault(t => t.Number == 9999);
                Assert.IsNull(result);

                // Try FirstOrDefault() for multiple existing items.
                result = searcher.AsQueryable<TestObject>().FirstOrDefault(t => t.String == "test" && t.Number.InRange(2, null, true, false));
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Number);
            }
        }

        #endregion

        #region Single and SingleOrDefault

        [Test]
        public void Single()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try Single() for a single existing item.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Number == 9
                                               select t;

                TestObject result = query.Single();
                Assert.IsNotNull(result);
                Assert.AreEqual(9, result.Number);

                // Try Single() for a non-existing item.
                try
                {
                    query = from t in searcher.AsQueryable<TestObject>()
                            where t.Number == 9999
                            select t;

                    result = query.Single();
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has no results.", ex.Message);
                }

                // Try Single() for multiple existing items.
                try
                {
                    query = from t in searcher.AsQueryable<TestObject>()
                            where t.Boolean
                            select t;

                    result = query.Single();
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has more than one result.", ex.Message);
                }
            }
        }

        [Test]
        public void SingleWithPredicate()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try Single() for a single existing item.
                TestObject result = searcher.AsQueryable<TestObject>().Single(t => t.Number == 0);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Number);

                // Try Single() for a non-existing item.
                try
                {
                    result = searcher.AsQueryable<TestObject>().Single(t => t.Number == 9999);
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has no results.", ex.Message);
                }

                // Try Single() for multiple existing items.
                try
                {
                    result = searcher.AsQueryable<TestObject>().Single(t => !t.Boolean);
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has more than one result.", ex.Message);
                }
            }
        }

        [Test]
        public void SingleOrDefault()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try SingleOrDefault() for a single existing item.
                IQueryable<TestObject> query = from t in searcher.AsQueryable<TestObject>()
                                               where t.Number == 9
                                               select t;

                TestObject result = query.SingleOrDefault();
                Assert.IsNotNull(result);
                Assert.AreEqual(9, result.Number);

                // Try SingleOrDefault() for a non-existing item.
                query = from t in searcher.AsQueryable<TestObject>()
                        where t.Number == 9999
                        select t;

                result = query.SingleOrDefault();
                Assert.IsNull(result);

                // Try SingleOrDefault() for multiple existing items.
                try
                {
                    query = from t in searcher.AsQueryable<TestObject>()
                            where t.Boolean
                            select t;

                    result = query.SingleOrDefault();
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has more than one result.", ex.Message);
                }
            }
        }

        [Test]
        public void SingleOrDefaultWithPredicate()
        {
            const int NumObjects = 10;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                // Try SingleOrDefault() for a single existing item.
                TestObject result = searcher.AsQueryable<TestObject>().SingleOrDefault(t => t.Number == 0);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Number);

                // Try SingleOrDefault() for a non-existing item.
                result = searcher.AsQueryable<TestObject>().SingleOrDefault(t => t.Number == 9999);
                Assert.IsNull(result);

                // Try SingleOrDefault() for multiple existing items.
                try
                {
                    result = searcher.AsQueryable<TestObject>().SingleOrDefault(t => !t.Boolean);
                    Assert.Fail("Must get an exception.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("The query has more than one result.", ex.Message);
                }
            }
        }

        #endregion

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
            Random rng = new Random();
            Decimal e = new Decimal(Math.E);

            for (int i = 0; i < count; i++)
            {
                TestObject obj = new TestObject()
                {
                    Number = i,
                    String = String.Format("Test Object {0}", i),
                    SecondString = String.Format("abcdef{0}", i),
                    Long = rng.Next(1000),
                    Float = 1.0101010101f * i,
                    Double = Math.PI * i,
                    Decimal = e * i,
                    Enum = (MyEnum)(i % 4),
                    Boolean = (i % 3 == 0),
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
    }
}
