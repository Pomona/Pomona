#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Nancy.TinyIoc;

using NUnit.Framework;

namespace Pomona.UnitTests.Ioc
{
    [TestFixture]
    public class TinyIocTests : IocTestsBase<TinyIoCContainer>
    {
        [Test]
        public void Constructor_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => CreateWrapper());
        }


        [Test]
        public void GetInstance_ByType_ReturnsInstance()
        {
            Assert.That(CreateWrapper().GetInstance(typeof(IDummyContract)),
                        Is.TypeOf<DummyImplementation>());
        }


        protected override void Register<TService, TImplementation>()
        {
            Container.Register<TService, TImplementation>();
        }
    }
}