// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Test.Interfaces
{
    using Orleans;

    using System.Diagnostics.CodeAnalysis;

    [GenerateSerializer]
    public record class Tag(string OriginHtml, IReadOnlyCollection<string> Attributes, string? Value);

    [GenerateSerializer]
    public record class TagQualify(Tag tag, string tagName, string? valueType);

    public class TagQualifyComparer : IEqualityComparer<TagQualify>, IEqualityComparer<Tag>
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static TagQualifyComparer()
        {
            Instance = new TagQualifyComparer();
        }

        private TagQualifyComparer()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static TagQualifyComparer Instance { get; }

        /// <summary>
        /// Equalses the specified x.
        /// </summary>
        public bool Equals(TagQualify? x, TagQualify? y)
        {
            var xnull = x is null;
            var ynull = y is null;

            if (xnull && ynull)
                return true;

            if (xnull || ynull)
                return false;

            if (object.ReferenceEquals(x, y))
                return true;

            return x!.valueType == y!.valueType &&
                   x!.tagName == y!.tagName &&
                   Equals(x.tag, y.tag);
        }

        /// <summary>
        /// Equalses the specified x.
        /// </summary>
        public bool Equals(Tag? x, Tag? y)
        {
            var xnull = x is null;
            var ynull = y is null;

            if (xnull && ynull)
                return true;

            if (xnull || ynull)
                return false;

            if (object.ReferenceEquals(x, y))
                return true;

            return x!.Value == y!.Value &&
                   x.OriginHtml == y.OriginHtml;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        public int GetHashCode([DisallowNull] TagQualify obj)
        {
            return (obj.tagName?.GetHashCode() ?? 0) ^
                   (obj.valueType?.GetHashCode() ?? 0) ^
                   (obj.tag != null ? GetHashCode(obj.tag) : 0);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        public int GetHashCode([DisallowNull] Tag obj)
        {
            return (obj.OriginHtml?.GetHashCode() ?? 0) ^
                   (obj.Value?.GetHashCode() ?? 0) ^
                   (obj.Attributes?.Aggregate(0, (acc, attr) => (attr?.GetHashCode() ?? 0) ^ acc) ?? 0);
        }

        #endregion
    }
}