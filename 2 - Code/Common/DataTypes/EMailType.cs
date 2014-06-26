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
    public class EMailType : ValueType<EMailType>
    {
        public readonly string EMail;

        public EMailType(string email)
        {
            if (!IsValid(email))
            {
                throw new ArgumentException("Die Zeichenkette '" + email + "' ist keine gültige E-Mail-Adresse.");
            }
            this.EMail = email;
        }

        /// <summary>
        /// Prüft eine potenzielle E-Mail-Adresse auf Gültigkeit.
        /// </summary>
        /// <see href="http://stackoverflow.com/questions/16167983/best-regular-expression-for-email-validation-in-c-sharp"/>
        public static bool IsValid(string email)
        {
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
        }
    }
}
