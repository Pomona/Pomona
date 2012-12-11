using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

using Common.Logging;
using Common.Logging.Simple;

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

using PomonaNHibernateTest.Models;

namespace PomonaNHibernateTest
{
    internal class Program
    {
        private string DbFile = Path.GetTempFileName();
        private List<int> productIds = new List<int>();
        private Random rng;
        private ISession session;


        public void Run()
        {
            this.rng = new Random(376473672);
            var sessionFactory = CreateSessionFactory();
            // create properties
            var properties = new NameValueCollection();
            properties["level"] = "All";
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(properties);
            using (this.session = sessionFactory.OpenSession())
            {
                using (var trans = this.session.BeginTransaction())
                {
                    CreateProducts();
                    trans.Commit();
                }
            }

            var customerCount = 30;
            for (var i = 0; i < customerCount; i++)
            {
                using (this.session = sessionFactory.OpenSession())
                {
                    using (var trans = this.session.BeginTransaction())
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


        private void AddProduct(string name, string sku, decimal price)
        {
            var prod = new Product() { Name = name, Price = price, Sku = sku };
            this.session.Save(prod);
            this.productIds.Add(prod.Id);
        }


        private void AddRandomAttributesToOrder(PurchaseOrder order)
        {
            var attrKeys = new[] { "mega", "micro", "giga", "ultra" };
            var attrCount = this.rng.Next(0, 4);
            for (var i = 0; i < attrCount; i++)
            {
                var attrKey = attrKeys[this.rng.Next(0, attrKeys.Length)];
                if (order.Attributes.All(x => x.Key != attrKey))
                    order.Attributes.Add(
                        new EntityAttribute() { Key = attrKey, Value = this.rng.Next(0, 4444).ToString() });
            }
        }


        private void BuildSchema(Configuration config)
        {
            // delete the existing db on each run
            if (File.Exists(this.DbFile))
                File.Delete(this.DbFile);

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config)
                .Create(true, true);
        }


        private void CreateCustomerWithOrders()
        {
            var customer = new Customer() { Name = Words.GetAnimalWithPersonality(this.rng) };
            this.session.Save(customer);

            var orderCount = this.rng.Next(0, 9);

            for (var i = 0; i < orderCount; i++)
            {
                var itemCount = this.rng.Next(1, 5);
                var order = new PurchaseOrder() { Customer = customer, SomeGroup = this.rng.Next(0, 3) };
                Enumerable.Range(0, itemCount)
                    .Select(x => GetRandomProduct())
                    .Select(
                        x => new Item() { Price = x.Price, Product = x, Quantity = this.rng.Next(1, 6), Order = order })
                    .ToList()
                    .ForEach(item => order.Items.Add(item));

                AddRandomAttributesToOrder(order);

                this.session.Save(order);
            }
        }


        private void CreateProducts()
        {
            AddProduct("Coca Cola", "soda-coke", 10.5m);
            AddProduct("Solo", "soda-solo", 11.3m);
            AddProduct("Pizza Pepperoni", "pizza-pepperoni", 99.9m);
        }


        private ISessionFactory CreateSessionFactory()
        {
            var cfg = new TestAutomappingConfiguration();
            return Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .UsingFile(this.DbFile)
                        .ShowSql()
                )
                .Mappings(
                    m =>
                    m.AutoMappings.Add(
                        AutoMap.AssemblyOf<EntityBase>(cfg).Conventions.AddFromAssemblyOf<CascadeAllConvention>))
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }


        private Product GetRandomProduct()
        {
            return this.session.Load<Product>(this.productIds[this.rng.Next(0, this.productIds.Count())]);
        }
    }
}