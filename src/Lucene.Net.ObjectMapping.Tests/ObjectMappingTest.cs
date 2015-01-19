using Lucene.Net.Documents;
using Lucene.Net.ObjectMapping.Tests.Model;
using NUnit.Framework;
using System;

namespace Lucene.Net.ObjectMapping.Tests
{
    [TestFixture]
    public class ObjectMappingTest
    {
        [Test]
        public void PropertyTypes()
        {
            DateTime now = DateTime.Now;

            TestObject obj = new TestObject()
            {
                Guid = Guid.NewGuid(),
                Boolean = true,
                Float = (float)Math.PI,
                Null = null,
                Number = 1234,
                String = "This is a simple string for test 'MapObjectToDocument'.",
                TimeSpan = TimeSpan.FromDays(Math.E),
                TimestampLocal = now,
                TimestampUtc = now.ToUniversalTime(),
                Uri = new Uri("https://github.com/rokeller/Lucene.Net.ObjectMapping"),

                Double = Math.PI,
                Decimal = new Decimal(Math.E),
                Long = 98765432109,
                Short = 987,
                Byte = 123,
            };

            Document doc = obj.ToDocument();
            Assert.NotNull(doc);
            int remainingFields = 15 /* total fields */ - 1 /* null field */;

            foreach (IFieldable field in doc.GetFields())
            {
                if (field.IsStored)
                {
                    // Ignore stored fields here.
                    continue;
                }

                remainingFields--;

                switch (field.Name)
                {
                    case "Guid":
                        Assert.False(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Guid.ToString(), field.StringValue);
                        break;

                    case "Boolean":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual("1", field.StringValue);
                        break;

                    case "Float":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Float.ToString(), field.StringValue);
                        break;

                    case "Null":
                        Assert.Fail("Null values must not be indexed.");
                        break;

                    case "Number":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Number.ToString(), field.StringValue);
                        break;

                    case "String":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.String.ToString(), field.StringValue);
                        break;

                    case "TimeSpan":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.TimeSpan.Ticks.ToString(), field.StringValue);
                        break;

                    case "TimestampLocal":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.TimestampLocal.Ticks.ToString(), field.StringValue);
                        break;

                    case "TimestampUtc":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.TimestampUtc.Ticks.ToString(), field.StringValue);
                        break;

                    case "Uri":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Uri.ToString(), field.StringValue);
                        break;

                    case "Double":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Double.ToString(), field.StringValue);
                        break;

                    case "Decimal":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Decimal.ToString(), field.StringValue);
                        break;

                    case "Long":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Long.ToString(), field.StringValue);
                        break;

                    case "Short":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Short.ToString(), field.StringValue);
                        break;

                    case "Byte":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(obj.Byte.ToString(), field.StringValue);
                        break;

                    default:
                        Assert.Fail("Must get one of the expected fields.");
                        break;
                }
            }

            Assert.AreEqual(0, remainingFields);
        }

        [Test]
        public void RoundTrip()
        {
            DateTime now = DateTime.Now;

            TestObject obj = new TestObject()
            {
                Guid = Guid.NewGuid(),
                Boolean = true,
                Float = (float)Math.PI,
                Null = null,
                Number = 1234,
                String = "This is a simple string for test 'RoundTrip'.",
                TimeSpan = TimeSpan.FromDays(Math.E),
                TimestampLocal = now,
                TimestampUtc = now.ToUniversalTime(),
                Uri = new Uri("https://github.com/rokeller/Lucene.Net.ObjectMapping"),
            };

            Document doc = obj.ToDocument();

            TestObject obj2 = doc.ToObject<TestObject>();

            Assert.AreEqual(obj.Guid, obj2.Guid);
            Assert.AreEqual(obj.Boolean, obj2.Boolean);
            Assert.AreEqual(obj.Float, obj2.Float);
            Assert.AreEqual(obj.Null, obj2.Null);
            Assert.AreEqual(obj.Number, obj2.Number);
            Assert.AreEqual(obj.String, obj2.String);
            Assert.AreEqual(obj.TimeSpan, obj2.TimeSpan);
            Assert.AreEqual(obj.TimestampLocal, obj2.TimestampLocal);
            Assert.AreEqual(obj.TimestampUtc, obj2.TimestampUtc);
            Assert.AreEqual(obj.Uri, obj2.Uri);

            Assert.AreEqual(obj2.TimestampLocal, obj2.TimestampUtc.ToLocalTime());
        }

        [Test]
        public void NestedProperties()
        {
            NestedTestObjectA a = new NestedTestObjectA()
            {
                Id = Guid.NewGuid(),
                B = new NestedTestObjectB()
                {
                    Id = Int32.MaxValue,
                    C = new NestedTestObjectC()
                    {
                        String = "Test string for 'NestedProperties'.",
                        Array = new string[] { "First", "Second", "Third" },
                    }
                }
            };

            Document doc = a.ToDocument();
            Assert.NotNull(doc);
            int remainingFields = 3 /* normal fields */ + 3 /* array fields */;
            int arrayIndex = 0;

            foreach (IFieldable field in doc.GetFields())
            {
                if (field.IsStored)
                {
                    // Ignore stored fields here.
                    continue;
                }

                remainingFields--;

                switch (field.Name)
                {
                    case "Id":
                        Assert.False(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(a.Id.ToString(), field.StringValue);
                        break;

                    case "B.Id":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(a.B.Id.ToString(), field.StringValue);
                        break;

                    case "B.C.String":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(a.B.C.String, field.StringValue);
                        break;

                    case "B.C.Array":
                        Assert.True(field.IsTokenized);
                        Assert.True(field.IsIndexed);
                        Assert.AreEqual(a.B.C.Array[arrayIndex], field.StringValue);
                        arrayIndex++;
                        break;

                    default:
                        Assert.Fail("Must get one of the expected fields.");
                        break;
                }
            }

            Assert.AreEqual(0, remainingFields);

            NestedTestObjectA a2 = doc.ToObject<NestedTestObjectA>();

            Assert.AreEqual(a.Id, a2.Id);
            Assert.AreEqual(a.B.Id, a2.B.Id);
            Assert.AreEqual(a.B.C.String, a2.B.C.String);

            Assert.AreEqual(a.B.C.Array.Length, a2.B.C.Array.Length);
            for (int i = 0; i < a.B.C.Array.Length; i++)
            {
                Assert.AreEqual(a.B.C.Array[i], a2.B.C.Array[i]);
            }
        }

        [Test]
        public void NonGenericToObject()
        {
            DateTime now = DateTime.Now;

            TestObject obj = new TestObject()
            {
                Guid = Guid.NewGuid(),
                Boolean = true,
                Float = (float)Math.PI,
                Null = null,
                Number = 1234,
                String = "This is a simple string for test 'RoundTrip'.",
                TimeSpan = TimeSpan.FromDays(Math.E),
                TimestampLocal = now,
                TimestampUtc = now.ToUniversalTime(),
                Uri = new Uri("https://github.com/rokeller/Lucene.Net.ObjectMapping"),
            };

            Document doc = obj.ToDocument();

            TestObject obj2 = (TestObject)doc.ToObject();

            Assert.AreEqual(obj.Guid, obj2.Guid);
            Assert.AreEqual(obj.Boolean, obj2.Boolean);
            Assert.AreEqual(obj.Float, obj2.Float);
            Assert.AreEqual(obj.Null, obj2.Null);
            Assert.AreEqual(obj.Number, obj2.Number);
            Assert.AreEqual(obj.String, obj2.String);
            Assert.AreEqual(obj.TimeSpan, obj2.TimeSpan);
            Assert.AreEqual(obj.TimestampLocal, obj2.TimestampLocal);
            Assert.AreEqual(obj.TimestampUtc, obj2.TimestampUtc);
            Assert.AreEqual(obj.Uri, obj2.Uri);

            Assert.AreEqual(obj2.TimestampLocal, obj2.TimestampUtc.ToLocalTime());
        }

        [Test]
        public void ArgumentExceptionForMissingSettings()
        {
            try
            {
                TestObject obj = new TestObject();
                obj.ToDocument(null);

                Assert.Fail("Must get an exception.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("mappingSettings", ex.ParamName);
            }
        }
    }
}
