using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Util.Common.Interfaces
{
    /// <summary>
    /// Implements data transfer object pattern
    /// </summary>
    /// <typeparam name="T">type of value object</typeparam>
    [Serializable]
    public abstract class DTOType<T> : ValueEqualityType<T> where T : ValueEqualityType<T>
    {
        public DTOType()
        {
        }
    }

    public interface ICanConvertToEntity<T>
    {
        T ToEntity();
    }

    public interface ICanConvertToDTO<T>
    {
        T ToDTO();
    }
}
