using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.TimeServices
{
    public interface ITimeServices
    {
        DateTime Now { get; }
    }

    public class TimeServices : ITimeServices
    {
        public DateTime Now
        {
            get 
            {
                return DateTime.Now;
            }
        }
    }
}
