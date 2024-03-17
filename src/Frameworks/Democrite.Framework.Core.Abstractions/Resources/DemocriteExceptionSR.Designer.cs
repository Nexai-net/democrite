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
        ///   Looks up a localized string similar to Artifact execution failed (Uid: {0}) : {1}.
        /// </summary>
        internal static string ArtifactExecutionException {
            get {
                return ResourceManager.GetString("ArtifactExecutionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not found artificat Id({0}) type &apos;{1}&apos;..
        /// </summary>
        internal static string ArtifactMissing {
            get {
                return ResourceManager.GetString("ArtifactMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Artifact preparation failed - Due to {0}.
        /// </summary>
        internal static string ArtifactPreparationFailed {
            get {
                return ResourceManager.GetString("ArtifactPreparationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remote communication to artifact &quot;{0}&quot; failed due to : {1}.
        /// </summary>
        internal static string ArtifactRemoteException {
            get {
                return ResourceManager.GetString("ArtifactRemoteException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Artificat Id({0}) is of type &apos;{1}&apos; but expect type &apos;{2}&apos;..
        /// </summary>
        internal static string ArtifactWrongType {
            get {
                return ResourceManager.GetString("ArtifactWrongType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Conflict between multiple definitions type &apos;{0}&apos;, Source &apos;{1}&apos;, Conflict &apos;{2}&apos;.
        /// </summary>
        internal static string ConflictDefinitionException {
            get {
                return ResourceManager.GetString("ConflictDefinitionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; definition with id &apos;{1}&apos; fail due to : {2}.
        /// </summary>
        internal static string DefinitionExceptionMessage {
            get {
                return ResourceManager.GetString("DefinitionExceptionMessage", resourceCulture);
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
        ///   Looks up a localized string similar to Access blocked by security level {0}  Reason -&gt; {1}..
        /// </summary>
        internal static string DemocriteSecurityIssueExceptionMessage {
            get {
                return ResourceManager.GetString("DemocriteSecurityIssueExceptionMessage", resourceCulture);
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
        
        /// <summary>
        ///   Looks up a localized string similar to [Definition: {0} - {1}] failed due to \n {2}..
        /// </summary>
        internal static string InvalidDefinitionErrorMessages {
            get {
                return ResourceManager.GetString("InvalidDefinitionErrorMessages", resourceCulture);
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
        ///   Looks up a localized string similar to Invalid vgrain id get &apos;{0}&apos; expect : {1}..
        /// </summary>
        internal static string InvalidVGrainIdExceptionMessage {
            get {
                return ResourceManager.GetString("InvalidVGrainIdExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Etag {0} provided doesn&apos;t match the one in place &apos;{1}&apos;.
        /// </summary>
        internal static string MemoryRepositoryStorageEtagMismatchExceptionMessage {
            get {
                return ResourceManager.GetString("MemoryRepositoryStorageEtagMismatchExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Method with signature &quot;{0}&quot; doesn&apos;t have been founded on type &quot;{1}&quot;..
        /// </summary>
        internal static string MethodNotFound {
            get {
                return ResourceManager.GetString("MethodNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sequence definition id &apos;{0}&apos; is missing from the ISequenceDefinitionManager or parent definition.
        /// </summary>
        internal static string SequenceDefinitionMissing {
            get {
                return ResourceManager.GetString("SequenceDefinitionMissing", resourceCulture);
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
        ///   Looks up a localized string similar to Stage executor was not found to solved stage definition type &apos;{0}&apos;..
        /// </summary>
        internal static string StageExecutorNotFoundException {
            get {
                return ResourceManager.GetString("StageExecutorNotFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to VGrainType could not be generated on vgrain {0} and have no fallback values.
        /// </summary>
        internal static string VGrainIdGenerationExceptionMessage {
            get {
                return ResourceManager.GetString("VGrainIdGenerationExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to VGrain implementation of id &quot;{0}&quot; doesn&apos;t have been founded..
        /// </summary>
        internal static string VGrainIdNotFounded {
            get {
                return ResourceManager.GetString("VGrainIdNotFounded", resourceCulture);
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
        ///   Looks up a localized string similar to A valitor ({0}) on grain {1} reject the grain id provided  &apos;{2}&apos;.
        /// </summary>
        internal static string VGrainIdValidationFaildExceptionMessage {
            get {
                return ResourceManager.GetString("VGrainIdValidationFaildExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to VGrain implementation of type &quot;{0}&quot; doesn&apos;t have been founded..
        /// </summary>
        internal static string VGrainTypeNotFounded {
            get {
                return ResourceManager.GetString("VGrainTypeNotFounded", resourceCulture);
            }
        }
    }
}
