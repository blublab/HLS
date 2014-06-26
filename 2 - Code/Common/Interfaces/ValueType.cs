using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Util.Common.Interfaces
{
    /// <summary>
    /// Implements generic value object pattern
    /// </summary>
    /// <typeparam name="T">type of value object</typeparam>
    [Serializable]
    public abstract class ValueType<T> : ValueEqualityType<T> where T : ValueType<T>
    {
        protected ValueType()
        {
            if (!AreAllFieldsReadonly())
            {
                throw new InvalidOperationException("All fields of a value type must be readonly, i.e. the type must be immutable.");
            }
        }

        private bool AreAllFieldsReadonly()
        {
            IEnumerable<FieldInfo> fields = this.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (!field.IsInitOnly)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<FieldInfo> GetFields()
        {
            Type t = GetType();

            List<FieldInfo> fields = new List<FieldInfo>();

            while (t != typeof(object))
            {
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                t = t.BaseType;
            }

            return fields;
        }
    }
}
