// <copyright file="RabbitMQQueue.cs" company="Stefan Sarstedt">
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
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Util.Common.Exceptions;
using Util.Common.Interfaces;
using Util.MessagingServices.Interfaces;

namespace Util.MessagingServices.Implementations
{
    internal class RabbitMQQueue<T> : IQueueServices<T>, IDisposable where T : DTOType<T>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RabbitMQQueue<T>));

        public string Queue { get; private set; }
        private readonly IModel channel = null;
        private readonly QueueingBasicConsumer consumer = null;

        public RabbitMQQueue(IConnection connection, string queue)
        {
            Contract.Requires(connection != null, "connection-Parameter is required.");
            Contract.Requires(queue != null, "queue-Parameter is required.");
            this.Queue = queue;
            this.channel = connection.CreateModel();
            channel.QueueDeclare(queue, true, false, false, null);
            channel.BasicQos(0, 1, false);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queue, false, consumer);
        }

        public void Send(T data)
        {
            Contract.Requires(data != null, "data-Parameter is required.");
            try
            {
                string jsonString = JsonConvert.SerializeObject(data);
                byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonString);
                channel.BasicPublish("", Queue, null, body);
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error sending message to queue.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        public Task<T> ReceiveAsync(Func<T, MessageAckBehavior> function)
        {
            Contract.Requires(function != null, "function-Parameter is required.");
            try
            {
                Task<T> receiver = new Task<T>(() =>
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    byte[] body = ea.Body;
                    string message = System.Text.Encoding.UTF8.GetString(body);
                    T data = JsonConvert.DeserializeObject<T>(message);

                    // execute user function
                    MessageAckBehavior msgAckBehavior = function(data);

                    if (msgAckBehavior == MessageAckBehavior.AcknowledgeMessage)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                        Log.Debug(string.Format("Message '{0}' was acknowledged in queue '{1}'.", message, Queue));
                    }
                    else
                    {
                        channel.BasicNack(ea.DeliveryTag, false, true);
                        Log.Debug(string.Format("Message '{0}' was left in queue '{1}'.", message, Queue));
                    }

                    return data;
                });
                receiver.Start();

                return receiver;
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error receiving message from queue asynchronously.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }     
        }

        public T ReceiveSync(Func<T, MessageAckBehavior> function)
        {
            Contract.Requires(function != null, "function-Parameter is required.");
            try
            {
                Task<T> receiverTask = ReceiveAsync(function);
                receiverTask.Wait();

                return receiverTask.Result;
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error receiving message from queue synchronously.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        public void Purge()
        {
            try 
            {
                channel.QueuePurge(Queue);
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error purging message queue.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        public void Dispose()
        {
            try
            {
                channel.Close();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error closing message queue.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }           
        }
    }
}
