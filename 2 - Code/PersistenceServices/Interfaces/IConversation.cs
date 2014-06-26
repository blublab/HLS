// <copyright file="IConversation.cs" company="Stefan Sarstedt">
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

namespace Util.PersistenceServices.Interfaces
{
    public interface ITransactionControl
    {
        /// <summary>
        /// Sets the transaction-status to 'rollback' indicating that the current transaction is to be aborted.
        /// </summary>
        void DoRollbackAtEndOfTransaction();
    }

    /// <summary>
    /// Code to be executed within a transaction.
    /// </summary>
    /// <typeparam name="T">Returntype of the code.</typeparam>
    /// <param name="status">Transactional status for indicating a rollback.</param>
    /// <returns>A custom return value of the given type.</returns>
    public delegate T TransactionalCode<T>(ITransactionControl status);

    /// <summary>
    /// Code to be executed within a transaction.
    /// </summary>
    /// <param name="status">Transactional status for indicating a rollback.</param>
    public delegate void TransactionalCode(ITransactionControl status);

    public interface IConversationFactory
    {
        /// <summary>
        /// Starts a new conversation.
        /// </summary>
        /// <returns>A new conversation.</returns>
        IConversation NewConversation();
    }

    public interface IConversation : IDisposable
    {
        /// <summary>
        /// Code to be executed within a transaction.
        /// </summary>
        /// <typeparam name="T">Returntype of the code.</typeparam>
        /// <param name="body">Transactional status for indicating a rollback.</param>
        /// <returns>A custom return value of the given type.</returns>
        T ExecuteTransactional<T>(TransactionalCode<T> body);

        /// <summary>
        /// Code to be executed within a transaction.
        /// </summary>
        /// <param name="body">Transactional status for indicating a rollback.</param>
        void ExecuteTransactional(TransactionalCode body);
    }
}
