// <copyright file="MessagingServicesFactory.cs" company="Stefan Sarstedt">
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
using System;
using Util.Common.Exceptions;
using Util.MessagingServices.Interfaces;

namespace Util.MessagingServices.Implementations
{
    public class MessagingServicesFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IMessagingServices CreateMessagingServices()
        {
            try
            {
                // RabbitMQ is currently the only supported service
                return new RabbitMQMessagingServices();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error creating rabbitmq messaging provider.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }
    }
}
