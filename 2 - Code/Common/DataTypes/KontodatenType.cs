using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace Common.DataTypes
{
    [Serializable]
    public class KontodatenType : ValueType<KontodatenType>
    {
        public readonly string IBAN;
        public readonly string BIC;

        public KontodatenType(string iban, string bic)
        {
            if (!IsValidIban(iban))
            {
                throw new ArgumentException("Die Zeichenkette '" + iban + "' ist keine gültige International Bank Account Number.");
            }
            this.IBAN = iban;

            if (!IsValidBic(bic))
            {
                throw new ArgumentException("Die Zeichenkette '" + bic + "' ist kein gültiger Bank Identifier Code.");
            }
            this.BIC = bic;
        }

        /// <summary>
        /// Check for valid IBAN.
        /// </summary>
        /// <see href="http://snipplr.com/view/15322/"/>
        public static bool IsValidIban(string iban)
        {
            return Regex.IsMatch(iban, @"[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}");
        }

        /// <summary>
        /// Check for valid BIC.
        /// </summary>
        /// <see href="http://snipplr.com/view/15320/"/>
        public static bool IsValidBic(string bic)
        {
            return Regex.IsMatch(bic, @"([a-zA-Z]{4}[a-zA-Z]{2}[a-zA-Z0-9]{2}([a-zA-Z0-9]{3})?)");
        }

        public override bool Equals(object obj)
        {
            KontodatenType other = obj as KontodatenType;

            if (other == null)
            {
                return false;
            }

            return this.IBAN.Equals(other.IBAN) && this.BIC.Equals(other.BIC);
        }

        public override int GetHashCode()
        {
            return this.IBAN.GetHashCode() + this.BIC.GetHashCode();
        }

        public override string ToString()
        {
            return "IBAN: " + this.IBAN.ToString() + "| BIC: " + this.BIC.ToString();
        }
    }
}
