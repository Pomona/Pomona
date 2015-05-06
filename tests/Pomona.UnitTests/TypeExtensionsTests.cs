#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using Pomona.Common;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        private static MethodInfo GenericTestMethodInfo
        {
            get
            {
                var method = typeof(TypeExtensionsTests).GetMethod("GenericTestMethod");
                Assert.That(method, Is.Not.Null);
                Assert.That(method.IsGenericMethodDefinition, Is.True);
                return method;
            }
        }

        private static MethodInfo GenericTestMethodWithAdvancedConstraintInfo
        {
            get
            {
                var method = typeof(TypeExtensionsTests).GetMethod("GenericTestMethodWithAdvancedConstraint");
                Assert.That(method, Is.Not.Null);
                Assert.That(method.IsGenericMethodDefinition, Is.True);
                return method;
            }
        }


        /// <summary>
        /// A difficult generic GenericTestMethodInfo
        /// </summary>
        public void GenericTestMethod<T1, TStream, TStruct>(IEnumerable<T1> arg1,
                                                            TStream arg2,
                                                            string arg3,
                                                            Tuple<T1, Func<TStream, TStruct>> arg4,
                                                            TStruct arg5)
            where TStream : Stream, new()
            where TStruct : struct
        {
            throw new InvalidOperationException("Not to be run, only to be reflected upon.");
        }


        public void GenericTestMethodWithAdvancedConstraint<T1, TList>(T1 arg1, TList list)
            where TList : IList<T1>
        {
            throw new InvalidOperationException("Not to be run, only to be reflected upon.");
        }


        [Test]
        public void GetConstructor_ParameterCountMismatch_ThrowsMissingMethodException()
        {
            var type = typeof(TheInheritedClass);
            var exception =
                Assert.Throws<MissingMethodException>(
                    () => type.GetConstructor(typeof(string), typeof(int)));
            Assert.That(exception.Message,
                        Is.StringContaining("TheInheritedClass(System.String, System.Int32)"));
        }


        [Test]
        public void GetConstructor_ParameterTypesIsNotGiven_ThrowsArgumentNullException()
        {
            var type = GetType();
            var exception = Assert.Throws<ArgumentNullException>(() => type.GetConstructor());
            Assert.That(exception.ParamName, Is.EqualTo("parameterTypes"));
        }


        [Test]
        public void GetConstructor_ParameterTypesIsNull_ThrowsArgumentNullException()
        {
            var type = GetType();
            var exception = Assert.Throws<ArgumentNullException>(() => TypeExtensions.GetConstructor(type, null));
            Assert.That(exception.ParamName, Is.EqualTo("parameterTypes"));
        }


        [Test]
        public void GetConstructor_ReturnsConstructor()
        {
            var type = typeof(TheInheritedClass);
            var method = type.GetConstructor(typeof(string), typeof(int), typeof(decimal));
            Assert.That(method, Is.Not.Null);
        }


        [Test]
        public void GetConstructor_TypeIsNull_ThrowsArgumentNullException()
        {
            Type type = null;
            var exception = Assert.Throws<ArgumentNullException>(() => type.GetConstructor());
            Assert.That(exception.ParamName, Is.EqualTo("type"));
        }


        [Test]
        public void GetFullNameWithSignature_MethodHasComplexArgumentsAndGenericParameters_ReturnsExpectedString()
        {
            var result = typeof(Queryable).GetMethods().Last(x => x.Name == "OrderBy").GetFullNameWithSignature();
            Console.WriteLine(result);

            Assert.That(result, Is.EqualTo(
                "System.Linq.IOrderedQueryable<TSource> System.Linq.Queryable.OrderBy<TSource, TKey>(System.Linq.IQueryable<TSource>, System.Linq.Expressions.Expression<System.Func<TSource, TKey>>, System.Collections.Generic.IComparer<TKey>)"));
        }


        [Test]
        public void GetFullNameWithSignature_MethodHasGenericArgumentsAndGenericParameters_ReturnsExpectedString()
        {
            var result = typeof(TheInheritedClass).GetMethod("GenericMethodWithParameters").GetFullNameWithSignature();

            Assert.That(result, Is.EqualTo(
                "System.Void Pomona.UnitTests.TypeExtensionsTests+TheInheritedClass.GenericMethodWithParameters<T1, T2, T3>(T1, T2, T3)"));
        }


        [Test]
        public void GetFullNameWithSignature_MethodHasGenericArgumentsAndParameters_ReturnsExpectedString()
        {
            var result = typeof(TheInheritedClass)
                .GetGenericInstanceMethod("ProtectedVirtualMethod", 3)
                .GetFullNameWithSignature();

            Assert.That(result, Is.EqualTo(
                "System.Void Pomona.UnitTests.TypeExtensionsTests+TheInheritedClass.ProtectedVirtualMethod<T1, T2, T3>(System.String, System.Int32)"));
        }


        [Test]
        public void GetFullNameWithSignature_MethodHasGenericArgumentsButNoParameters_ReturnsExpectedString()
        {
            var result = typeof(TheInheritedClass).GetMethod("GenericMethod").GetFullNameWithSignature();

            Assert.That(result, Is.EqualTo(
                "System.Void Pomona.UnitTests.TypeExtensionsTests+TheInheritedClass.GenericMethod<T1, T2, T3>()"));
        }


        [Test]
        public void GetFullNameWithSignature_MethodHasParameters_ReturnsExpectedString()
        {
            var result = typeof(TheInheritedClass).GetMethod("PlainMethodWithParameters").GetFullNameWithSignature();

            Assert.That(result, Is.EqualTo(
                "System.Void Pomona.UnitTests.TypeExtensionsTests+TheInheritedClass.PlainMethodWithParameters(System.String, System.Int32)"));
        }


        [Test]
        public void GetFullNameWithSignature_MethodIsNull_ThrowsArgumentNullException()
        {
            MethodInfo method = null;
            var exception = Assert.Throws<ArgumentNullException>(() => method.GetFullNameWithSignature());
            Assert.That(exception.ParamName, Is.EqualTo("method"));
        }


        [Test]
        public void GetFullNameWithSignature_MethodIsPlain_ReturnsExpectedString()
        {
            var result = typeof(TheInheritedClass).GetMethod("PlainMethod").GetFullNameWithSignature();

            Assert.That(result, Is.EqualTo(
                "System.Void Pomona.UnitTests.TypeExtensionsTests+TheInheritedClass.PlainMethod()"));
        }


        [Test]
        public void GetGenericInstanceMethod_ArgumentCountMismatch_ThrowsMissingMethodException()
        {
            var type = typeof(TheInheritedClass);
            var exception =
                Assert.Throws<MissingMethodException>(
                    () => type.GetGenericInstanceMethod("GenericMethod", typeof(string), typeof(int)));
            Assert.That(exception.Message,
                        Is.StringContaining("TheInheritedClass.GenericMethod<System.String, System.Int32>()"));
        }


        [Test]
        public void GetGenericInstanceMethod_GenericArgumentTypesIsEmpty_ThrowsArgumentNullException()
        {
            var type = GetType();
            var exception =
                Assert.Throws<ArgumentNullException>(() => type.GetGenericInstanceMethod("Method", new Type[0]));
            Assert.That(exception.ParamName, Is.EqualTo("genericArgumentTypes"));
        }


        [Test]
        public void GetGenericInstanceMethod_GenericArgumentTypesIsNull_ThrowsArgumentNullException()
        {
            var type = GetType();
            var exception = Assert.Throws<ArgumentNullException>(() => type.GetGenericInstanceMethod("Method"));
            Assert.That(exception.ParamName, Is.EqualTo("genericArgumentTypes"));
        }


        [Test]
        public void GetGenericInstanceMethod_MethodNameDoesNotExist_ThrowsMissingMethodException()
        {
            var type = typeof(TheInheritedClass);
            var exception =
                Assert.Throws<MissingMethodException>(
                    () => type.GetGenericInstanceMethod("NonExistingMethod", typeof(string)));
            Assert.That(exception.Message, Is.StringContaining("TheInheritedClass.NonExistingMethod"));
        }


        [Test]
        public void GetGenericInstanceMethod_MethodNameIsNull_ThrowsArgumentNullException()
        {
            var type = GetType();
            var exception = Assert.Throws<ArgumentNullException>(() => type.GetGenericInstanceMethod(null));
            Assert.That(exception.ParamName, Is.EqualTo("methodName"));
        }


        [Test]
        public void GetGenericInstanceMethod_ProtectedVirtualMethod_ReturnsMethod()
        {
            var type = typeof(TheInheritedClass);
            var method = type.GetGenericInstanceMethod("ProtectedVirtualMethod", typeof(string), typeof(int),
                                                       typeof(decimal));
            Assert.That(method, Is.Not.Null);
        }


        [Test]
        public void GetGenericInstanceMethod_PublicMethod_ReturnsMethod()
        {
            var type = typeof(TheInheritedClass);
            var method = type.GetGenericInstanceMethod("GenericMethod", typeof(string), typeof(int), typeof(decimal));
            Assert.That(method, Is.Not.Null);
        }


        [Test]
        public void GetGenericInstanceMethod_TypeIsNull_ThrowsArgumentNullException()
        {
            Type type = null;
            var exception = Assert.Throws<ArgumentNullException>(() => type.GetGenericInstanceMethod(""));
            Assert.That(exception.ParamName, Is.EqualTo("type"));
        }


        public void NonGenericTestMethod(int a, Stream s)
        {
            throw new InvalidOperationException("Not to be run, only to be reflected upon.");
        }


        [Test]
        public void ParameterTypesMatch_Matching_ReturnsTrue()
        {
            var parameterTypesMatch = typeof(TheInheritedClass)
                .GetMethod("PlainMethodWithParameters")
                .ParameterTypesMatch<string, int>();

            Assert.That(parameterTypesMatch, Is.True);
        }


        [Test]
        public void ParameterTypesMatch_NotMatching_ReturnsFalse()
        {
            var parameterTypesMatch = typeof(TheInheritedClass)
                .GetMethod("PlainMethodWithParameters")
                .ParameterTypesMatch<Guid, DateTime>();

            Assert.That(parameterTypesMatch, Is.False);
        }


        [Test]
        public void TryExtractTypeArguments_WhenBaseClassOfTypeImplementsGenericInterface_IsSuccessful()
        {
            Type[] typeArgs;
            Assert.That(typeof(TheInheritedClass).TryExtractTypeArguments(typeof(ITheInterface<,>), out typeArgs),
                        Is.True);
            Assert.That(typeArgs.Length, Is.EqualTo(2));
            Assert.That(typeArgs[0], Is.EqualTo(typeof(int)));
            Assert.That(typeArgs[1], Is.EqualTo(typeof(string)));
        }


        [Test]
        public void TryExtractTypeArguments_WhenTypeImplementsGenericInterfaceDirectly_IsSuccessful()
        {
            Type[] typeArgs;
            Assert.That(typeof(IEnumerable<int>).TryExtractTypeArguments(typeof(IEnumerable<>), out typeArgs), Is.True);
            Assert.That(typeArgs.Length, Is.EqualTo(1));
            Assert.That(typeArgs[0], Is.EqualTo(typeof(int)));
        }


        [Test]
        public void TryExtractTypeArguments_WhenTypeImplementsGenericInterfaceInherited_IsSuccessful()
        {
            Type[] typeArgs;
            Assert.That(typeof(IGrouping<string, int>).TryExtractTypeArguments(typeof(IEnumerable<>), out typeArgs),
                        Is.True);
            Assert.That(typeArgs.Length, Is.EqualTo(1));
            Assert.That(typeArgs[0], Is.EqualTo(typeof(int)));
        }


        [Test]
        public void TryExtractTypeArguments_WhenTypeInheritsGenericTypeDefinition_IsSuccessful()
        {
            Type[] typeArgs;
            Assert.That(typeof(TheInheritedClass).TryExtractTypeArguments(typeof(TheBaseClass<>), out typeArgs),
                        Is.True);
            Assert.That(typeArgs.Length, Is.EqualTo(1));
            Assert.That(typeArgs[0], Is.EqualTo(typeof(double)));
        }


        [Test]
        public void TryResolveGenericMethod_WithArgsMatchingConstraintHavingGenericParameters_ResolvesMethod()
        {
            // Should make GenericTestMethodInfo instance with <int,MemoryStream,Guid>

            var testMethod = GenericTestMethodWithAdvancedConstraintInfo;

            var typeArgs = new[] { typeof(int), typeof(IList<int>) };
            MethodInfo methodInstance;
            Assert.That(testMethod.TryResolveGenericMethod(typeArgs, out methodInstance), Is.EqualTo(true));
            Assert.That(methodInstance.GetGenericArguments(), Is.EquivalentTo(new[] { typeof(int), typeof(IList<int>) }));
        }


        [Test]
        public void TryResolveGenericMethod_WithArgumentsNotMatchingNewConstraint_ReturnsFalse()
        {
            // type AppDomain does not satisfy TStruct: struct
            var typeArgs = new[]
            {
                typeof(IEnumerable<int>), typeof(Stream), typeof(string), typeof(Tuple<int, Func<Stream, bool>>)
                ,
                typeof(bool)
            };

            MethodInfo methodInstance;
            Assert.That(GenericTestMethodInfo.TryResolveGenericMethod(typeArgs, out methodInstance), Is.False);
        }


        [Test]
        public void TryResolveGenericMethod_WithArgumentsNotMatchingStructConstraint_ReturnsFalse()
        {
            // type AppDomain does not satisfy TStruct: struct
            var typeArgs = new[]
            {
                typeof(IEnumerable<int>), typeof(MemoryStream), typeof(string),
                typeof(Tuple<int, Func<MemoryStream, bool?>>), typeof(bool?)
            };

            MethodInfo methodInstance;
            Assert.That(GenericTestMethodInfo.TryResolveGenericMethod(typeArgs, out methodInstance), Is.False);
        }


        [Test]
        public void TryResolveGenericMethod_WithArgumentsNotMatchingTypeConstraint_ReturnsFalse()
        {
            var typeArgs = new[]
            {
                typeof(IEnumerable<int>), typeof(bool), typeof(string), typeof(Tuple<int, Func<bool, Guid>>),
                typeof(Guid)
            };

            MethodInfo methodInstance;
            Assert.That(GenericTestMethodInfo.TryResolveGenericMethod(typeArgs, out methodInstance), Is.False);
        }


        [Test]
        public void TryResolveGenericMethod_WithNonGenericMethodAndMatchingArguments_ResolvesMethod()
        {
            var method = typeof(TypeExtensionsTests).GetMethod("NonGenericTestMethod");
            Assert.That(method, Is.Not.Null);
            Assert.That(method.IsGenericMethodDefinition, Is.False);
            var typeArgs = new[] { typeof(int), typeof(MemoryStream) };
            MethodInfo methodInstance;
            Assert.That(method.TryResolveGenericMethod(typeArgs, out methodInstance), Is.True);
            Assert.That(method, Is.EqualTo(methodInstance));
        }


        [Test]
        public void TryResolveGenericMethod_WithNonGenericMethodAndWrongArguments_ReturnsFalse()
        {
            var method = typeof(TypeExtensionsTests).GetMethod("NonGenericTestMethod");
            Assert.That(method, Is.Not.Null);
            Assert.That(method.IsGenericMethodDefinition, Is.False);
            var typeArgs = new[] { typeof(bool), typeof(MemoryStream) };
            MethodInfo methodInstance;
            Assert.That(method.TryResolveGenericMethod(typeArgs, out methodInstance), Is.False);
        }


        [Test]
        public void TryResolveGenericMethod_WithPerfectlyMatchingArguments_ResolvesMethod()
        {
            // Should make GenericTestMethodInfo instance with <int,MemoryStream,Guid>
            var typeArgs = new[]
            {
                typeof(IEnumerable<int>), typeof(MemoryStream), typeof(string),
                typeof(Tuple<int, Func<MemoryStream, Guid>>), typeof(Guid)
            };

            MethodInfo methodInstance;
            Assert.That(GenericTestMethodInfo.TryResolveGenericMethod(typeArgs, out methodInstance), Is.EqualTo(true));
            Assert.That(methodInstance.GetGenericArguments(),
                        Is.EquivalentTo(new[] { typeof(int), typeof(MemoryStream), typeof(Guid) }));
        }


        [Test]
        public void TryResolveGenericMethod_WithSomeArgumentsInherited_ResolvesMethod()
        {
            var typeArgs = new[]
            {
                typeof(IGrouping<Uri, int>), typeof(MemoryStream), typeof(string),
                typeof(Tuple<int, Func<MemoryStream, Guid>>), typeof(Guid)
            };

            MethodInfo methodInstance;
            Assert.That(GenericTestMethodInfo.TryResolveGenericMethod(typeArgs, out methodInstance), Is.EqualTo(true));
            Assert.That(methodInstance.GetGenericArguments(),
                        Is.EquivalentTo(new[] { typeof(int), typeof(MemoryStream), typeof(Guid) }));
        }


        public interface ITheInterface<T1, T2>
        {
        }

        public class TheBaseClass<T3> : ITheInterface<int, string>
        {
        }

        public class TheInheritedClass : TheBaseClass<double>
        {
            public TheInheritedClass(string s, int i, decimal d)
            {
            }


            public void GenericMethod<T1, T2, T3>()
            {
            }


            public void GenericMethodWithParameters<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
            {
            }


            public void PlainMethod()
            {
            }


            public void PlainMethodWithParameters(string s, int i)
            {
            }


            protected virtual void ProtectedVirtualMethod<T1, T2, T3>(string s, int i)
            {
            }
        }
    }
}