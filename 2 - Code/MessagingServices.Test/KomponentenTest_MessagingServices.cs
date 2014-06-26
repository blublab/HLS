// <copyright file="KomponentenTest_MessagingServices.cs" company="Stefan Sarstedt">
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using Util.Common.Interfaces;
using Util.MessagingServices.Implementations;
using Util.MessagingServices.Interfaces;

namespace Tests.KomponentenTest.MessagingService
{
    public class OrderDetail : DTOType<OrderDetail>
    {
        public string Name   { get; set; }
        public string Color  { get; set; }
        public int    Amount { get; set; }
    }

    [TestClass]
    public class KomponentenTest_MessagingServices
    {
        private static IMessagingServices messagingManager = null;
        private static IQueueServices<OrderDetail> orderDetailQueue = null;

        [ClassInitialize]
        public static void SetUp(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            orderDetailQueue = messagingManager.CreateQueue<OrderDetail>("HLS.Queue");
            orderDetailQueue.Purge();       
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            messagingManager.Dispose();
        }

        [TestMethod]
        public void TestSendReceiveSyncOrderDetailMessage()
        {
            OrderDetail orderDetailSent = new OrderDetail() { Name = "iPad", Amount = 42, Color = "Black" };
            orderDetailQueue.Send(orderDetailSent);

            OrderDetail orderDetailReceived = orderDetailQueue.ReceiveSync((o) =>
            {
                return MessageAckBehavior.AcknowledgeMessage;
            });

            Assert.IsTrue(orderDetailSent == orderDetailReceived, "OrderDetail received is not identically to the one sent.");
        }

        [TestMethod]
        public void TestSendReceiveAsyncOrderDetailMessage()
        {
            OrderDetail orderDetailSent = new OrderDetail() { Name = "iPad", Amount = 42, Color = "Black" };
            orderDetailQueue.Send(orderDetailSent);

            Task<OrderDetail> receiverTask = orderDetailQueue.ReceiveAsync((o) => 
            { 
                return MessageAckBehavior.AcknowledgeMessage; 
            });
            SpinWait.SpinUntil(() => receiverTask.IsCompleted, 1000);
            Assert.IsTrue(receiverTask.IsCompleted, "OrderDetail was not received from queue.");

            OrderDetail orderDetailReceived = receiverTask.Result;
            Assert.IsTrue(orderDetailSent == orderDetailReceived, "OrderDetail received is not identically to the one sent.");
        }

        [TestMethod]
        public void TestSendRejectAsyncOrderDetailMessage()
        {
            OrderDetail orderDetailSent = new OrderDetail() { Name = "iPad", Amount = 42, Color = "Black" };
            orderDetailQueue.Send(orderDetailSent);

            Task<OrderDetail> receiverTask = orderDetailQueue.ReceiveAsync((o) =>
            {
                return MessageAckBehavior.RejectMessage;
            });
            SpinWait.SpinUntil(() => receiverTask.IsCompleted, 1000);
            Assert.IsTrue(receiverTask.IsCompleted, "OrderDetail was not seen in the queue.");

            receiverTask = orderDetailQueue.ReceiveAsync((o) =>
            {
                return MessageAckBehavior.AcknowledgeMessage;
            });
            SpinWait.SpinUntil(() => receiverTask.IsCompleted, 1000);
            Assert.IsTrue(receiverTask.IsCompleted, "OrderDetail was not received from queue.");

            OrderDetail orderDetailReceived = receiverTask.Result;
            Assert.IsTrue(orderDetailSent == orderDetailReceived, "OrderDetail received is not identically to the one sent.");
        }
    }
}
