// <copyright file="IPersistenceServices.cs" company="Stefan Sarstedt">
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Util.PersistenceServices.Interfaces
{
    /// <summary>
    /// Type-0 interface to encapsulate a persistence framework.
    /// </summary>
    public interface IPersistenceServices
    {
        /// <summary>
        /// Fetches an object from the persistence by id.
        /// </summary>
        /// <typeparam name="T">Type of object to fetch.</typeparam>
        /// <typeparam name="I">Type of primary key.</typeparam>
        /// <param name="id">Key of object to be fetched.</param>
        /// <pre>id must not be empty.</pre>
        /// <returns>Fetched object.</returns>
        T GetById<T, I>(I id) where T : class;

        /// <summary>
        /// Fetches all objects of a given type.
        /// </summary>
        /// <typeparam name="T">Type of objects to fetch.</typeparam>
        /// <returns>List of fetched objects.</returns>
        IList<T> GetAll<T>() where T : class;

        /// <summary>
        /// Returns an instance of System.Linq.IQueryable for defining Linq-Queries.
        /// </summary>
        /// <typeparam name="T">Type for which queries are to be defined.</typeparam>
        /// <returns>Instance of System.Linq.IQueryable</returns>
        IQueryable<T> Query<T>() where T : class;

        /// <summary>
        /// Execute an SQL-statement.
        /// </summary>
        /// <param name="query">SQL-statement to be executed.</param>
        /// <param name="queryParameters">Query parameters.</param>
        /// <pre>query must not be empty.</pre>
        /// <returns>Query results.</returns>
        IList ExecuteSQLQuery(string query, IDictionary<string, object> queryParameters = null);

        /// <summary>
        /// Executes a database statement which is provided in an external configuration.
        /// </summary>
        /// <param name="queryName">Name of the statement.</param>
        /// <param name="queryParameters">Parameters of the statement.</param>
        /// <pre>query must not be empty.</pre>
        /// <returns>Query results.</returns>
        IList ExecuteNamedQuery(string queryName, IDictionary<string, object> queryParameters = null);

        /// <summary>
        /// Saves an object to the persistence.
        /// </summary>
        /// <typeparam name="T">Type of object to add.</typeparam>
        /// <param name="entity">Object to add.</param>
        /// <pre>entity must not be empty.</pre>
        /// <returns>Object added.</returns>
        T Save<T>(T entity) where T : class;

        /// <summary>
        /// Deletes an object from the persistence.
        /// </summary>
        /// <typeparam name="T">Type of object to delete.</typeparam>
        /// <param name="entity">Object to be deleted.</param>
        /// <pre>entity must not be empty.</pre>
        void Delete<T>(T entity) where T : class;

        /// <summary>
        /// Deletes all instances of a given type from the persistence.
        /// </summary>
        /// <typeparam name="T">Type of ojects to delete.</typeparam>
        void DeleteAll<T>() where T : class;
    }
}
