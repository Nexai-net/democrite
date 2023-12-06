﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Democrite.Framework.Core.Abstractions.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DemocriteExceptionSR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DemocriteExceptionSR() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Democrite.Framework.Core.Abstractions.Resources.DemocriteExceptionSR", typeof(DemocriteExceptionSR).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to VGrainCategories could not be generated on vgrain {0} and have no fallback values.
        /// </summary>
        internal static string VGrainIdGenerationExceptionMessage {
            get {
                return ResourceManager.GetString("VGrainIdGenerationExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attribute &quot;ExpectedVGrainIdFormatAttribute&quot; is missing on type &apos;{0}&apos; (this attribute is not inherited). This attribute is mandatory to generate vgrain id..
        /// </summary>
        internal static string VGrainIdTemplateMissingExceptionMessage {
            get {
                return ResourceManager.GetString("VGrainIdTemplateMissingExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; definition with id &apos;{1}&apos; is missing..
        /// </summary>
        internal static string DefinitionMissingExceptionMessage {
            get {
                return ResourceManager.GetString("DefinitionMissingExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid vgrain id get &apos;{0}&apos; expect : {1]..
        /// </summary>
        internal static string InvalidVGrainIdExceptionMessage {
            get {
                return ResourceManager.GetString("InvalidVGrainIdExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property &apos;{0}&apos; of the definition &apos;{1}&apos; expose value &apos;{2}&apos; and expect : {3}.
        /// </summary>
        internal static string InvalidDefinitionPropertyValueExceptionMessage {
            get {
                return ResourceManager.GetString("InvalidDefinitionPropertyValueExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input type ({0}) provided is not allowed by the execution process &apos;{1}&apos;. (Expect. {2})..
        /// </summary>
        internal static string InvalidInputDemocriteExceptionMessage {
            get {
                return ResourceManager.GetString("InvalidInputDemocriteExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output type ({0}) desired is not compatible with the execution process &apos;{1}&apos; declaration &apos;{2}&apos;..
        /// </summary>
        internal static string InvalidOutputDemocriteExceptionMessage {
            get {
                return ResourceManager.GetString("InvalidOutputDemocriteExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signal named &quot;{0}&quot;has not be founded on different definition provider sources..
        /// </summary>
        internal static string SignalNotFounded {
            get {
                return ResourceManager.GetString("SignalNotFounded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Door information &quot;{0}&quot;has not be founded on different definition provider sources..
        /// </summary>
        internal static string DoorNotFounded {
            get {
                return ResourceManager.GetString("DoorNotFounded", resourceCulture);
            }
        }
    }
}
