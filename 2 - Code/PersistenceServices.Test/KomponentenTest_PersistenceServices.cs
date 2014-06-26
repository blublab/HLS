// <copyright file="KomponentenTest_PersistenceServices.cs" company="Stefan Sarstedt">
// Copyright (c) 2013 Stefan Sarstedt
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using FluentNHibernate.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;

namespace Tests.KomponentenTest.PersistenceService
{
    public class NHibernate_HiLo_Values
    {
        public virtual string TableName { get; set; }
        public virtual int NextHi { get; set; }
    }

    public class Address
    {
        public virtual int Id { get; set; }
        public virtual string Street { get; set; }
        public virtual string Zip { get; set; }
        public virtual string City { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual int Version { get; set; }
    }

    public class Customer
    {
        private IList<Order> orders = new List<Order>();
        private Address address = null;
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Version { get; set; }

        public Customer()
        {
        }

        public virtual ReadOnlyCollection<Order> Orders
        {
            get
            {
                return new ReadOnlyCollection<Order>(orders);
            }
        }

        /// <summary>
        /// Adds an order to the customer
        /// </summary>
        /// <param name="order">order to add</param>
        public virtual void AddOrder(Order order)
        {
            orders.Add(order);
            order.Customer = this;
        }

        public virtual Address Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }
    }

    public class Order
    {
        private IList<Product> products = new List<Product>();
        public virtual int Id { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual int Version { get; set; }

        public Order()
        {
        }

        public virtual ReadOnlyCollection<Product> Products
        {
            get
            {
                return new ReadOnlyCollection<Product>(products);
            }
        }

        public virtual void AddProduct(Product product)
        {
            products.Add(product);

            ICollection<Order> collectionObject = (ICollection<Order>)typeof(Product)
                .GetField("orders", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(product);
            collectionObject.Add(this);
        }
    }

    public class Product
    {
        private IList<Order> orders = new List<Order>();
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Version { get; set; }

        public Product()
        {
        }

        public virtual ReadOnlyCollection<Order> Orders
        {
            get
            {
                return new ReadOnlyCollection<Order>(orders);
            }
        }

        public virtual void AddOrder(Order order)
        {
            orders.Add(order);

            ICollection<Product> collectionObject = (ICollection<Product>)typeof(Order)
                .GetField("products", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(order);
            collectionObject.Add(this);
        }
    }

    public class NHibernate_HiLo_ValuesMap : ClassMap<NHibernate_HiLo_Values>
    {
        public NHibernate_HiLo_ValuesMap()
        {
            Id(x => x.TableName);
            Map(x => x.NextHi);
        }
    }

    public class AddressMap : ClassMap<Address>
    {
        public AddressMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("NHibernate_HiLo_Values", "NextHi", "100", "TableName='Address'");
            Map(x => x.Street);
            Map(x => x.Zip);
            Map(x => x.City);
            References(x => x.Customer).ForeignKey("none"); // 1:1
            Version(x => x.Version);
        }
    }

    public class CustomerMap : ClassMap<Customer>
    {
        public CustomerMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("NHibernate_HiLo_Values", "NextHi", "100", "TableName='Customer'");
            Map(x => x.Name);
            References(x => x.Address)
                .Cascade.All(); // 1:1, cascade speichert beim Speichern des Customer auch dessen Adresse
            HasMany(x => x.Orders)
                .Access.LowerCaseField()
                .LazyLoad()
                .Cascade.AllDeleteOrphan(); // 1:n  Inverse nicht nötig bei 1:n
            Version(x => x.Version);
        }
    }

    public class OrderMap : ClassMap<Order>
    {
        public OrderMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("NHibernate_HiLo_Values", "NextHi", "100", "TableName='Order'");
            References(x => x.Customer); // 1:1
            HasManyToMany(x => x.Products)
                .Access.LowerCaseField()
                .LazyLoad().Cascade.All(); // n:m
            Version(x => x.Version);
        }
    }

    public class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("NHibernate_HiLo_Values", "NextHi", "100", "TableName='Product'");
            Map(x => x.Name);
            HasManyToMany(x => x.Orders)
                .Access.LowerCaseField()
                .LazyLoad().Inverse(); // n:m Inverse nötig, da ansonsten doppelte Einträge in Zwischentabelle
            Version(x => x.Version);
        }
    }

    /// <summary>
    /// Komponententest für die PersistenceServices
    /// </summary>
    [TestClass]
    public class KomponentenTest_PersistenceServices
    {
        private static IPersistenceServices persistenceService;
        private static ITransactionServices transactionService;
        private static IConversationFactory conversationFactory;

        public KomponentenTest_PersistenceServices()
        {
        }

        private TestContext testContextInstance;

        private static void CleanupAndCreateTestData()
        {
            using (IConversation conversation = conversationFactory.NewConversation())
            {
                conversation.ExecuteTransactional(delegate(ITransactionControl control)
                {
                    persistenceService.ExecuteSQLQuery("DELETE FROM NHIBERNATE_HILO_VALUES");

                    persistenceService.Save(new NHibernate_HiLo_Values() { TableName = "Address", NextHi = 0 });
                    persistenceService.Save(new NHibernate_HiLo_Values() { TableName = "Customer", NextHi = 0 });
                    persistenceService.Save(new NHibernate_HiLo_Values() { TableName = "Order", NextHi = 0 });
                    persistenceService.Save(new NHibernate_HiLo_Values() { TableName = "Product", NextHi = 0 });

                    return 0;
                });

                conversation.ExecuteTransactional(delegate(ITransactionControl control)
                {
                    // Cleanup database
                    persistenceService.ExecuteSQLQuery("DELETE FROM ORDERSTOPRODUCTS");
                    persistenceService.ExecuteSQLQuery("DELETE FROM `ORDER`");
                    persistenceService.ExecuteSQLQuery("DELETE FROM PRODUCT");
                    persistenceService.ExecuteSQLQuery("DELETE FROM CUSTOMER");
                    persistenceService.ExecuteSQLQuery("DELETE FROM ADDRESS");

                    // Add objects to database
                    Product p1 = new Product() { Name = "iPhone" };
                    Product p2 = new Product() { Name = "Espresso" };

                    Customer c = new Customer() { Name = "Stefan" };
                    Address a = new Address() { Customer = c, City = "Hamburg", Street = "Berliner Tor 7", Zip = "20099" };
                    c.Address = a;

                    Order o1 = new Order();
                    o1.AddProduct(p1);
                    o1.AddProduct(p2);
                    c.AddOrder(o1);

                    Order o2 = new Order();
                    o2.AddProduct(p1);
                    c.AddOrder(o2);

                    persistenceService.Save(c);

                    return 0;
                });
            }
        }

        /// <summary>
        /// Initialize the persistence
        /// </summary>
        /// <param name="testContext">Testcontext provided by framework</param>
        [ClassInitialize]
        public static void InitializePersistence(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreatePersistenceService(
                PersistenceServicesFactory.PersistenceServiceType.MySQL,
                true,
                out persistenceService,
                out transactionService,
                out conversationFactory);

            CleanupAndCreateTestData();
        }

        [ClassCleanup]
        public static void CleanUp()
        {
        }

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
        }

        [TestMethod]
        public void TestGetAll()
        {
            IList<Customer> customers = null;
            using (IConversation conversation = conversationFactory.NewConversation())
            {
                conversation.ExecuteTransactional(delegate(ITransactionControl control)
                {
                    customers = persistenceService.GetAll<Customer>();
                    return 0;
                });
            }
            Assert.AreEqual(1, customers.Count);
        }

        [TestMethod]
        public void TestSaveRollback()
        {
            Customer customer = new Customer() { Name = "Mustermann" };
            int customerId = 0;
            using (IConversation conversation = conversationFactory.NewConversation())
            {
                conversation.ExecuteTransactional(delegate(ITransactionControl control)
                {
                    persistenceService.Save(customer);
                    Assert.IsTrue(customer.Id > 0);
                    customerId = customer.Id;
                    control.DoRollbackAtEndOfTransaction();
                    return 0;
                });
                conversation.ExecuteTransactional(delegate(ITransactionControl control)
                {
                    Assert.IsNull(persistenceService.GetById<Customer, int>(customerId));
                    return 0;
                });
            }
        }

        [TestMethod]
        public void TestQuery()
        {
            using (IConversation conversation = conversationFactory.NewConversation())
            {
                conversation.ExecuteTransactional(delegate(ITransactionControl control)
                {
                    IEnumerable<Order> orderQuery =
                            from order in persistenceService.Query<Order>()
                            where order.Products.Count() == 2
                            select order;

                    Assert.AreEqual(1, orderQuery.Count<Order>());

                    return 0;
                });
            }
        }
    }
}
