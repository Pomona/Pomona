using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Common.Logging.Simple;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
using Pomona;
using Pomona.Common;
using Pomona.Internals;
using PomonaNHibernateTest.Models;
using LinqExtensionMethods = NHibernate.Linq.LinqExtensionMethods;

namespace PomonaNHibernateTest
{
    public class TestPomonaModule : PomonaModule
    {
        public TestPomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper) : base(dataSource, typeMapper)
        {
        }
    }

    public class TestPomonaDataSource : IPomonaDataSource, IDisposable
    {
        private static readonly MethodInfo queryMethod;
        private static readonly MethodInfo getEntityByIdMethod;
        private readonly ISession session;


        static TestPomonaDataSource()
        {
            getEntityByIdMethod =
                ReflectionHelper.GetGenericMethodDefinition<TestPomonaDataSource>(x => x.GetEntityById<EntityBase>(0));
            queryMethod = ReflectionHelper.GetGenericMethodDefinition<TestPomonaDataSource>(x => x.Query<object>(null));
        }

        public TestPomonaDataSource(ISessionFactory sessionFactory)
        {
            session = sessionFactory.OpenSession();
        }

        #region Implementation of IPomonaDataSource

        public T GetById<T>(object id)
        {
            return (T) getEntityByIdMethod.MakeGenericMethod(typeof (T)).Invoke(this, new object[] {Convert.ToInt32(id)});
        }


        public QueryResult Query(IPomonaQuery query)
        {
            return
                (QueryResult)
                queryMethod.MakeGenericMethod(query.TargetType.MappedTypeInstance).Invoke(this, new object[] {query});
        }


        public object Post<T>(T newObject)
        {
            throw new NotImplementedException();
        }

        public T GetEntityById<T>(int id)
            where T : EntityBase
        {
            return LinqExtensionMethods.Query<T>(session).First(x => x.Id == id);
        }

        private QueryResult Query<T>(IPomonaQuery query)
        {
            var pq = (PomonaQuery) query;
            return pq.ApplyAndExecute(LinqExtensionMethods.Query<T>(session));
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            session.Dispose();
        }

        #endregion
    }

    public class TestPomonaTypeMappingFilter : TypeMappingFilterBase
    {
        #region Overrides of TypeMappingFilterBase

        public override object GetIdFor(object entity)
        {
            return ((EntityBase) entity).Id;
        }


        public override IEnumerable<Type> GetSourceTypes()
        {
            return typeof (EntityBase).Assembly.GetTypes().Where(x => typeof (EntityBase).IsAssignableFrom(x));
        }

        #endregion
    }

    public class TestPomonaConfiguration : PomonaConfigurationBase
    {
        #region Overrides of PomonaConfigurationBase

        public override ITypeMappingFilter TypeMappingFilter
        {
            get { return new TestPomonaTypeMappingFilter(); }
        }

        public override IEnumerable<object> FluentRuleObjects
        {
            get { yield break; }
        }

        #endregion
    }

    public class TestPomonaBootstrapper : PomonaBootstrapper
    {
        private readonly ISessionFactory sessionFactory;
        private readonly TypeMapper typeMapper;

        public TestPomonaBootstrapper(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            typeMapper = new TypeMapper(new TestPomonaConfiguration());
        }


        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(sessionFactory);
            container.Register(typeMapper);
            container.Register<IPomonaDataSource, TestPomonaDataSource>();
        }
    }

    public class TestPomonaHost
    {
        private readonly Uri baseUri;
        private NancyHost host;
        private ISessionFactory sessionFactory;

        public TestPomonaHost(Uri baseUri, ISessionFactory sessionFactory)
        {
            this.baseUri = baseUri;
            this.sessionFactory = sessionFactory;
        }


        public Uri BaseUri
        {
            get { return baseUri; }
        }

        public NancyHost Host
        {
            get { return host; }
        }


        public void Start()
        {
            host = new NancyHost(baseUri, new TestPomonaBootstrapper(sessionFactory));
            host.Start();
        }


        public void Stop()
        {
            host.Stop();
            host = null;
        }
    }

    public class CascadeAll : IHasOneConvention, IHasManyConvention, IReferenceConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.Cascade.All();
        }

        public void Apply(IOneToOneInstance instance)
        {
            instance.Cascade.All();
        }

        public void Apply(IManyToOneInstance instance)
        {
            instance.Cascade.All();
        }
    }

    public class TestAutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            return typeof (EntityBase).IsAssignableFrom(type);
        }
    }

    internal class Program
    {
        private string DbFile = Path.GetTempFileName();
        private List<int> productIds = new List<int>();
        private Random rng;
        private ISession session;

        private ISessionFactory CreateSessionFactory()
        {
            var cfg = new TestAutomappingConfiguration();
            return Fluently.Configure()
                           .Database(
                               SQLiteConfiguration.Standard
                                                  .UsingFile(DbFile)
                                                  .ShowSql()
                )
                           .Mappings(m =>
                                     m.AutoMappings.Add(
                                         AutoMap.AssemblyOf<EntityBase>(cfg).Conventions.AddFromAssemblyOf<CascadeAll>))
                           .ExposeConfiguration(BuildSchema)
                           .BuildSessionFactory();
        }

        private void BuildSchema(Configuration config)
        {
            // delete the existing db on each run
            if (File.Exists(DbFile))
                File.Delete(DbFile);

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config)
                .Create(true, true);
        }

        public void Run()
        {
            rng = new Random(376473672);
            var sessionFactory = CreateSessionFactory();
            // create properties
            var properties = new NameValueCollection();
            properties["level"] = "All";
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(properties);
            using (session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    CreateProducts();
                    trans.Commit();
                }
            }

            var customerCount = 30;
            for (var i = 0; i < customerCount; i++)
            {
                using (session = sessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        CreateCustomerWithOrders();
                        trans.Commit();
                    }
                }
            }

            var pth = new TestPomonaHost(new Uri("http://localhost:4321/"), sessionFactory);
            pth.Start();

            Console.ReadLine();
            pth.Stop();
        }

        private static void Main(string[] args)
        {
            var program = new Program();
            program.Run();
        }

        private void CreateCustomerWithOrders()
        {
            var customer = new Customer() {Name = Words.GetAnimalWithPersonality(rng)};
            session.Save(customer);

            var orderCount = rng.Next(0, 9);

            for (var i = 0; i < orderCount; i++)
            {
                var itemCount = rng.Next(1, 5);
                var order = new PurchaseOrder() {Customer = customer, SomeGroup = rng.Next(0, 3)};
                Enumerable.Range(0, itemCount)
                          .Select(x => GetRandomProduct())
                          .Select(
                              x => new Item() {Price = x.Price, Product = x, Quantity = rng.Next(1, 6), Order = order})
                          .ToList()
                          .ForEach(item => order.Items.Add(item));
                session.Save(order);
            }
        }

        private Product GetRandomProduct()
        {
            return session.Load<Product>(productIds[rng.Next(0, productIds.Count())]);
        }

        private void AddProduct(string name, string sku, decimal price)
        {
            var prod = new Product() {Name = name, Price = price, Sku = sku};
            session.Save(prod);
            productIds.Add(prod.Id);
        }

        private void CreateProducts()
        {
            AddProduct("Coca Cola", "soda-coke", 10.5m);
            AddProduct("Solo", "soda-solo", 11.3m);
            AddProduct("Pizza Pepperoni", "pizza-pepperoni", 99.9m);
        }
    }
}