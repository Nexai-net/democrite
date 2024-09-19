﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.CodeGenerator
{
    using Microsoft.CodeAnalysis;

    using System.Collections.Generic;

    /// <summary>
    /// Managed use to easy the error reporting during mapping compilation
    /// </summary>
    internal static class ErrorManager
    {
        #region Fields

        private readonly static IReadOnlyDictionary<string, DiagnosticDescriptor> s_errorInfo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ErrorManager"/> class.
        /// </summary>
        static ErrorManager()
        {
            s_errorInfo = new Dictionary<string, DiagnosticDescriptor>()
            {
                 
                {  "EDG001", new DiagnosticDescriptor("EDG001", "simpleNameIdentifier missing", "Missing value/field named 'simpleNameIdentifier'", "Mapper", DiagnosticSeverity.Error, isEnabledByDefault: true) },
                 
                {  "EDG999", new DiagnosticDescriptor("EDG999", "Internal Error", "Mapper failed {0}", "Mapper", DiagnosticSeverity.Error, isEnabledByDefault: true) },
                
            };
        }

        #endregion

        #region Properties

        /// <summary>
        /// simpleNameIdentifier missing : Missing value/field named 'simpleNameIdentifier'
        /// </summary>
        public static DiagnosticDescriptor EDG001
        {
            get { return s_errorInfo["EDG001"]; }
        }
        /// <summary>
        /// Internal Error : Mapper failed {0}
        /// </summary>
        public static DiagnosticDescriptor EDG999
        {
            get { return s_errorInfo["EDG999"]; }
        }
        
        #endregion
    }
}
