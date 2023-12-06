// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System.ComponentModel;
    using System.Text;

    /// <summary>
    /// Error code democrite managment
    /// </summary>
    public static class DemocriteErrorCodes
    {
        public const string KEY = "ErrorCode";

        /// <summary>
        /// Mask used to extract the error code category
        /// </summary>
        public const ulong CategoryMask = 0x_FF_00_00_00_00_00_00_00;

        /// <summary>
        /// Mask used to extract the error code macro type
        /// </summary>
        public const ulong MacroTypeMask = 0x_00_FF_00_00_00_00_00_00;

        /// <summary>
        /// Mask used to extract the generic error 
        /// </summary>
        public const ulong GenericMask = 0x_00_00_FF_00_00_00_00_00;

        /// <summary>
        /// Mask used to extract the custom error code
        /// </summary>
        public const ulong CustomErrorCodeMask = 0x_00_00_00_00_FF_FF_FF_FF;

        public enum Categories : ulong
        {
            None = 0,

            /// <summary>
            /// Error code category sequence
            /// </summary>
            [Description("Error occured during the sequence execution")]
            Sequence = 0x_01_00_00_00_00_00_00_00,

            /// <summary>
            /// Error code category security
            /// </summary>
            [Description("Error code category security")]
            Security = 0x_02_00_00_00_00_00_00_00,

            /// <summary>
            /// Error code category artifact
            /// </summary>
            [Description("Error code category security")]
            Artifact = 0x_03_00_00_00_00_00_00_00,

            /// <summary>
            /// Error code category vgrain
            /// </summary>
            [Description("Error code category vgrain")]
            VGrain = 0x_04_00_00_00_00_00_00_00,

            /// <summary>
            /// Error code category definition
            /// </summary>
            [Description("Error code category definition")]
            Definition = 0x_05_00_00_00_00_00_00_00,

            /// <summary>
            /// Error code category definition
            /// </summary>
            [Description("Error code category signal")]
            Signal = 0x_06_00_00_00_00_00_00_00,

            /// <summary>
            /// Error code category build
            /// </summary>
            [Description("Error code category build")]
            Build = 0x_07_00_00_00_00_00_00_00,

            /// <summary>
            /// Error code category door
            /// </summary>
            [Description("Error code category door")]
            Door = 0x_08_00_00_00_00_00_00_00,
        }

        public enum PartType : ulong
        {
            None = 0,

            /// <summary>
            /// Error about invalid cast or invalid type provided
            /// </summary>
            [Description("Error about invalid cast or invalid type provided")]
            Type = 0x_00_01_00_00_00_00_00_00,

            /// <summary>
            /// Error about reflection
            /// </summary>
            [Description("Error about reflection")]
            Reflection = 0x_00_02_00_00_00_00_00_00,

            /// <summary>
            /// Error about a setup phase
            /// </summary>
            [Description("Error about reflection")]
            Setup = 0x_00_03_00_00_00_00_00_00,

            /// <summary>
            /// Error about a setup meta information
            /// </summary>
            [Description("Error about meta information")]
            MetaInformation = 0x_00_04_00_00_00_00_00_00,

            /// <summary>
            /// Error about a setup execution
            /// </summary>
            [Description("Error about execution")]
            Execution = 0x_00_05_00_00_00_00_00_00,

            /// <summary>
            /// Error about a definition
            /// </summary>
            [Description("Error about definition")]
            Definition = 0x_00_06_00_00_00_00_00_00,

            /// <summary>
            /// Error about a identifier
            /// </summary>
            [Description("Error about identifier")]
            Identifier = 0x_00_07_00_00_00_00_00_00,

            /// <summary>
            /// Error about a Property
            /// </summary>
            [Description("Error about Property")]
            Property = 0x_00_08_00_00_00_00_00_00,

            /// <summary>
            /// Error about input information
            /// </summary>
            [Description("Error about input information.")]
            Input = 0x_00_00_09_00_00_00_00_00,

            /// <summary>
            /// Error about output information
            /// </summary>
            [Description("Error about input information.")]
            Output = 0x_00_00_0A_00_00_00_00_00,

            /// <summary>
            /// Error about Configuration information
            /// </summary>
            [Description("Error about input information.")]
            Configuration = 0x_00_00_0B_00_00_00_00_00,
        }

        public enum ErrorType : ulong
        {
            None = 0,

            /// <summary>
            /// Error about action that Not Founde
            /// </summary>
            [Description("Error When data or action is invalid.")]
            Invalid = 0x_00_00_01_00_00_00_00_00,

            /// <summary>
            /// Error about missing information
            /// </summary>
            [Description("Error about missing information.")]
            Missing = 0x_00_00_02_00_00_00_00_00,

            /// <summary>
            /// Error about action that Not Founde
            /// </summary>
            [Description("Error When resources doesn't have been founded.")]
            NotFounded = 0x_00_00_03_00_00_00_00_00,

            /// <summary>
            /// Error about action that failed
            /// </summary>
            [Description("Error about action that failed.")]
            Failed = 0x_00_00_FF_00_00_00_00_00,

        }

        /// <summary>
        /// Builds the specified category.
        /// </summary>
        public static ulong Build(Categories category,
                                  PartType macro = PartType.None,
                                  ErrorType genericType = ErrorType.None,
#pragma warning disable IDE0049 // Simplify Names
                                  Int32? customErrorCode = 0)
#pragma warning restore IDE0049 // Simplify Names
        {
            return (ulong)category &
                   (ulong)macro &
                   (ulong)genericType &
                   (ulong)(customErrorCode ?? 0);
        }

        /// <summary>
        /// Decrypt <paramref name="errorCode"/> to reable string.
        /// </summary>
        public static string ToDecryptErrorCode(this in ulong errorCode)
        {
            var str = new StringBuilder();

            var category = errorCode & CategoryMask;
            var macroType = errorCode & MacroTypeMask;
            var genericType = errorCode & GenericMask;

            str.Append(nameof(Categories));
            str.Append(": ");

            var categoryStr = Enum.GetName<Categories>((Categories)category);
            str.Append(categoryStr);

            str.Append(" - ");

            str.Append(nameof(PartType));
            str.Append(": ");

            var macroTypeStr = Enum.GetName<PartType>((PartType)macroType);
            str.Append(macroTypeStr);

            str.Append(" - ");

            str.Append(nameof(ErrorType));
            str.Append(": ");

            var genericStr = Enum.GetName<ErrorType>((ErrorType)genericType);
            str.Append(genericStr);

            str.Append(" - ");

            str.Append("Custom Code: ");
            str.Append(errorCode & CustomErrorCodeMask);

            return str.ToString();
        }
    }
}
