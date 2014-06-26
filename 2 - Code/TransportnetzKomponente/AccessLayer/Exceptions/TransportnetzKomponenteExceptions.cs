using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.TransportnetzKomponente.AccessLayer
{
    public abstract class LokationException : Exception
    {
        public long LokNr { get; private set; }

        public LokationException(long lokNr)
        {
            this.LokNr = lokNr;
        }

        public abstract string Meldung { get; }
    }

    public class LokationNichtGefundenException : LokationException
    {
        public LokationNichtGefundenException(long lokNr)
            : base(lokNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "Lokation " + this.LokNr + " wurde nicht gefunden.";
            }
        }
    }
}
