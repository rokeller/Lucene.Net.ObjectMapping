# Lucene.Net.ObjectMapping
Mapping of .NET objects to Lucene.Net Documents and vice versa.

Installation
------------
Simply download the `Lucene.Net.ObjectMapping` NuGet package like this:

```
Install-Package Lucene.Net.ObjectMapping
```

You are ready to use the object mapping now.

How To Use It
-------------

Uses JSON Serialization to store the object in the Lucene.Net Document, and indexes each property (also nested properties) individually.

Any object that is JSON serializable (using Newtonsoft.Json) can be mapped to a Document, like this:

```c#
Document doc = myObject.ToDocument();
```

Similarly, a Document created like called out above can be converted back into an object like this:

```c#
MyClass myObject = doc.ToObject<MyClass>();
// or
object myObject = doc.ToObject();
```

Since the library stores the actual type of the object when creating a Document, we can always reconstruct the same type
of object when deserializing using `ToObject`, provided that the types can be deserialized from JSON.

You can add your documents to your indices the usual way, or you can add an object to an IndexWriter like this:

```c#
myIndexWriter.Add(myObject);
// or
myIndexWriter.Add(myObject, myAnalyzer);
```

Updating existing documents is just as easy. Like the `UpdateDocument` method on the `IndexReader`, it will delete the
documents that match the given query and then just add the new document. Use it like this:

```c#
myIndexWriter.Update(myObject, new TermQuery(new Term("Id", myObject.Id)));
// or
myIndexWriter.Update(myObject, new TermQuery(new Term("Id", myObject.Id)), myAnalyzer);
```

Please note that it is not necessary to filter for the document type in your query. The `Update` method does it automatically.

Search for Documents mapped from a specifc class using the extensions to the Searcher, e.g. like this:

```c#
mySearcher.Search<MyClass>(myQuery, numResultsToReturn, mySort);
```

There are other extensions to the Searcher to, also non-generic ones just in case.

Read More
---------

You can read more about Lucene.Net.ObjectMapping on [my blog](http://www.cymbeline.ch/lucene-net-objectmapping/).

Dependencies
------------
This library depends on the following NuGet packages.
* Lucene.Net
* Newtonsoft.Json
