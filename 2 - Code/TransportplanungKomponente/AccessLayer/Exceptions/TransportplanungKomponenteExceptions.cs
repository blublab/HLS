using ApplicationCore.AuftragKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.TransportplanungKomponente.AccessLayer
{
    public abstract class TransportplanException : Exception
    {
        public int TpNr { get; private set; }

        public TransportplanException(int tpNr)
        {
            this.TpNr = tpNr;
        }

        public abstract string Meldung { get; }
    }

    public class TransportplanNichtGefundenException : TransportplanException
    {
        public TransportplanNichtGefundenException(int tpNr)
            : base(tpNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "Transportplan " + this.TpNr + " wurde nicht gefunden.";
            }
        }
    }
}
