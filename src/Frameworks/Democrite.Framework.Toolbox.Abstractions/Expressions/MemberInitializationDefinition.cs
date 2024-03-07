// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Expressions
{
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Definition that serialize a member initialization
    /// </summary>
    /// <seealso cref="IEquatable{MemberInitializationDefinition}" />
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class MemberInitializationDefinition : IEquatable<MemberInitializationDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInitializationDefinition"/> class.
        /// </summary>
        public MemberInitializationDefinition(ConcretType newType,
                                              IEnumerable<ConcretType> inputs,
                                              AbstractMethod? ctor,
                                              IEnumerable<MemberBindingDefinition> bindings)
        {
            this.Ctor = ctor;
            this.Inputs = inputs?.ToArray() ?? Array.Empty<ConcretType>();
            this.NewType = newType;
            this.Bindings = bindings?.ToArray() ?? Array.Empty<MemberBindingDefinition>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ctor.
        /// </summary>
        [DataMember]
        public AbstractMethod? Ctor { get; }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<ConcretType> Inputs { get; }

        /// <summary>
        /// Get type information to create
        /// </summary>
        [DataMember]
        public ConcretType NewType { get; }

        /// <summary>
        /// Gets the bindings.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<MemberBindingDefinition> Bindings { get; }

        #endregion

        #region Method

        /// <inheritdoc />
        public bool Equals(MemberInitializationDefinition? other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
