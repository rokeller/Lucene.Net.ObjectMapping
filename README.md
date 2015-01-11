# Lucene.Net.ObjectMapping
Mapping of .NET objects to Lucene.Net Documents and vice versa.

Uses JSON Serialization to store the object in the Lucene.Net Document, and indexes each property (also nested properties) individually.

Any object that is JSON serializable (using Newtonsoft.Json) can be mapped to a Document, like this:

    Document doc = myObject.ToDocument();

Similarly, a Document created like called out above can be converted back into an object like this:

    MyClass myObject = doc.ToObject<MyClass>();
    // or
    object myObject = doc.ToObject();

Since the library stores the actual type of the object when creating a Document, we can always reconstruct the same type of object when deserializing using `ToObject`, provided that the types can be deserialized from JSON.

Dependencies
------------
This library depends on the following NuGet packages.
* Lucene.Net
* Lucene.Net.Contrib
* Newtonsoft.Json
