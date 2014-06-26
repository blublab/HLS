// <copyright file="RabbitMQMessagingServices.cs" company="Stefan Sarstedt">
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

using log4net;
using RabbitMQ.Client;
using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using Util.Common.Exceptions;
using Util.Common.Interfaces;
using Util.MessagingServices.Interfaces;

namespace Util.MessagingServices.Implementations
{
    public sealed class RabbitMQMessagingServices : IMessagingServices
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static IConnection connection = null;

        public RabbitMQMessagingServices()
        {
            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["RabbitMQ"];
            if (connectionSettings == null)
            {
                throw new ArgumentException("RabbitMQ-connectionsettings nicht in App.config gefunden.");
            }

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionSettings.ConnectionString;

            ConnectionFactory factory = new ConnectionFactory();
            factory.HostName = (string)builder["host"];
            factory.UserName = (string)builder["username"];
            factory.Password = (string)builder["password"];
            try
            {
                connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error creating rabbitmq connection.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }    
        }

        public void Dispose()
        {
            try
            {
                connection.Close();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error closing rabbitmq connection.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        public IQueueServices<T> CreateQueue<T>(string queue) where T : DTOType<T>
        {
            Contract.Requires(queue != null, "queue-Parameter is required.");

            try
            {
                return new RabbitMQQueue<T>(connection, queue);
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error creating messaging queue.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }
    }
}
