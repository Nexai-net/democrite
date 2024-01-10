// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System;

    public interface IDemocriteBaseExceptionSurrogate
    {
        public string Message { get; set; }

        public ulong ErrorCode { get; set; }

        public Exception? InnerException { get; set; }
    }
}
