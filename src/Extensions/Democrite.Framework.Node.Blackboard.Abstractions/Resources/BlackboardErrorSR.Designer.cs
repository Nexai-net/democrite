﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Democrite.Framework.Node.Blackboard.Abstractions.Resources {
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
    internal class BlackboardErrorSR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal BlackboardErrorSR() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Democrite.Framework.Node.Blackboard.Abstractions.Resources.BlackboardErrorSR", typeof(BlackboardErrorSR).Assembly);
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
        ///   Looks up a localized string similar to Executing command ({1}) failed : {0}.
        /// </summary>
        internal static string BlackboardCommandExecutionException {
            get {
                return ResourceManager.GetString("BlackboardCommandExecutionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The blackboard with id {0} doesn&apos;t exists..
        /// </summary>
        internal static string BlackboardMissingDemocriteException {
            get {
                return ResourceManager.GetString("BlackboardMissingDemocriteException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple data entry conflict in the blackboard reason : {0} \n Existing : {1} \nVS\n NewValue: {2}.
        /// </summary>
        internal static string BlackboardPushConflictException {
            get {
                return ResourceManager.GetString("BlackboardPushConflictException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pushing data record failed {0} \n Input: {1}.
        /// </summary>
        internal static string BlackboardPushException {
            get {
                return ResourceManager.GetString("BlackboardPushException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New data doesn&apos;t match the required {0} \n Input: {1}.
        /// </summary>
        internal static string BlackboardPushValidationException {
            get {
                return ResourceManager.GetString("BlackboardPushValidationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Query ({0}) have been rejected : {1}.
        /// </summary>
        internal static string BlackboardQueryRejectedException {
            get {
                return ResourceManager.GetString("BlackboardQueryRejectedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string String1 {
            get {
                return ResourceManager.GetString("String1", resourceCulture);
            }
        }
    }
}
