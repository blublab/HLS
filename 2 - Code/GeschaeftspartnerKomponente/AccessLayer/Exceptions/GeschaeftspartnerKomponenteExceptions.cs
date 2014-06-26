using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.GeschaeftspartnerKomponente.AccessLayer
{
    public abstract class GeschaeftspartnerException : Exception
    {
        public int GpNr { get; private set; }

        public GeschaeftspartnerException(int gpNr)
        {
            this.GpNr = gpNr;
        }

        public abstract string Meldung { get; }
    }

    public class GeschaeftspartnerNichtGefundenException : GeschaeftspartnerException
    {
        public GeschaeftspartnerNichtGefundenException(int gpNr)
            : base(gpNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "Geschaeftspartner " + this.GpNr + " wurde nicht gefunden.";
            }
        }
    }
}
