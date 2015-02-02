using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.ObjectMapping.Tests.Model;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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
        }

        [Test]
        public void AddSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(0, writer.NumDocs());
            writer.Add(t);
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            IndexReader reader = IndexReader.Open(dir, true);
            TermEnum terms = reader.Terms();
            HashSet<string> expectedTerms = new HashSet<string>(new string[] { "test", "object", "1234" });

            while (terms.Next())
            {
                if (String.Equals("String", terms.Term.Field))
                {
                    Assert.True(expectedTerms.Contains(terms.Term.Text));
                    expectedTerms.Remove(terms.Term.Text);
                }
            }

            Assert.AreEqual(0, expectedTerms.Count);
        }

        [Test]
        public void AddWithAnalyzerArgumentExceptions()
        {
            try
            {
                Index.ObjectMappingExtensions.Add(null, new object(), new StandardAnalyzer(Util.Version.LUCENE_30));
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("writer", ex.ParamName);
            }

            try
            {
                writer.Add<object>(null, new StandardAnalyzer(Util.Version.LUCENE_30));
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Add(new object(), null);
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

            Assert.AreEqual(0, writer.NumDocs());
            writer.Add(t, new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            IndexReader reader = IndexReader.Open(dir, true);
            TermEnum terms = reader.Terms();
            int nTerms = 0;

            while (terms.Next())
            {
                if (String.Equals("String", terms.Term.Field))
                {
                    Assert.AreEqual("Test Object 9876", terms.Term.Text);
                    nTerms++;
                }
            }

            Assert.AreEqual(1, nTerms);
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
                writer.Update<object>(null, new MatchAllDocsQuery());
                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("obj", ex.ParamName);
            }

            try
            {
                writer.Update(new object(), null);
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

            Assert.AreEqual(0, writer.NumDocs());
            writer.Add(t);
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, new TermQuery(new Term("String", "1234")));
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            IndexReader reader = IndexReader.Open(dir, true);
            TermEnum terms = reader.Terms();
            HashSet<string> expectedTerms = new HashSet<string>(new string[] { "something", "else", "2345" });

            while (terms.Next())
            {
                if (String.Equals("String", terms.Term.Field))
                {
                    if (expectedTerms.Contains(terms.Term.Text))
                    {
                        expectedTerms.Remove(terms.Term.Text);
                    }
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
        }

        [Test]
        public void UpdateWithKindSuccess()
        {
            TestObject t = new TestObject()
            {
                Number = 1234,
                String = "Test Object 1234",
            };

            Assert.AreEqual(0, writer.NumDocs());
            writer.Add<object>(t);
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, DocumentObjectTypeKind.Static, new TermQuery(new Term("String", "1234")));
            writer.Commit();
            Assert.AreEqual(2, writer.NumDocs());

            writer.DeleteDocuments<object>(new MatchAllDocsQuery());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            TestObject t3 = new TestObject()
            {
                Number = 3456,
                String = "Completely Different 3456",
            };

            writer.Update(t3, DocumentObjectTypeKind.Actual, new TermQuery(new Term("String", "2345")));
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            IndexReader reader = IndexReader.Open(dir, true);
            TermEnum terms = reader.Terms();
            HashSet<string> expectedTerms = new HashSet<string>(new string[] { "completely", "different", "3456" });

            while (terms.Next())
            {
                if (String.Equals("String", terms.Term.Field))
                {
                    if (expectedTerms.Contains(terms.Term.Text))
                    {
                        expectedTerms.Remove(terms.Term.Text);
                    }
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

            Assert.AreEqual(0, writer.NumDocs());
            writer.Add(t, new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, new TermQuery(new Term("String", "Test Object 1234")), new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            IndexReader reader = IndexReader.Open(dir, true);
            TermEnum terms = reader.Terms();
            int nTerms = 0;

            while (terms.Next())
            {
                if (String.Equals("String", terms.Term.Field))
                {
                    if (String.Equals("Something Else 2345", terms.Term.Text))
                    {
                        nTerms++;
                    }
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

            Assert.AreEqual(0, writer.NumDocs());
            writer.Add<object>(t, new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            TestObject t2 = new TestObject()
            {
                Number = 2345,
                String = "Something Else 2345",
            };

            writer.Update(t2, DocumentObjectTypeKind.Static, new TermQuery(new Term("String", "Test Object 1234")), new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(2, writer.NumDocs());

            writer.DeleteDocuments<object>(new MatchAllDocsQuery());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            TestObject t3 = new TestObject()
            {
                Number = 3456,
                String = "Completely Different 3456",
            };

            writer.Update(t3, DocumentObjectTypeKind.Actual, new TermQuery(new Term("String", "Something Else 2345")), new KeywordAnalyzer());
            writer.Commit();
            Assert.AreEqual(1, writer.NumDocs());

            IndexReader reader = IndexReader.Open(dir, true);
            TermEnum terms = reader.Terms();
            int nTerms = 0;

            while (terms.Next())
            {
                if (String.Equals("String", terms.Term.Field))
                {
                    if (String.Equals("Completely Different 3456", terms.Term.Text))
                    {
                        nTerms++;
                    }
                }
            }

            Assert.AreEqual(1, nTerms);
        }

        #endregion

        #region DeleteDocuments

        [Test]
        public void DeleteDocumentsFromIndex()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments(
                typeof(TestObject),
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>()); // Static Type: Object, Actual Type: TestObject
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments(
                typeof(TestObject),
                DocumentObjectTypeKind.Static,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            // No object must have been deleted yet, because the static type is set to Object, not TestObject.
            VerifyTestObjects(NumObjects, 0);

            writer.DeleteDocuments(
                typeof(TestObject),
                DocumentObjectTypeKind.Actual,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexGeneric()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments<TestObject>(
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexWithTypeKindCheckGeneric()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>()); // Static Type: Object, Actual Type: TestObject
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments<TestObject>(
                DocumentObjectTypeKind.Static,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            // No object must have been deleted yet, because the static type is set to Object, not TestObject.
            VerifyTestObjects(NumObjects, 0);

            writer.DeleteDocuments<TestObject>(
                DocumentObjectTypeKind.Actual,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        #endregion

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

        private void VerifyTestObjects(int count, int maxDeletedExclusive)
        {
            Assert.AreEqual(count - maxDeletedExclusive, writer.NumDocs());

            using (IndexReader reader = IndexReader.Open(dir, true))
            {
                for (int i = 0; i < count; i++)
                {
                    if (i < maxDeletedExclusive)
                    {
                        Assert.True(reader.IsDeleted(i));
                    }
                    else
                    {
                        Assert.False(reader.IsDeleted(i));
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
