#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;
#if !DISABLE_PROXY_GENERATION
using System.Reflection.Emit;

#endif

namespace Pomona.Common.Proxies
{
    public static class EmitHelpers
    {
        public static Type CreatePocoType(string @namespace,
                                          string name,
                                          IEnumerable<KeyValuePair<string, Type>> properties)
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else
            var typeBuilder = CreateRuntimeTypeBuilder(@namespace, name);
            properties.ForEach(x =>
            {
                var propertyType = x.Value;
                var prop = typeBuilder.DefineProperty(x.Key, PropertyAttributes.None, x.Value, Type.EmptyTypes);
                var field = typeBuilder.DefineField("_" + x.Key.LowercaseFirstLetter(), x.Value, FieldAttributes.Private);
                var accessorMethodAttributes = MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                               MethodAttributes.HideBySig
                                               | MethodAttributes.Virtual | MethodAttributes.Public;
                var getter = typeBuilder.DefineMethod(
                    "get_" + name,
                    accessorMethodAttributes,
                    propertyType,
                    Type.EmptyTypes);

                var getIl = getter.GetILGenerator();
                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, field);
                getIl.Emit(OpCodes.Ret);

                prop.SetGetMethod(getter);

                var setter = typeBuilder.DefineMethod(
                    "set_" + name,
                    accessorMethodAttributes,
                    null,
                    new[] { propertyType });

                setter.DefineParameter(0, ParameterAttributes.None, "value");

                prop.SetSetMethod(setter);

                var setIl = setter.GetILGenerator();
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Ldarg_1);
                setIl.Emit(OpCodes.Stfld, field);
                setIl.Emit(OpCodes.Ret);
            });
            return typeBuilder.CreateType();
#endif
        }


#if !DISABLE_PROXY_GENERATION
        internal static ModuleBuilder CreateRuntimeModule(string assemblyNameString)
        {
            AssemblyBuilder asmBuilder;
            return CreateRuntimeModule(new AssemblyName(assemblyNameString), out asmBuilder);
        }


        internal static ModuleBuilder CreateRuntimeModule(AssemblyName assemblyName, out AssemblyBuilder asmBuilder)
        {
            // RunAndSave is not currently supported in .NET Core, and it's not on the roadmap.
            asmBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var modBuilder = asmBuilder.DefineDynamicModule(assemblyName.Name + ".dll");
            return modBuilder;
        }


        internal static TypeBuilder CreateRuntimeTypeBuilder(string @namespace,
                                                             string name,
                                                             TypeAttributes? typeAttributes = null,
                                                             Type baseType = null,
                                                             IEnumerable<Type> implementedInterfaces = null)
        {
            baseType = baseType ?? typeof(object);
            var typeBuilder = CreateRuntimeModule(@namespace).DefineType(@namespace + "." + name,
                                                                         typeAttributes ?? (TypeAttributes.Public),
                                                                         baseType ?? typeof(object),
                                                                         implementedInterfaces.EmptyIfNull().ToArray());

            var proxyBaseCtor =
                baseType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).First(
                    x => x.GetParameters().Length == 0);

            var ctor =
                typeBuilder.DefineConstructor(MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                                              MethodAttributes.RTSpecialName
                                              | MethodAttributes.Public,
                                              CallingConventions.Standard,
                                              Type.EmptyTypes);

            var ctorIlProcessor = ctor.GetILGenerator();
            ctorIlProcessor.Emit(OpCodes.Ldarg_0);
            ctorIlProcessor.Emit(OpCodes.Call, proxyBaseCtor);
            ctorIlProcessor.Emit(OpCodes.Ret);

            return typeBuilder;
        }
#endif
    }
}