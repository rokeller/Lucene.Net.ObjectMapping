# Mapping of .NET objects to Lucene.Net Documents and vice versa

Uses JSON Serialization to store the object in the Lucene.Net Document, and
indexes each property (also nested properties) individually.

Any object that is JSON serializable (using Newtonsoft.Json) can be mapped to a
Document, like this:

```csharp
Document doc = myObject.ToDocument();
```

Similarly, a Document created like called out above can be converted back into
an object like this:

```csharp
MyClass myObject = doc.ToObject<MyClass>();
// or
object myObject = doc.ToObject();
```

Since the library stores the actual type of the object when creating a Document,
we can always reconstruct the same type of object when deserializing using
`ToObject`, provided that the types can be deserialized from JSON.

You can add your documents to your indices the usual way, or you can add an
object to an IndexWriter like this:

```csharp
myIndexWriter.Add(myObject);
// or
myIndexWriter.Add(myObject, myAnalyzer);
```

Updating existing documents is just as easy. Like the `UpdateDocument` method on
the `IndexWriter`, it will delete the documents that match the given query and
then just add the new document. Use it like this:

```csharp
myIndexWriter.Update(myObject, new TermQuery(new Term("Id", myObject.Id)));
// or
myIndexWriter.Update(myObject, new TermQuery(new Term("Id", myObject.Id)), myAnalyzer);
```

Please note that it is not necessary to filter for the document type in your
query. The `Update` method does it automatically.

Search for Documents mapped from a specific class using the extensions to the
`IndexSearcher`, e.g. like this:

```csharp
mySearcher.Search<MyClass>(myQuery, numResultsToReturn, mySort);
```

There are other extensions to the `IndexSearcher` to, also non-generic ones in
case those are needed.
