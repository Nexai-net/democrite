// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// KEEP : Democrite.Framework.Core
namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System.Collections.Frozen;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper used to format, validate and generate refId
    /// </summary>
    public static class RefIdHelper
    {
        #region Fields

        public const string REF_SCHEMA = "ref";
        public const string DEFAULT_NAMESPACE = "global";

        private readonly static IReadOnlyDictionary<RefTypeEnum, string> s_typeToString;
        private static readonly IReadOnlyDictionary<string, RefTypeEnum> s_stringToType;

        private readonly static Regex s_simpleNameIdentifierValidator;
        private readonly static Regex s_namespaceValidator;

        private readonly static Regex s_refIdValidator;

        #endregion

        /// <summary>
        /// Initializes the <see cref="RefIdHelper"/> class.
        /// </summary>
        static RefIdHelper()
        {
            s_typeToString = new Dictionary<RefTypeEnum, string>()
            {
                { RefTypeEnum.Sequence, "seq" },
                { RefTypeEnum.Signal, "sgl" },
                { RefTypeEnum.Door, "dor" },
                { RefTypeEnum.VGrain, "vgr" },
                { RefTypeEnum.VGrainImplementation, "gri" },
                { RefTypeEnum.Trigger, "tgr" },
                { RefTypeEnum.Artifact, "art" },
                { RefTypeEnum.StreamQueue, "stm" },
                { RefTypeEnum.BlackboardTemplate, "bbt" },
                { RefTypeEnum.BlackboardController, "bbc" },
                { RefTypeEnum.Type, "typ" },
                { RefTypeEnum.Method, "mth" },
                { RefTypeEnum.Other, "otr" },

            }.ToFrozenDictionary();

#if DEBUG
            foreach (var type in Enum.GetValues<RefTypeEnum>().Where(d => d != RefTypeEnum.None))
            {
                if (!s_typeToString.ContainsKey(type))
                    throw new InvalidOperationException("DEFINITION TYPE is not mapped in the RefIdHelper : " + type);
            }
#endif

            s_stringToType = s_typeToString.ToFrozenDictionary(kv => kv.Value, kv => kv.Key);

            var simpleNameIdentifierPattern = "[a-z0-9_-]{2,}";
            var namespacePattern = "[a-z0-9.]{2,}";

            s_simpleNameIdentifierValidator = new Regex("^" + simpleNameIdentifierPattern + "$");
            s_namespaceValidator = new Regex("^" + namespacePattern + "$");
            s_refIdValidator = new Regex("^" + REF_SCHEMA + "://(" + string.Join("|", s_typeToString.Values) + ")@" + namespacePattern + "/" + simpleNameIdentifierPattern + "(#" + simpleNameIdentifierPattern + ")?$");
        }

        /// <summary>
        /// Generates the resource RedId from information
        /// </summary>
        public static Uri Generate(RefTypeEnum type, string simpleNameIdentifier, string? @namespace = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(simpleNameIdentifier);

            if (!s_simpleNameIdentifierValidator.IsMatch(simpleNameIdentifier))
                throw new InvalidDataException("'{0}' - SNI (Simple Name Identifier) must be lower and alphanumerical, ... min 2 => [a-z0-9_-](2,)".WithArguments(simpleNameIdentifier));

            if (type == RefTypeEnum.None)
                throw new InvalidDataException(nameof(type) + " must not be None");

            var defaultNamespace = string.IsNullOrWhiteSpace(@namespace);

            if (!defaultNamespace && !s_namespaceValidator.IsMatch(@namespace!))
                throw new InvalidDataException("'{0}' - Namespace must be lower and alphanumerical and point min 2 => [a-z0-9.](2,)".WithArguments(@namespace));

            var builder = new UriBuilder
            {
                Scheme = REF_SCHEMA
            };

            if (s_typeToString.TryGetValue(type, out var typeName))
                builder.UserName = typeName;

            if (defaultNamespace)
                builder.Host = DEFAULT_NAMESPACE;
            else
                builder.Host = @namespace;

            builder.Path = simpleNameIdentifier;

            return builder.Uri;
        }

        /// <summary>
        /// Withes the method.
        /// </summary>
        public static Uri WithMethod(Uri sourceRefId, string simpleNameMethodIndentifier)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(simpleNameMethodIndentifier);

            Explode(sourceRefId, out var _, out var @namespace, out var sni);

            var buider = new UriBuilder()
            {
                Scheme = REF_SCHEMA,
                UserName = s_typeToString[RefTypeEnum.Method],
                Host = @namespace,
                Path = sni,
                Fragment = simpleNameMethodIndentifier
            };

            return buider.Uri;
        }

        /// <summary>
        /// Determines whether this <paramref name="uri"/> follow the Definition Ref norme
        /// </summary>
        public static bool IsRefId(this Uri uri)
        {
            return s_refIdValidator.IsMatch(uri.OriginalString);
        }

        /// <summary>
        /// Validates the reference identifier.
        /// </summary>
        public static bool ValidateRefId(Uri uri, ILogger logger)
        {
            if (!IsRefId(uri))
            {
                logger.OptiLog(LogLevel.Critical, "Invalid Definition RefId format.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the type of the definition.
        /// </summary>
        public static RefTypeEnum? GetDefinitionType(Uri refId)
        {
            if (s_stringToType.TryGetValue(refId.UserInfo, out var type))
                return type;

            return null;
        }

        /// <summary>
        /// Gets the type of the definition.
        /// </summary>
        public static string GetSimpleNameIdentification(Uri uri)
        {
            return uri.PathAndQuery.Trim('/');
        }

        /// <summary>
        /// Gets the type of the definition.
        /// </summary>
        public static string GetNamespaceIdentification(Uri uri)
        {
            return uri.Host;
        }

        /// <summary>
        /// Gets the method name of the definition.
        /// </summary>
        public static string GetMethodName(Uri uri)
        {
            return uri.Fragment.Trim('#');
        }

        /// <summary>
        /// Gets the type of the definition.
        /// </summary>
        public static void Explode(Uri refId, out RefTypeEnum type, out string @namespace, out string simpleNameIdentifier)
        {
            type = s_stringToType[refId.UserInfo];
            @namespace = refId.Host;
            simpleNameIdentifier = refId.PathAndQuery.Trim('/');
        }
    }
}
