#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using NUnit.Framework;

using Pomona.Common;
using Pomona.NHibernate3;
using Pomona.Profiling;
using Pomona.Security.Crypto;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class ExceptionTests
    {
        #region Setup/Teardown

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var pomonaExceptions = typeof(UnknownTypeException).Assembly
                .GetLoadableTypes()
                .Where(type => typeof(Exception).IsAssignableFrom(type));

            var pomonaCommonExceptions = typeof(PomonaSerializationException).Assembly
                .GetLoadableTypes()
                .Where(type => typeof(Exception).IsAssignableFrom(type));

            var pomonaNHibernate3Exceptions = typeof(NhQueryProviderCapabilityResolver).Assembly
                .GetLoadableTypes()
                .Where(type => typeof(Exception).IsAssignableFrom(type));

            var pomonaProfilingExceptions = typeof(ICryptoSerializer).Assembly
                .GetLoadableTypes()
                .Where(type => typeof(MiniProfilerWrapper).IsAssignableFrom(type));

            var pomonaSecurityExceptions = typeof(ICryptoSerializer).Assembly
                .GetLoadableTypes()
                .Where(type => typeof(Exception).IsAssignableFrom(type));

            this.exceptions = pomonaExceptions
                .Union(pomonaCommonExceptions)
                .Union(pomonaNHibernate3Exceptions)
                .Union(pomonaProfilingExceptions)
                .Union(pomonaSecurityExceptions)
                .ToArray();

            Assert.That(this.exceptions, Has.Length.GreaterThan(0));
        }

        #endregion

        private Type[] exceptions;


        [Test]
        public void AllExceptionsAreSerializable()
        {
            foreach (var exception in this.exceptions)
            {
                Assert.That(exception.HasAttribute<SerializableAttribute>(false),
                            "{0} is not properly decorated with [Serializable]",
                            exception);
            }
        }


        [Test]
        public void AllExceptionsShouldHaveProtectedSerializationConstructor()
        {
            foreach (var exception in this.exceptions)
            {
                var constructor = exception.GetConstructors(BindingFlags.NonPublic)
                    .FirstOrDefault(ctor => ctor.ParameterTypesMatch<SerializationInfo, StreamingContext>());

                Assert.That(constructor,
                            Is.Not.Null,
                            "The protected constructor {0}(SerializationInfo, StreamingContext) does not exist.",
                            exception);

                Assert.That(constructor.IsFamily,
                            "The constructor {0}(SerializationInfo, StreamingContext) is not protected.",
                            exception);
            }
        }
    }
}