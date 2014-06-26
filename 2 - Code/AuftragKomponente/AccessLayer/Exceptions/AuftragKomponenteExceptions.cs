using ApplicationCore.AuftragKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.AuftragKomponente.AccessLayer
{
    public abstract class SendungsanfrageException : Exception
    {
        public int SaNr { get; private set; }

        public SendungsanfrageException(int saNr)
        {
            this.SaNr = saNr;
        }

        public abstract string Meldung { get; }
    }

    public class SendungsanfrageNichtGefundenException : SendungsanfrageException
    {
        public SendungsanfrageNichtGefundenException(int saNr)
            : base(saNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "Sendungsanfrage " + this.SaNr + " wurde nicht gefunden.";
            }
        }
    }

    public class SendungsanfrageNichtErfasstException : SendungsanfrageException
    {
        public SendungsanfrageNichtErfasstException(int saNr)
            : base(saNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "Sendungsanfrage " + this.SaNr + " wurde nicht erfasst.";
            }
        }
    }

    public class SendungsanfrageNichtGeplantException : SendungsanfrageException
    {
        public SendungsanfrageNichtGeplantException(int saNr)
            : base(saNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "Sendungsanfrage " + this.SaNr + " wurde nicht geplant.";
            }
        }
    }

    public class SendungsanfrageNichtAngenommenException : SendungsanfrageException
    {
        public SendungsanfrageNichtAngenommenException(int saNr)
            : base(saNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "Sendungsanfrage " + this.SaNr + " wurde nicht angenommen.";
            }
        }
    }
}
