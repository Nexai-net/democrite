// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Security
{
    using Democrite.Framework.Toolbox.Disposables;

    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    /*
     * 
     * The unfinished idea is to be able to create a proxy around service
     * An external declare to be the owner (or attribute rule ..) 
     * 
     * An before any call relayed a security token is check
     * 
     */

    internal abstract class SecureDynamicProxy<TType>
    {
        private readonly TType? _container;

        protected SecureDynamicProxy(TType container)
        {
            this._container = container;
        }

        protected IDisposable EnsureCallAllowed(MethodInfo callInfo)
        {
            return SafeDisposable.Empty;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    public sealed class SecureProxy
    {
        public static TInterface CreateAround<TInterface, TType>(TType test)
            where TType : class, TInterface
        {
            var interfaceTraits = typeof(TInterface);
            var objectTraits = typeof(TType);

            var assembly = objectTraits.Assembly;
            var nsName = new AssemblyName(assembly.GetName().Name + ".securityAutoGen");

            var module = AssemblyBuilder.DefineDynamicAssembly(nsName, AssemblyBuilderAccess.Run).DefineDynamicModule(nsName.Name!);

            var proxyType = module.DefineType(nameof(SecureProxy) + interfaceTraits.Name,
                                              TypeAttributes.Sealed | TypeAttributes.Public,
                                              typeof(SecureDynamicProxy<TType>),
                                              new Type[] { interfaceTraits });

            var baseCtor = typeof(SecureDynamicProxy<TType>).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();

            var ctorBuilder = proxyType.DefineConstructor(MethodAttributes.Public, CallingConventions.Any, new[] { objectTraits });
            var ilGenerator = ctorBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, baseCtor);

            foreach (var prop in interfaceTraits.GetProperties())
            {
                var newProp = proxyType.DefineProperty(prop.Name,
                                                       PropertyAttributes.HasDefault,
                                                       prop.PropertyType,
                                                       Type.EmptyTypes);

                var getMthd = proxyType.DefineMethod("get_" + prop.Name,
                                                     MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                                     prop.PropertyType,
                                                     Type.EmptyTypes);

                var il = getMthd.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, "Test str");
                il.Emit(OpCodes.Ret);

                newProp.SetGetMethod(getMthd);
            }

            var finalType = proxyType.CreateType();
            return (TInterface)Activator.CreateInstance(finalType, new object[] { test })!;
        }
    }
}
