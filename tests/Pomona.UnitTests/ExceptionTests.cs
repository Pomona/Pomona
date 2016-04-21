#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
        private const MethodAttributes methodAttributes = MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                          MethodAttributes.HideBySig
                                                          | MethodAttributes.Virtual | MethodAttributes.Public;

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
        public void AllExceptionsHaveProtectedSerializationConstructor()
        {
            foreach (var exception in this.exceptions)
            {
                var constructor = exception
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(ctor => ctor.ParameterTypesMatch<SerializationInfo, StreamingContext>());

                Assert.That(constructor,
                            Is.Not.Null,
                            "The constructor {0}(SerializationInfo, StreamingContext) does not exist.",
                            exception);

                Assert.That(constructor.IsFamily,
                            "The constructor {0}(SerializationInfo, StreamingContext) is not protected.",
                            exception);
            }
        }

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
                                                                                       .Where(
                                                                                           type => typeof(Exception).IsAssignableFrom(type));

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
    }
}