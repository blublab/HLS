using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Util.Common.Interfaces
{
    /// <summary>
    /// Implements generic pattern for value equality of objects
    /// </summary>
    /// <typeparam name="T">type of value object</typeparam>
    /// <see cref="http://grabbagoft.blogspot.de/2007/06/generic-value-object-equality.html"/>
    /// <remarks>Mit Bugfix für null == null und als serialisierbar markiert.</remarks>
    [Serializable]
    public abstract class ValueEqualityType<T> : IEquatable<T> where T : ValueEqualityType<T>
    {
        internal ValueEqualityType()
        {
        }

        public static bool operator ==(ValueEqualityType<T> x, ValueEqualityType<T> y)
        {
            if ((object)x != null)
            {
                return x.Equals(y);
            }
            else
            {
                return (object)y == null;
            }
        }

        public static bool operator !=(ValueEqualityType<T> x, ValueEqualityType<T> y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            T other = obj as T;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            IEnumerable<FieldInfo> fields = GetFields();

            int startValue = 17;
            int multiplier = 59;

            int hashCode = startValue;

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(this);

                if (value != null)
                {
                    hashCode = (hashCode * multiplier) + value.GetHashCode();
                }
            }

            return hashCode;
        }

        public virtual bool Equals(T other)
        {
            if (other == null)
            {
                return false;
            }

            Type t = GetType();
            Type otherType = other.GetType();

            if (t != otherType)
            {
                return false;
            }

            FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                object value1 = field.GetValue(other);
                object value2 = field.GetValue(this);

                if (value1 == null)
                {
                    if (value2 != null)
                    {
                        return false;
                    }
                }
                else 
                {
                    if (!value1.Equals(value2))
                    {
                        return false;
                    }
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
