ObjectComparer
=================================

ObjectComparer library that compares and reports the differences between two .NET objects. It is made to specifically cater for  comparisons between two entities using [Entity Framework](http://msdn.microsoft.com/en-us/data/ef.aspx)

Features
--------
* Supports deep object comparisons
* Supports [`KeyAttribute`](http://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.keyattribute(v=vs.110).aspx) (If found, `IEnumerable<TEntity>` comparisons will be key-based instead of position-based)
* Supports [`MetadataTypeAttribute`](http://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.metadatatypeattribute(v=vs.110).aspx) (If found, `ObjectComparer` will take into consideration of your `Metadata` `partial` class)
* Supports `IgnoreVarianceAttribute`
    * If found on a property, it will ignore variances on that property and stop traversing further
    * If found on a class definition, it will ignore variances on the entire class
* `ObjectComparer` will stop traversing when the following data types are encountered:
    * `Type.IsPrimitive` returns `true`
    * `string`
    * `DateTime`
    * `null` properties

Usage
--------
1. Pass .NET objects into the static method `ObjectComparer.GetObjectVariances(object object1, object object2)` method
2. An `IEnumerable<ObjectVariance>` is returned, containing all differences from `object1` and `object2` (by default it will traverse the object graph until conditions stated above returns `true`)
3. Note only public and instance properties are compared.

Traversing the result `IEnumerable<ObjectVariance>`
----------
`ObjectVariance` has the following properties:

* `PropertyName`
  * Name of the property that has a difference within the object graph
* `PropertyValue1`
  * Value of `object1.PropertyName`
* `PropertyValue2`
  * Value of `object2.PropertyName`
* `ParentVariance`
  * Recursive reference to the outer `ObjectVariance` when comparing properties that can be further traversed.

Sample
----------
`ObjectComparer.GetObjectVariances("test", "tast")` returns the following:

- `IEnumerable<ObjectVariance>`
  - `this.ElementAtOrDefault(0)`
    - `PropertyName`: `null`
    - `Value1`: `"test"`
    - `Value2`: `"tast"`
    - `ParentVariance`: `null`
