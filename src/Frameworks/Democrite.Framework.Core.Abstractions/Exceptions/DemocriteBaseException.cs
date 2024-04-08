// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System;
    using System.Collections;
    using System.Text;

    /// <summary>
    /// Base class exception of all system execeptions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class DemocriteBaseException : Exception, IDemocriteException, IEquatable<DemocriteBaseException>
    {
        #region Fields
        
        private readonly SortedDictionary<string, object> _data;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteBaseException"/> class.
        /// </summary>
        public DemocriteBaseException(string message, ulong errorCode = 0, Exception? innerException = null)
            : base(message, innerException)
        {
            this._data = new SortedDictionary<string, object>(StringComparer.InvariantCulture);

            this.ErrorCode = errorCode;
            this.Data.Add(DemocriteErrorCodes.KEY, errorCode);

#if DEBUG
            this.Data.Add(DemocriteErrorCodes.KEY + "Debug", DemocriteErrorCodes.ToDecryptErrorCode(errorCode));
#endif
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        /// <remarks>
        ///     override to have a correct comparaison
        /// </remarks>
        public sealed override IDictionary Data
        {
            get { return this._data; }
        }

        /// <inheritdoc />
        public ulong ErrorCode { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public sealed override bool Equals(object? obj)
        {
            if (obj is DemocriteBaseException ex)
                return Equals(ex);
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(DemocriteBaseException? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return string.Equals(this.Message, other.Message) &&
                   (this.InnerException?.Equals(other.InnerException) ?? other.InnerException is null) &&
                   this.ErrorCode == other.ErrorCode && 
                   this._data.All(kv => other.Data.Contains(kv.Key) &&
                                       ((object.Equals(other.Data[kv.Key], null) && object.Equals(kv.Value, null)) || other.Data[kv.Key]!.Equals(kv.Value))) &&
                   OnEquals(other);
        }

        /// <summary>
        /// Called when equality is validated
        /// </summary>
        protected virtual bool OnEquals(DemocriteBaseException other)
        {
            return true;
        }

        /// <inheritdoc/>
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.Message,
                                    this.InnerException,
                                    this.ErrorCode,
                                    this._data.Aggregate(0, (acc, kv) => HashCode.Combine(acc, kv.Key, kv.Value)),
                                    this.OnGetHashCode());
        }

        /// <summary>
        /// Called when to compute hash code
        /// </summary>
        protected virtual int OnGetHashCode()
        {
            return 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var build = new StringBuilder();

            build.Append(this.GetType().Name);
            build.AppendLine();
            
            build.Append("Message: ");
            build.Append(this.Message);
            build.AppendLine();

            if (this._data.Count > 0)
            {
                build.AppendLine();
                build.AppendLine("Data:");
            }

            foreach (var data in this._data)
            {
                build.Append(data.Key);
                build.Append(": ");
                build.Append(data.Value);
                build.AppendLine();
            }

            if (this.InnerException != null)
            {
                build.AppendLine();
                build.Append("-- Inner --");
                build.AppendLine();
                build.Append(this.InnerException);
                build.AppendLine();
            }

            return build.ToString();
        }

        #endregion
    }

    public abstract class DemocriteBaseException<TChild> : DemocriteBaseException, IEquatable<TChild>
    {
        protected DemocriteBaseException(string message,
                                         ulong errorCode = 0,
                                         Exception? innerException = null) 
            : base(message, errorCode, innerException)
        {
        }

        public bool Equals(TChild? other)
        {
            return base.Equals(other);
        }
    }

    public static class DemocriteExceptionExtension
    {
        /// <summary>
        /// Create a <see cref="DemocriteInternalException"/> to carry the exception (client side) through democrite enviroment
        /// </summary>
        /// <remarks>
        ///     If exception is arleady a <see cref="DemocriteInternalException"/> the exception is simply returned
        /// </remarks>
        public static DemocriteInternalException ToDemocriteInternal(this DemocriteBaseException exception)
        {
            if (exception is DemocriteInternalException internalException)
                return internalException;

            return new DemocriteInternalException(exception);
        }
    }
}
