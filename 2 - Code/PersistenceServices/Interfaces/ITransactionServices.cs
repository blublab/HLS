// <copyright file="ITransactionServices.cs" company="Stefan Sarstedt">
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
    /// <summary>
    /// Type-0 interface to encapsulate a transaction framework.
    /// </summary>
    public interface ITransactionServices
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

        /// <summary>
        /// Execute code within a new transaction.
        /// </summary>
        /// <typeparam name="T">Returntype of transactional code.</typeparam>
        T ExecuteTransactional<T>(Func<T> body);

        /// <summary>
        /// Execute code within a new transaction.
        /// </summary>
        void ExecuteTransactional(Action body);

        /// <summary>
        /// Execute code within a new transaction, if no transaction was provided.
        /// </summary>
        /// <typeparam name="T">Returntype of transactional code.</typeparam>
        T ExecuteTransactionalIfNoTransactionProvided<T>(Func<T> body);

        /// <summary>
        /// Execute code within a new transaction, if no transaction was provided.
        /// </summary>
        void ExecuteTransactionalIfNoTransactionProvided(Action body);

        /// <summary>
        /// Gets a value indicating whether the code executes in an active transaction.
        /// </summary>
        bool IsTransactionActive { get; }
    }
}
