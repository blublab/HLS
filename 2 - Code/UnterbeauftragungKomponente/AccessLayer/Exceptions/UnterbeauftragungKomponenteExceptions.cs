using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.UnterbeauftragungKomponente.AccessLayer
{
    public abstract class FrachtfuehrerRahmenvertragException : Exception
    {
        public int FrvNr { get; private set; }

        public FrachtfuehrerRahmenvertragException(int frvNr)
        {
            this.FrvNr = frvNr;
        }

        public abstract string Meldung { get; }
    }

    public class FrachtfuehrerRahmenvertragNichtGefundenException : FrachtfuehrerRahmenvertragException
    {
        public FrachtfuehrerRahmenvertragNichtGefundenException(int frvNr)
            : base(frvNr)
        {
        }

        public override string Meldung
        {
            get
            {
                return "FrachtfuehrerRahmenvertrag " + this.FrvNr + " wurde nicht gefunden.";
            }
        }
    }
}
