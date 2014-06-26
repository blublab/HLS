// <copyright file="OptimisticConcurrencyException.cs" company="Stefan Sarstedt">
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

namespace Util.PersistenceServices.Exceptions
{
    /// <summary>
    /// Repräsentiert einen Versionsfehler einer Entität.
    /// </summary>
    public class OptimisticConcurrencyException : Exception
    {
        public object EntityName { get; private set; }
        public object Identifier { get; private set; }

        public OptimisticConcurrencyException(string entityName, object identifier)
        {
            this.EntityName = entityName;
            this.Identifier = identifier;
        }
    }
}
