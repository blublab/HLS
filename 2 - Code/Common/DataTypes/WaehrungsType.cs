using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace Common.DataTypes
{
    [Serializable]
    public class WaehrungsType : ValueType<WaehrungsType>
    {
        public readonly decimal Wert;

        public WaehrungsType(decimal wert)
        {
            this.Wert = wert;
        }

        public static WaehrungsType operator +(WaehrungsType wt1, WaehrungsType wt2)
        {
            return new WaehrungsType(wt1.Wert + wt2.Wert);
        }

        public static WaehrungsType operator -(WaehrungsType w1, WaehrungsType w2)
        {
            return new WaehrungsType(w1.Wert - w2.Wert);
        }

        public decimal EuroToDollar()
        {
            decimal umrechnungsfaktor = 1.37850m; //conversion ratio from 22.10.2013 20:00
            return Wert * umrechnungsfaktor;
        }

        public decimal DollarToEuro()
        {
            decimal umrechnungsfaktor = 0.725505m; //conversion ratio from 22.10.2013 20:00
            return Wert * umrechnungsfaktor;
        }

        public override bool Equals(object obj)
        {
            WaehrungsType other = obj as WaehrungsType;        

            if (other == null)
            {
                return false;
            }

            return this.Wert.Equals(other.Wert);
        }

        public override int GetHashCode()
        {
            return this.Wert.GetHashCode();
        }

        public override string ToString()
        {
            return this.Wert.ToString();
        }
    }
}
