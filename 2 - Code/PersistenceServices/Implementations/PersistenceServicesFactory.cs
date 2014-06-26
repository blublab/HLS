// <copyright file="PersistenceServicesFactory.cs" company="Stefan Sarstedt">
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

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Util.Common.Exceptions;
using Util.PersistenceServices.Implementations.NHibernateImplementation;
using Util.PersistenceServices.Interfaces;

namespace Util.PersistenceServices.Implementations
{
    /// <summary>
    /// Factory for different persistence implementations.
    /// </summary>
    public sealed class PersistenceServicesFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Types of persistence managers availavble.
        /// </summary>
        public enum PersistenceServiceType { MSSQL2008, MySQL }

        /// <summary>
        /// Factory-method for different persistences.
        /// </summary>
        /// <param name="type">Type of persistence to create.</param>
        /// <param name="schemaUpdate">Indicates if schema is updated ('true') or recreated from scratch ('false').</param>
        /// <param name="persistenceService">Reference to persistence service.</param>
        /// <param name="transactionService">Reference to transaction service.</param>
        /// /// <param name="conversationFactory">Reference to conversation factory for creating conversations.</param>
        public static void CreatePersistenceService(
            PersistenceServiceType type, 
            bool schemaUpdate, 
            out IPersistenceServices persistenceService, 
            out ITransactionServices transactionService,
            out IConversationFactory conversationFactory)
        {
            Configuration configuration = new Configuration();
            FluentConfiguration fluentConfiguration = Fluently.Configure(configuration);

            // fetch connection string from configuration file
            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["DatabaseConnection"];
            Contract.Assert(connectionSettings != null, "A database connection setting needs to be defined in the App.config."); 
            string connectionString = connectionSettings.ConnectionString;
            Contract.Assert(connectionString != null, "A database connection string needs to be defined in the App.config."); 

            // set persistencetype
            switch (type)
            {
                case PersistenceServiceType.MSSQL2008:
                    fluentConfiguration = fluentConfiguration.Database(
                        MsSqlConfiguration.MsSql2008.ConnectionString(connectionString));
                    break;
                case PersistenceServiceType.MySQL:
                    fluentConfiguration = fluentConfiguration.Database(
                        MySQLConfiguration.Standard.ConnectionString(connectionString));
                    break; 
            }

            // get all user assemblies
            ICollection<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => 
                         assembly.ManifestModule.Name != "<In Memory Module>"
                     && !assembly.FullName.StartsWith("mscorlib") 
                     && !assembly.FullName.StartsWith("System")
                     && !assembly.FullName.StartsWith("Microsoft")).ToList();
            foreach (Assembly mappingAssembly in allAssemblies)
            {
                // find all types that derive from ClassMap<>
                IList<Type> types = mappingAssembly.GetTypes().Where(t =>
                       t != typeof(AutoMapping<>)
                    && t.BaseType != null
                    && t.BaseType.IsGenericType
                    && t.BaseType.GetGenericTypeDefinition() == typeof(ClassMap<>)).ToList();

                // if there are any, we add their assembly
                if (types.Count > 0)
                {
                    fluentConfiguration = fluentConfiguration.Mappings(m => m.FluentMappings.AddFromAssembly(mappingAssembly));
                }
            }

            try
            {
                configuration = fluentConfiguration
                    .ExposeConfiguration(cfg =>
                        {
                            if (schemaUpdate)
                            {
                                new SchemaUpdate(cfg)
                                    .Execute(false, true);
                            }
                            else
                            {
                                new SchemaExport(cfg)
                                    .Create(false, true);
                            }
                        })
                    .BuildConfiguration();
            }
            catch (FluentConfigurationException fluentEx)
            {
                if (fluentEx.InnerException != null)
                {
                    if (fluentEx.InnerException is HibernateException)
                    {
                        if (fluentEx.InnerException.Message.Contains("Table") && fluentEx.InnerException.Message.Contains("already exists"))
                        {
                            TechnicalProblemException tpEx = new TechnicalProblemException("Error building FluentNHibernate configuration. Try dropping and re-creating database schema.", fluentEx);
                            Log.Fatal(tpEx.ToString());
                            throw tpEx;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error building FluentNHibernate configuration.", ex);
                Log.Fatal(tpEx.ToString());
                throw tpEx;
            }
            NHibernatePersistenceServices nhPersistenceService = new NHibernatePersistenceServices(configuration);
            persistenceService = nhPersistenceService as IPersistenceServices;
            transactionService = nhPersistenceService as ITransactionServices;
            conversationFactory = persistenceService as IConversationFactory;
        }

        /// <summary>
        /// Creates a simple MySQL persistence with one conversation per transaction.
        /// </summary>
        /// <param name="persistenceService">Reference to persistence service.</param>
        /// <param name="transactionService">Reference to transaction service.</param>
        public static void CreateSimpleMySQLPersistenceService(
            out IPersistenceServices persistenceService, 
            out ITransactionServices transactionService)
        {
            IConversationFactory conversationFactory;

            PersistenceServicesFactory.CreatePersistenceService(
               PersistenceServicesFactory.PersistenceServiceType.MySQL,
               false,
               out persistenceService,
               out transactionService,
               out conversationFactory);

            // forget the conversationFactory
        }
    }
}
