# AnyClone
[![nuget](https://img.shields.io/nuget/v/AnyClone.svg)](https://www.nuget.org/packages/AnyClone/)
[![nuget](https://img.shields.io/nuget/dt/AnyClone.svg)](https://www.nuget.org/packages/AnyClone/)
[![Build status](https://ci.appveyor.com/api/projects/status/xr7gebcdins0hs4f?svg=true)](https://ci.appveyor.com/project/MichaelBrown/anyclone)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/24f78d682c844168809617fd56ecad0f)](https://www.codacy.com/app/replaysMike/AnyClone?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=replaysMike/AnyClone&amp;utm_campaign=Badge_Grade)
[![Codacy Badge](https://api.codacy.com/project/badge/Coverage/24f78d682c844168809617fd56ecad0f)](https://www.codacy.com/app/replaysMike/AnyClone?utm_source=github.com&utm_medium=referral&utm_content=replaysMike/AnyClone&utm_campaign=Badge_Coverage)

A CSharp library that can deep clone any object using only reflection.

No requirements for `[Serializable]` attributes, supports standard ignore attributes.

## Description

I built this library as almost all others I tried on complex objects either didn't work at all, or failed to account for common scenarios. Serialization required too much boiler plate (BinarySerialization, Protobuf, or Json.Net) and fails to account for various designs. Implementing IClonable was too much of a chore and should be unnecessary. Various projects that use expression trees also failed to work, IObservable patterns were difficult to implement on large, already written code base.

## Installation
Install AnyClone from the Package Manager Console:
```
PM> Install-Package AnyClone
```

## Usage

```csharp
using AnyClone.Extensions;

var originalObject = new SomeComplexTypeWithDeepStructure();
var myClonedObject = originalObject.Clone();
```

Capture Errors
```csharp
using AnyClone.Extensions;

var originalObject = new SomeComplexTypeWithDeepStructure();
// capture errors found with your object where impossible situations occur, and add [IgnoreDataMember] to those properties/fields.

var myClonedObject = originalObject.Clone((ex, path, property, obj) => {
  Console.WriteLine($"Cloning error: {path} {ex.Message}");
  Assert.Fail();
});
```

Get differences between cloned objects using [AnyDiff](https://github.com/replaysMike/AnyDiff)
```csharp
using AnyClone.Extensions;
using AnyDiff;

var object1 = new MyComplexObject(1, "A string");
var object1Snapshot = object1.Clone();

var diff = AnyDiff.Diff(object1, object1Snapshot);
Assert.AreEqual(diff.Count, 0);

// change something anywhere in the object tree
object1.Name = "A different string";

diff = AnyDiff.Diff(object1, object1Snapshot);
Assert.AreEqual(diff.Count, 1);
```

### Ignoring Properties/Fields
There are unfortunately a few situations that can't be resolved, such as cloning delegates, events etc. Fortunately, you can specify attributes that AnyClone will skip properties decorated with them. By default, AnyClone will ignore properties decorated with the following attributes: `IgnoreDataMemberAttribute, NonSerializedAttribute, JsonIgnoreAttribute`. If you wish to disable this behavior, or provide other attributes that you wish to decorate properties to ignore you can provide a custom CloneConfiguration:

```csharp
using AnyClone;
using AnyClone.Extensions;

var originalObject = new SomeComplexTypeWithDeepStructure();
var myClonedObject = originalObject.Clone(CloneConfiguration.UsingAttributeNamesToIgnore("IgnoreDataMemberAttribute", "MyCustomAttribute"));
```

Both attribute names and attribute types can be specified:
```csharp
var myClonedObject = originalObject.Clone(CloneConfiguration.UsingAttributeNamesToIgnore("IgnoreDataMemberAttribute", "MyCustomAttribute"));
var myOtherClonedObject = originalObject.Clone(CloneConfiguration.UsingAttributesToIgnore(typeof(IgnoreDataMemberAttribute), typeof(MyCustomAttribute)));
```

### Other Applications

If you need to perform serialization instead of deep copying, try out [AnySerializer](https://github.com/replaysMike/AnySerializer) which doesn't require attribute decoration and works on complex structures.
