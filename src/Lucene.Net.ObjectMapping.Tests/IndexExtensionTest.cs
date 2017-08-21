using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Linq;
using Lucene.Net.Mapping;
using Lucene.Net.ObjectMapping.Tests.Model;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lucene.Net.ObjectMapping.Tests
{
    [TestFixture]
    public class IndexExtensionTest
    {
        private Directory dir;
        private IndexWriter writer;

        #region Add

        [Test]
        public void AddArgumentExceptions()
        {
            try
            {
                Index.ObjectMappingExtensions.Add(null, new object());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }

            try
            {
                writer.Add<object>(null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Add<object>(new object(), (MappingSettings)null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }

            try
            {
                writer.Add<object>(new object(), null, new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }
        }

        [Test]
        public void AddSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(0, writer.NumDocs);
            writer.Add(t);
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);


            var reader = DirectoryReader.Open(dir); //fixme: readonly flag ignored


            HashSet<string> expectedTerms = new HashSet<string>(new string[] { "test", "object", "1234" });

            var fields = MultiFields.GetFields(reader);
            foreach (var field in fields)
            {
                var tms = fields.GetTerms(field);
                TermsEnum termsEnum = tms.GetIterator(null);
                BytesRef text = termsEnum.Next();
                while (text != null)
                {
                    if (String.Equals("String", field))
                    {
                        Assert.True(expectedTerms.Contains(text.Utf8ToString()));
                        expectedTerms.Remove(text.Utf8ToString());
                    }
                    text = termsEnum.Next();
                }
            }

            Assert.AreEqual(0, expectedTerms.Count);
        }

        [Test]
        public void AddWithAnalyzerArgumentExceptions()
        {
            try
            {
                Index.ObjectMappingExtensions.Add(null, new object(), new StandardAnalyzer(Util.LuceneVersion.LUCENE_48));
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }

            try
            {
                writer.Add<object>(null, new StandardAnalyzer(Util.LuceneVersion.LUCENE_48));
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Add(new object(), (Analyzer)null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("analyzer", ex.ParamName);
            }
        }

        [Test]
        public void AddWithAnalyzerSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 9876,
                String = "Test Object 9876",
            };

            Assert.AreEqual(0, writer.NumDocs);
            writer.Add(t, new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            var reader = DirectoryReader.Open(dir); //fixme: readonly flag ignored
            var nTerms = 0;
            var fields = MultiFields.GetFields(reader);
            foreach (var field in fields)
            {
                var tms = fields.GetTerms(field);
                TermsEnum termsEnum = tms.GetIterator(null);
                BytesRef text = termsEnum.Next();
                while (text != null)
                {
                    if (String.Equals("String", field))
                    {
                        var str = text.Utf8ToString();
                        Assert.AreEqual("Test Object 9876", str );
                        nTerms++;
                    }
                    text = termsEnum.Next();
                }
            }

            Assert.AreEqual(1, nTerms);
        }

        #endregion

        #region Update with Expressions

        [Test]
        public void UpdateWithExpressionArgumentExceptions()
        {
            IndexWriter oldWriter = writer;

            try
            {
                writer = null;
                writer.Update(new SimpleObject(), MappingSettings.Default, o => o.Text == "Blah");
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }
            finally
            {
                writer = oldWriter;
            }

            try
            {
                writer.Update<SimpleObject>(null, MappingSettings.Default, o => o.Text == "Blah");
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Update(new SimpleObject(), null, o => o.Text == "Blah");
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }

            try
            {
                writer.Update(new SimpleObject(), MappingSettings.Default, (Expression<Func<SimpleObject, bool>>)null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("predicate", ex.ParamName);
            }
        }

        [Test]
        public void UpdateWithExpressionAndAnalyzerArgumentExceptions()
        {
            IndexWriter oldWriter = writer;

            try
            {
                writer = null;
                writer.Update(new SimpleObject(), MappingSettings.Default, o => o.Text == "Blah", new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }
            finally
            {
                writer = oldWriter;
            }

            try
            {
                writer.Update<SimpleObject>(null, MappingSettings.Default, o => o.Text == "Blah", new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Update(new SimpleObject(), null, o => o.Text == "Blah", new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }

            try
            {
                writer.Update(new SimpleObject(), MappingSettings.Default, (Expression<Func<SimpleObject, bool>>)null, new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("predicate", ex.ParamName);
            }

            try
            {
                writer.Update(new SimpleObject(), MappingSettings.Default, o => o.Text == "Blah", null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("analyzer", ex.ParamName);
            }
        }

        [Test]
        public void UpdateWithExpressionSuccess()
        {
            const int NumObjects = 10;
            WriteTestObjects(NumObjects, o => o.ToDocument());

            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(NumObjects, writer.NumDocs);
            writer.Add(t);
            writer.Commit();
            Assert.AreEqual(NumObjects + 1, writer.NumDocs);

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, o => o.String == "1234");
            writer.Commit();
            Assert.AreEqual(NumObjects + 1, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader);

                // Verify that the updated item can be found.
                TestObject t3 = searcher.AsQueryable<TestObject>().Single(o => o.Number == 2345);

                Assert.AreEqual(t2.Number, t3.Number);
                Assert.AreEqual(t2.String, t3.String);

                // Verify that the old item cannot be found anymore.
                TestObject t4 = searcher.AsQueryable<TestObject>().SingleOrDefault(o => o.Number == 1234);
                Assert.IsNull(t4);

                // Verify that all other items remain untouched.
                TestObject[] others = (from o in searcher.AsQueryable<TestObject>()
                                       where o.Number != 2345
                                       select o).ToArray();
                Assert.IsNotNull(others);
                Assert.AreEqual(NumObjects, others.Length);

                foreach (TestObject o in others)
                {
                    Assert.AreNotEqual(t2.Number, o.Number);
                    Assert.AreNotEqual(t2.String, o.String);
                }
            }
        }

        [Test]
        public void UpdateWithExpressionAndAnalyzerSuccess()
        {
            Analyzer analyzer = new KeywordAnalyzer();
            const int NumObjects = 10;
            WriteTestObjects(NumObjects, o => o.ToDocument(), analyzer);

            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(NumObjects, writer.NumDocs);
            writer.Add(t, analyzer);
            writer.Commit();
            Assert.AreEqual(NumObjects + 1, writer.NumDocs);

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, MappingSettings.Default, o => o.String == "Test Object 1234", analyzer);
            writer.Commit();
            Assert.AreEqual(NumObjects + 1, writer.NumDocs);

            using (var reader = DirectoryReader.Open(dir))
            {
                var searcher = new IndexSearcher(reader);

                // Verify that the updated item can be found.
                TestObject t3 = searcher.AsQueryable<TestObject>().Single(o => o.Number == 2345);

                Assert.AreEqual(t2.Number, t3.Number);
                Assert.AreEqual(t2.String, t3.String);

                // Verify that the old item cannot be found anymore.
                TestObject t4 = searcher.AsQueryable<TestObject>().SingleOrDefault(o => o.Number == 1234);
                Assert.IsNull(t4);

                // Verify that all other items remain untouched.
                TestObject[] others = (from o in searcher.AsQueryable<TestObject>()
                                       where o.Number != 2345
                                       select o).ToArray();
                Assert.IsNotNull(others);
                Assert.AreEqual(NumObjects, others.Length);

                foreach (TestObject o in others)
                {
                    Assert.AreNotEqual(t2.Number, o.Number);
                    Assert.AreNotEqual(t2.String, o.String);
                }
            }
        }

        #endregion

        #region Update

        [Test]
        public void UpdateArgumentExceptions()
        {
            try
            {
                Index.ObjectMappingExtensions.Update(null, new object(), new MatchAllDocsQuery());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), null, new MatchAllDocsQuery());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }

            try
            {
                writer.Update<object>(null, new MatchAllDocsQuery());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), (Query)null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("selection", ex.ParamName);
            }
        }

        [Test]
        public void UpdateSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(0, writer.NumDocs);
            writer.Add(t);
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, new TermQuery(new Term("String", "1234")));
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            var reader = DirectoryReader.Open(dir); //fixme: readonly flag ignored
            HashSet<string> expectedTerms = new HashSet<string>(new string[] { "something", "else", "2345" });

            var fields = MultiFields.GetFields(reader);
            foreach (var field in fields)
            {
                var tms = fields.GetTerms(field);
                TermsEnum termsEnum = tms.GetIterator(null);
                BytesRef text = termsEnum.Next();
                while (text != null)
                {
                    if (expectedTerms.Contains(text.Utf8ToString()))
                    {
                        expectedTerms.Remove(text.Utf8ToString());
                    }
                    text = termsEnum.Next();
                }
            }

            Assert.AreEqual(0, expectedTerms.Count);
        }

        [Test]
        public void UpdateWithKindArgumentExceptions()
        {
            try
            {
                Index.ObjectMappingExtensions.Update(null, new object(), DocumentObjectTypeKind.Static, new MatchAllDocsQuery());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }

            try
            {
                writer.Update<object>(null, DocumentObjectTypeKind.Static, new MatchAllDocsQuery());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), DocumentObjectTypeKind.Static, null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("selection", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), DocumentObjectTypeKind.Static, null, new MatchAllDocsQuery());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }
        }

        [Test]
        public void UpdateWithKindSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(0, writer.NumDocs);
            writer.Add<object>(t);
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, DocumentObjectTypeKind.Static, new TermQuery(new Term("String", "1234")));
            writer.Commit();
            Assert.AreEqual(2, writer.NumDocs);

            writer.DeleteDocuments<object>(new MatchAllDocsQuery());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            TestObject t3 = new TestObject()
            {
                Number = 3456,
                String = "Completely Different 3456",
            };

            writer.Update(t3, DocumentObjectTypeKind.Actual, new TermQuery(new Term("String", "2345")));
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            var reader = DirectoryReader.Open(dir); //fixme: readonly flag ignored
            HashSet<string> expectedTerms = new HashSet<string>(new string[] { "completely", "different", "3456" });

            var fields = MultiFields.GetFields(reader);
            foreach (var field in fields)
            {
                var tms = fields.GetTerms(field);
                TermsEnum termsEnum = tms.GetIterator(null);
                BytesRef text = termsEnum.Next();
                while (text != null)
                {
                    if (expectedTerms.Contains(text.Utf8ToString()))
                    {
                        expectedTerms.Remove(text.Utf8ToString());
                    }
                    text = termsEnum.Next();
                }
            }

            Assert.AreEqual(0, expectedTerms.Count);
        }

        [Test]
        public void UpdateWithAnalyzerArgumentExceptions()
        {
            try
            {
                Index.ObjectMappingExtensions.Update(null, new object(), new MatchAllDocsQuery(), new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }

            try
            {
                writer.Update<object>(null, new MatchAllDocsQuery(), new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), null, new MatchAllDocsQuery(), new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), null, new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("selection", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), new MatchAllDocsQuery(), null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("analyzer", ex.ParamName);
            }
        }

        [Test]
        public void UpdateWithAnalyzerSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(0, writer.NumDocs);
            writer.Add(t, new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, new TermQuery(new Term("String", "Test Object 1234")), new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            var reader = DirectoryReader.Open(dir); //fixme: readonly flag ignored
            var fields = MultiFields.GetFields(reader);
            var nTerms = 0;
            foreach (var field in fields)
            {
                var tms = fields.GetTerms(field);
                TermsEnum termsEnum = tms.GetIterator(null);
                BytesRef text = termsEnum.Next();
                while (text != null)
                {
                    if (String.Equals("String", field))
                    {
                        if (String.Equals("Something Else 2345", text.Utf8ToString()))
                        {
                            nTerms++;
                        }
                    }
                    text = termsEnum.Next();
                }
            }

            Assert.AreEqual(1, nTerms);
        }

        [Test]
        public void UpdateWithKindWithAnalyzerArgumentExceptions()
        {
            try
            {
                Index.ObjectMappingExtensions.Update(null, new object(), DocumentObjectTypeKind.Static, new MatchAllDocsQuery(), new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }

            try
            {
                writer.Update<object>(null, DocumentObjectTypeKind.Static, new MatchAllDocsQuery(), new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), DocumentObjectTypeKind.Static, null, new MatchAllDocsQuery(), new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), DocumentObjectTypeKind.Static, null, new KeywordAnalyzer());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("selection", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), DocumentObjectTypeKind.Static, new MatchAllDocsQuery(), null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("analyzer", ex.ParamName);
            }
        }

        [Test]
        public void UpdateWithKindWithAnalyzerSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(0, writer.NumDocs);
            writer.Add<object>(t, new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, DocumentObjectTypeKind.Static, new TermQuery(new Term("String", "Test Object 1234")), new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(2, writer.NumDocs);

            writer.DeleteDocuments<object>(new MatchAllDocsQuery());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            TestObject t3 = new TestObject()
            {
                Number = 3456,
                String = "Completely Different 3456",
            };

            writer.Update(t3, DocumentObjectTypeKind.Actual, new TermQuery(new Term("String", "Something Else 2345")), new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs);

            var reader = DirectoryReader.Open(dir); //fixme: readonly flag ignored
            var fields = MultiFields.GetFields(reader);
            var nTerms = 0;
            foreach (var field in fields)
            {
                var tms = fields.GetTerms(field);
                TermsEnum termsEnum = tms.GetIterator(null);
                BytesRef text = termsEnum.Next();
                while (text != null)
                {
                    if (String.Equals("String", field))
                    {
                        if (String.Equals("Completely Different 3456", text.Utf8ToString()))
                        {
                            nTerms++;
                        }
                    }
                    text = termsEnum.Next();
                }
            }

            Assert.AreEqual(1, nTerms);
        }

        #endregion

        #region Delete with Expressions

        [Test]
        public void DeleteArgumentExceptions()
        {
            IndexWriter oldWriter = writer;

            try
            {
                writer = null;
                writer.Delete<SimpleObject>(o => o.Text == "Blah");
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }
            finally
            {
                writer = oldWriter;
            }

            try
            {
                writer.Delete<SimpleObject>(null);
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("predicate", ex.ParamName);
            }
        }

        [Test]
        public void DeleteSuccess()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            writer.Delete<TestObject>(t => t.Number.InRange(null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        #endregion

        #region DeleteDocuments

        [Test]
        public void DeleteDocumentsFromIndex()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            writer.DeleteDocuments(
                typeof(TestObject),
                NumericRangeQuery.NewInt64Range("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>()); // Static Type: Object, Actual Type: TestObject
            Assert.AreEqual(NumObjects, writer.NumDocs);

            writer.DeleteDocuments(
                typeof(TestObject),
                DocumentObjectTypeKind.Static,
                NumericRangeQuery.NewInt64Range("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            // No object must have been deleted yet, because the static type is set to Object, not TestObject.
            VerifyTestObjects(NumObjects, 0);

            writer.DeleteDocuments(
                typeof(TestObject),
                DocumentObjectTypeKind.Actual,
                NumericRangeQuery.NewInt64Range("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexGeneric()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs);

            writer.DeleteDocuments<TestObject>(
                NumericRangeQuery.NewInt64Range("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexWithTypeKindCheckGeneric()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>()); // Static Type: Object, Actual Type: TestObject
            Assert.AreEqual(NumObjects, writer.NumDocs);

            writer.DeleteDocuments<TestObject>(
                DocumentObjectTypeKind.Static,
                NumericRangeQuery.NewInt64Range("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            // No object must have been deleted yet, because the static type is set to Object, not TestObject.
            VerifyTestObjects(NumObjects, 0);

            writer.DeleteDocuments<TestObject>(
                DocumentObjectTypeKind.Actual,
                NumericRangeQuery.NewInt64Range("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
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

        private void WriteTestObjects(int count, Func<TestObject, Document> converter, Analyzer analyzer = null)
        {
            for (int i = 0; i < count; i++)
            {
                TestObject obj = new TestObject()
                {
                    Number = i,
                    String = String.Format("Test Object {0}", i),
                };

                if (null != analyzer)
                {
                    writer.AddDocument(converter(obj), analyzer);
                }
                else
                {
                    writer.AddDocument(converter(obj));
                }
            }

            writer.Commit();
        }

        private void VerifyTestObjects(int count, int maxDeletedExclusive)
        {
            Assert.AreEqual(count - maxDeletedExclusive, writer.NumDocs);

            if (maxDeletedExclusive == 0 )

            using (IndexReader reader = DirectoryReader.Open(dir)) //fixme: readonly flag ignored
            {
                var liveDocs = MultiFields.GetLiveDocs(reader);
                if (liveDocs == null)
                {
                    //If no records are deleted, liveDocs will be null
                    Assert.AreEqual(0,maxDeletedExclusive);
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    if (i < maxDeletedExclusive)
                    {
                        Assert.True(!liveDocs.Get(i));
                    }
                    else
                    {
                        Assert.False(!liveDocs.Get(i));
                        Document doc = reader.Document(i);
                        TestObject obj = doc.ToObject<TestObject>();
                        Assert.AreEqual(i, obj.Number);
                        Assert.AreEqual(String.Format("Test Object {0}", i), obj.String);
                    }
                }
            }
        }
    }
}
