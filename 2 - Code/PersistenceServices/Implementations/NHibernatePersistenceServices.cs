// <copyright file="NHibernatePersistenceServices.cs" company="Stefan Sarstedt">
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using Util.Common.Exceptions;
using Util.PersistenceServices.Exceptions;
using Util.PersistenceServices.Interfaces;

namespace Util.PersistenceServices.Implementations.NHibernateImplementation
{
    public sealed class NHibernatePersistenceServices : IPersistenceServices, ITransactionServices, IConversationFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISessionFactory sessionFactory;

        public NHibernatePersistenceServices(Configuration configuration)
        {
            Contract.Requires(configuration != null, "NHibernatePersistenceManager must be configured.");
            
            configuration.Properties.Add("current_session_context_class", "NHibernate.Context.CallSessionContext");
            this.sessionFactory = configuration.BuildSessionFactory();
        }

        private bool HasCurrentSession()
        {
            return NHibernate.Context.CallSessionContext.HasBind(sessionFactory);
        }

        private ISession CurrentSession
        {
            get
            {
                if (HasCurrentSession() == false)
                {
                    throw new TechnicalProblemException("No active NHibernate-session. Please execute code inside a transaction.", null);
                }

                try
                {
                    ISession currentSession = sessionFactory.GetCurrentSession();
                    return currentSession;
                }
                catch (Exception ex)
                {
                    TechnicalProblemException tpEx = new TechnicalProblemException("Error retrieving session from sessionfcatory.", ex);
                    Log.Error(tpEx.ToString());
                    throw tpEx;
                }
            }
        }

        #region IPersistenceService Members

        /// <inheritdoc/>
        public T GetById<T, I>(I id) where T : class
        {
            Contract.Requires(id != null, "id-parameter is required.");
            try
            {
                T entity;
                entity = CurrentSession.Get<T>(id); // "Get" uses database access and does not return a proxy object (as opposed to "Load" which returns a proxy)
                return entity;
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error fetching object from persistence by id.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public IQueryable<T> Query<T>() where T : class
        {
            try
            {
                return CurrentSession.Query<T>();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error creating query object.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public IList ExecuteSQLQuery(string query, IDictionary<string, object> queryParameters)
        {
            Contract.Requires(!string.IsNullOrEmpty(query), "query-parameter is required.");

            try
            {
                IQuery sqlQuery = CurrentSession.CreateSQLQuery(query);
                if (queryParameters != null)
                {
                    foreach (string key in queryParameters.Keys)
                    {
                        Contract.Assert(key != null, "A parameter name is needed.");
                        Contract.Assert(key.Length > 0, "A parameter name is needed.");
                        Contract.Assert(queryParameters[key] != null, "A parameter object is needed for key '" + key + "'.");

                        sqlQuery = sqlQuery.SetParameter(key, queryParameters[key]);
                    }
                }
                return sqlQuery.List();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error executing sql-query.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public IList ExecuteNamedQuery(string queryName, IDictionary<string, object> queryParameters)
        {
            Contract.Requires(!string.IsNullOrEmpty(queryName), "query-parameter is required.");

            try
            {
                IQuery sqlQuery = CurrentSession.GetNamedQuery(queryName);
                if (queryParameters != null)
                {
                    foreach (string key in queryParameters.Keys)
                    {
                        Contract.Assert(key != null, "A parameter name is needed.");
                        Contract.Assert(key.Length > 0, "A parameter name is needed.");
                        Contract.Assert(queryParameters[key] != null, "A parameter object is needed for key '" + key + "'.");

                        sqlQuery = sqlQuery.SetParameter(key, queryParameters[key]);
                    }
                }
                return sqlQuery.List();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error executing named query.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public IList<T> GetAll<T>() where T : class
        {
            try
            {
                return CurrentSession.CreateCriteria<T>().List<T>();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error fetching objects of a given type.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public T Save<T>(T entity) where T : class
        {
            Contract.Requires(entity != null, "entity-parameter is required.");
            try
            {
                CurrentSession.SaveOrUpdate(entity);
                return entity;
            }
            catch (StaleObjectStateException ex)
            {
                throw new OptimisticConcurrencyException(ex.EntityName, ex.Identifier);
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error saving object.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public void Delete<T>(T entity) where T : class
        {
            Contract.Requires(entity != null, "entity-parameter is required.");
            try
            {
                CurrentSession.Delete(entity);
                CurrentSession.Flush();
            }
            catch (StaleObjectStateException ex)
            {
                throw new OptimisticConcurrencyException(ex.EntityName, ex.Identifier);
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error deleting object.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public void DeleteAll<T>() where T : class
        {
            try
            {
                CurrentSession.CreateQuery("DELETE " + typeof(T).Name).ExecuteUpdate();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error deleting all objects of a given type.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        #endregion

        #region ITransactionService Members

        /// <inheritdoc/>
        public bool IsTransactionActive
        {
            get
            {
                return HasCurrentSession();
            }
        }

        /// <inheritdoc/>
        public T ExecuteTransactional<T>(TransactionalCode<T> body)
        {
            Contract.Requires(body != null, "body-parameter is required.");
            T result;
            using (IConversation conversation = NewConversation())
            {
                result = conversation.ExecuteTransactional(body);
            }
            return result;
        }

        /// <inheritdoc/>
        public void ExecuteTransactional(TransactionalCode body)
        {
            Contract.Requires(body != null, "body-parameter is required.");
            
            using (IConversation conversation = NewConversation())
            {
                conversation.ExecuteTransactional(body);
            }
        }

        public T ExecuteTransactionalIfNoTransactionProvided<T>(Func<T> body)
        {
            T result;
            if (IsTransactionActive == false)
            {
                // no active transaction -> create a new one and execute code within it
                result = ExecuteTransactional(delegate(ITransactionControl tc) { return body(); });
            }
            else
            {
                result = body();
            }
            return result;
        }

        public T ExecuteTransactional<T>(Func<T> body)
        {
            T result = ExecuteTransactional(delegate(ITransactionControl tc) { return body(); });
            return result;
        }

        public void ExecuteTransactional(Action body)
        {
            ExecuteTransactional(delegate(ITransactionControl tc) { body(); });
        }

        public void ExecuteTransactionalIfNoTransactionProvided(Action body)
        {
            if (IsTransactionActive == false)
            {
                // no active transaction -> create a new one and execute code within it
                ExecuteTransactional(delegate(ITransactionControl tc) { body(); });
            }
            else
            {
                body();
            }
        }

        #endregion

        #region IConversationFactory Members

        /// <inheritdoc/>
        public IConversation NewConversation()
        {
            try
            {
                return new NHibernateConversation(sessionFactory);
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error creating conversation.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        #endregion
    }

    public sealed class NHibernateConversation : IConversation
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISessionFactory sessionFactory = null;
        private ISession session = null;

        internal NHibernateConversation(ISessionFactory sessionFactory)
        {
            Contract.Requires(sessionFactory != null, "sessionFactory-parameter is required.");
            this.sessionFactory = sessionFactory;
            Reset();
            Contract.Assert(this.session != null);
            this.session.FlushMode = FlushMode.Commit;
        }

        #region IConversation Members

        private void End()
        {
            try
            {
                if (this.session != null)
                {
                    this.session.Close();
                }
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error ending conversation.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }          
        }

        private void Reset()
        {
            // Resets the session. This is necessary when a transaction has beend rollbacked to remove stale objects.
            try
            {
                if (this.session != null)
                {
                    this.session.Close();
                }
                this.session = sessionFactory.OpenSession();
            }
            catch (Exception ex)
            {
                TechnicalProblemException tpEx = new TechnicalProblemException("Error ending conversation.", ex);
                Log.Error(tpEx.ToString());
                throw tpEx;
            }
        }

        /// <inheritdoc/>
        public T ExecuteTransactional<T>(TransactionalCode<T> body)
        {
            Contract.Requires(body != null, "body-parameter is required.");
            ITransaction transaction = null;
            try
            {
                transaction = session.BeginTransaction();
                NHibernateTransactionControl transactionControl = new NHibernateTransactionControl();

                NHibernate.Context.CallSessionContext.Bind(session);

                T result = body.Invoke(transactionControl);

                if (transactionControl.DoRollback)
                {
                    transaction.Rollback();
                    Reset();
                }
                else
                {
                    transaction.Commit();
                }
                return result;
            }
            catch (StaleObjectStateException ex)
            {
                try
                {
                    // try rolling back transaction
                    transaction.Rollback();
                    Reset();
                }
                catch (Exception)
                {
                }
                throw new OptimisticConcurrencyException(ex.EntityName, ex.Identifier);
            }
            catch (Exception ex)
            {
                try
                {
                    // try rolling back transaction
                    transaction.Rollback();
                    Reset();
                }
                catch (Exception)
                {
                }

                // only NHibernate-exceptions are technical problems here
                if (ex.GetType().FullName.Contains("NHibernate"))
                {
                    TechnicalProblemException tpEx = new TechnicalProblemException("Error executing transactional code. See inner exception for details.", ex);
                    Log.Error(tpEx.ToString());
                    throw tpEx;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                NHibernate.Context.CallSessionContext.Unbind(this.session.SessionFactory);
            }
        }

        /// <inheritdoc/>
        public void ExecuteTransactional(TransactionalCode body)
        {
            Contract.Requires(body != null, "body-parameter is required.");
            ITransaction transaction = null;
            try
            {
                transaction = session.BeginTransaction();
                NHibernateTransactionControl transactionControl = new NHibernateTransactionControl();

                NHibernate.Context.CallSessionContext.Bind(session);

                body.Invoke(transactionControl);

                if (transactionControl.DoRollback)
                {
                    transaction.Rollback();
                    Reset();
                }
                else
                {
                    transaction.Commit();
                }
            }
            catch (StaleObjectStateException ex)
            {
                try
                {
                    // try rolling back transaction
                    transaction.Rollback();
                    Reset();
                }
                catch (Exception)
                {
                }
                throw new OptimisticConcurrencyException(ex.EntityName, ex.Identifier);
            }
            catch (Exception ex)
            {
                try
                {
                    // try rolling back transaction
                    transaction.Rollback();
                    Reset();
                }
                catch (Exception)
                {
                }

                // only NHibernate-exceptions are technical problems here
                if (ex.GetType().FullName.Contains("NHibernate"))
                {
                    TechnicalProblemException tpEx = new TechnicalProblemException("Error executing transactional code. See inner exception for details.", ex);
                    Log.Error(tpEx.ToString());
                    throw tpEx;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                NHibernate.Context.CallSessionContext.Unbind(this.session.SessionFactory);
            }
        }
        #endregion

        #region IDisposable Members

        /// <inheritdoc/>
        public void Dispose()
        {
            End();
        }

        #endregion
    }

    internal sealed class NHibernateTransactionControl : ITransactionControl
    {
        private bool doRollback = false;

        public NHibernateTransactionControl()
        {
        }

        #region ITransactionStatus Members

        public bool DoRollback
        {
            get
            {
                return doRollback;
            }
        }

        public void DoRollbackAtEndOfTransaction()
        {
            doRollback = true;
        }

        #endregion
    }
}
