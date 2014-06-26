// <copyright file="IQueueServices.cs" company="Stefan Sarstedt">
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

using System;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace Util.MessagingServices.Interfaces
{
    /// <summary>
    /// Acknowledgement mode for receiving functions.
    /// </summary>
    public enum MessageAckBehavior
    {
        AcknowledgeMessage, RejectMessage
    }

    /// <summary>
    /// Type-0 interface that encapsulates a queue.
    /// </summary>
    /// <typeparam name="T>Type of objects (DTO or data types) which the queue operates on.</typeparam>
    public interface IQueueServices<T> where T : DTOType<T>
    {
        string Queue { get; }

        /// <summary>
        /// Sends an object.
        /// </summary>
        /// <param name="data">Object to be sent.</param>
        void Send(T data);

        /// <summary>
        /// Receveives an object synchronously (FiFo).
        /// </summary>
        /// <param name="function">Code to be executed upon receival of message.
        /// Returnvalue of this functions determines, if the message is to be acknowledge or to be rejected.</param>
        /// <returns>Object received.</returns>
        T ReceiveSync(Func<T, MessageAckBehavior> function);

        /// <summary>
        /// Receives an object asynchronously (FiFo).
        /// </summary>
        /// <param name="function">Code to be executed upon receival of message.
        /// Returnvalue of this functions determines, if the message is to be acknowledge or to be rejected.</param>
        /// <returns>Object received.</returns>
        Task<T> ReceiveAsync(Func<T, MessageAckBehavior> function);

        /// <summary>
        /// Purge the queue.
        /// </summary>
        void Purge();
    }
}
