Voyagers.Utilities.ObjectComparer
=================================

ObjectComparer library that compares and reports the differences between two .NET objects.

Usage
--------
1. Pass .NET objects into the static method `ObjectComparer.GetObjectVariances(dynamic object1, dynamic object2)` method
2. An `IEnumerable<ObjectVariance>` is returned, containing all differences from `object1` and `object2`.
3. Note only public and instance properties are compared.

Traversing the result `IEnumerable<ObjectVariance>`
----------
`ObjectVariance` has the following properties:
- `PropertyName`
  - Name of the property that has a difference within the object graph
- `PropertyValue1`
  - Value of `object1.PropertyName`
- `PropertyValue2`
  - Value of `object2.PropertyName`
- `ParentVariance`
  - Recursive reference to the outer `ObjectVariance` when comparing properties that can be further traversed.

Sample
----------
`ObjectComparer.GetObjectVariances("test", "tast")` returns the following:
- `IEnumerable<ObjectVariance>`
  - `this[0]`
    - `PropertyName`: `"IEnumerable<char> at index 1"`
    - `Value1`: `'e'`
    - `Value2`: `'a'`
    - `ParentVariance`:
      - `PropertyName`: `null`
      - `Value1`: `"test"`
      - `Value2`: `"tast"`
      - `ParentVariance`: `null`
