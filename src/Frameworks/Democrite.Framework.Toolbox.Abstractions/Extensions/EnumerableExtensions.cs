// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System.Collections.Generic
{
    using System;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public static class EnumerableExtensions
    {
        /// <summary>
        /// Flattern result through tree recursion
        /// </summary>
        public static IEnumerable<TInst> GetTreeValues<TInst>(this TInst instance, Func<TInst?, IEnumerable<TInst?>?> getChildren)
        {
            if (!EqualityComparer<TInst>.Default.Equals(instance, default))
            {
                yield return instance;

                foreach (var child in getChildren(instance) ?? Array.Empty<TInst>())
                {
                    foreach (var childInst in GetTreeValues(child, getChildren))
                    {
                        if (!EqualityComparer<TInst>.Default.Equals(childInst, default))
                            yield return childInst!;
                    }
                }
            }
        }

        /// <summary>
        /// Flattern result through tree recursion
        /// </summary>
        public static IEnumerable<TInst> GetTreeValues<TInst>(this TInst instance, Func<TInst, TInst?> getChildren)
        {
            return GetTreeValues<TInst>(instance, i => EqualityComparer<TInst>.Default.Equals(i, default)
                                                            ? Array.Empty<TInst>()
                                                            : new[] { getChildren(i!) });
        }

        /// <summary>
        /// Converts collection to readonly.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyCollection<TInst> ToReadOnly<TInst>(this IEnumerable<TInst>? collection)
        {
            if (collection is null)
                return Array.Empty<TInst>();

            if (collection is IReadOnlyCollection<TInst> readOnly)
                return readOnly;

            if (collection is IList<TInst> list)
                return new ReadOnlyCollection<TInst>(list);

            return collection.ToImmutableArray();
        }

        /// <summary>
        /// Converts collection to readonly.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<TInst> ToReadOnlyList<TInst>(this IEnumerable<TInst>? collection)
        {
            if (collection is null)
                return Array.Empty<TInst>();

            if (collection is IReadOnlyList<TInst> readOnly)
                return readOnly;

            if (collection is IList<TInst> list)
                return new ReadOnlyCollection<TInst>(list);

            return collection.ToImmutableList();
        }

        /// <summary>
        /// Turn <paramref name="instance"/> into a collection lightweight <see cref="IEnumerable{TItem}"/>
        /// </summary>
        public static IEnumerable<TItem> AsEnumerable<TItem>(this TItem instance)
        {
            yield return instance;
        }

        /// <summary>
        /// Expose node iterations
        /// </summary>
        public static IEnumerable<LinkedListNode<TItem>> Nodes<TItem>(this LinkedList<TItem>? collection)
        {
            if (collection is null)
                yield break;

            var current = collection!.First;
            while (current != null)
            {
                yield return current;

                current = current.Next;
            }
        }

        /// <summary>
        /// Adds after tested node when condition is validate.
        /// </summary>
        /// <param name="predicate">Func(Previous, Current, newNodeContent, Bool)</param>
        public static LinkedListNode<TNode> AddAfterWhen<TNode>(this LinkedList<TNode> collection, Func<TNode?, TNode, TNode, bool> predicate, TNode newNodeContent)
        {
            ArgumentNullException.ThrowIfNull(nameof(collection));

            foreach (var node in collection.Nodes())
            {
                TNode? previousNode = default;

                if (node.Previous != null)
                    previousNode = node.Previous.Value;

                if (predicate(previousNode, node.Value, newNodeContent))
                {
                    if (node.Previous == null)
                        return collection.AddBefore(node, newNodeContent);

                    return collection.AddAfter(node.Previous, newNodeContent);
                }
            }

            if (collection.First == null)
                return collection.AddFirst(newNodeContent);

            return collection.AddLast(newNodeContent);
        }
    }
}
