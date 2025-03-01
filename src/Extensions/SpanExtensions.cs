using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Extensions;
public static class SpanExtensions
{
    /// <summary>
    /// ForEach extension method for iterating over a list by accessing the internal array and using it as a Span.
    /// WARNING: Do not modify the list while iterating over it (e.g. add or remove). You can modify the items in the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    public static void ForEachSpan<T>(this List<T> items, Action<T> action)
    {
        var listAsSpan = CollectionsMarshal.AsSpan(items);
        ref var searchSpace = ref MemoryMarshal.GetReference(listAsSpan);

        for (var i = 0; i < listAsSpan.Length; i++)
        {
            var item = Unsafe.Add(ref searchSpace, i);
            action(item);
        }
    }

}

/*
 Sure! Let's delve deeper into the collections used and discuss their time and space complexities along with their memory footprints.

### List<T>
- **Definition:** A `List<T>` is a generic collection that can store elements of type `T`. It provides dynamic array functionality, allowing elements to be added or removed as needed.
- **Memory Footprint:**
  - The memory footprint of a `List<T>` depends on its capacity, which can grow dynamically. Initially, a `List<T>` allocates an array with a default capacity.
  - As elements are added, if the capacity is exceeded, the list allocates a new array with double the previous capacity, copying the existing elements.
  - The memory footprint includes the overhead for managing the array and any unused space in the allocated capacity.
- **Time Complexity:**
  - Access: O(1) - Direct access to elements by index.
  - Search: O(n) - Linear search in the worst case.
  - Insertion/Deletion: O(n) - Inserting or deleting elements can require shifting elements.
- **Space Complexity:** O(n) - Space needed grows linearly with the number of elements.

### Dictionary<TKey, TValue>
- **Definition:** A `Dictionary<TKey, TValue>` is a collection of key-value pairs, where each key is unique. It provides fast lookups, insertions, and deletions based on the key.
- **Memory Footprint:**
  - The memory footprint of a `Dictionary<TKey, TValue>` includes the space for an array of buckets and the entries for key-value pairs.
  - Each entry typically consists of a key, a value, and metadata such as hash codes and references to handle collisions.
  - The dictionary resizes its internal array when the load factor (number of elements vs. capacity) exceeds a certain threshold, leading to some memory overhead.
- **Time Complexity:**
  - Access: O(1) - Average case for accessing values by key.
  - Search: O(1) - Average case for searching values by key.
  - Insertion/Deletion: O(1) - Average case for inserting or deleting elements by key.
- **Space Complexity:** O(n) - Space needed grows linearly with the number of key-value pairs.

### ArraySegment<T>
- **Definition:** An `ArraySegment<T>` provides a way to reference a subset of an array without copying the elements. It is a lightweight structure that holds a reference to the original array, along with an offset and count.
- **Memory Footprint:**
  - The memory footprint of an `ArraySegment<T>` is minimal, as it only stores references to the original array, the offset, and the count.
  - It does not allocate additional memory for the elements themselves, making it efficient in terms of memory usage.
- **Time Complexity:**
  - Access: O(1) - Direct access to elements by index within the segment.
  - Search: O(n) - Linear search within the segment.
- **Space Complexity:** O(1) - Only stores references and metadata, no additional space for elements.

### ReadOnlyMemory<T>
- **Definition:** `ReadOnlyMemory<T>` represents a contiguous region of memory that is read-only. It can be backed by an array, string, or unmanaged memory.
- **Memory Footprint:**
  - The memory footprint of `ReadOnlyMemory<T>` is similar to that of `ArraySegment<T>` in that it holds a reference to the underlying data without copying it.
  - It includes metadata for managing the memory region, but it does not allocate additional memory for the elements themselves.
- **Time Complexity:**
  - Access: O(1) - Direct access to elements by index within the memory region.
  - Search: O(n) - Linear search within the memory region.
- **Space Complexity:** O(1) - Only stores references and metadata, no additional space for elements.

### Span<T>
- **Definition:** `Span<T>` is a stack-allocated type that represents a contiguous region of memory. It is a lightweight struct that can be used to access arrays, memory from the stack, and unmanaged memory.
- **Memory Footprint:**
  - The memory footprint of `Span<T>` is minimal, as it does not allocate additional memory. It holds a reference to the underlying data and a length.
  - `Span<T>` is meant to be used for short-lived, high-performance operations and does not incur garbage collection overhead.
- **Time Complexity:**
  - Access: O(1) - Direct access to elements by index.
  - Search: O(n) - Linear search within the span.
- **Space Complexity:** O(1) - Only stores references and metadata, no additional space for elements.

### ReadOnlySpan<T>
- **Definition:** `ReadOnlySpan<T>` is similar to `Span<T>` but provides read-only access to the memory region. It is also a stack-allocated type.
- **Memory Footprint:**
  - The memory footprint of `ReadOnlySpan<T>` is minimal and similar to that of `Span<T>`. It holds a reference to the underlying data and a length.
  - It provides efficient, read-only access without additional memory allocation.
- **Time Complexity:**
  - Access: O(1) - Direct access to elements by index.
  - Search: O(n) - Linear search within the span.
- **Space Complexity:** O(1) - Only stores references and metadata, no additional space for elements.

Each of these collections and types has different use cases and performance characteristics, making them suitable for various scenarios in modern C# development. Let me know if you need further details or have any other questions!
 */
