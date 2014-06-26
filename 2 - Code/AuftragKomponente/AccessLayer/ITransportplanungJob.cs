using ApplicationCore.AuftragKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationCore.AuftragKomponente.AccessLayer
{
    public enum TransportplanungJobStatusTyp { Erzeugt, Gestartet, BeendetOk, BeendetNok }
    public enum TransportplanungMeldungTag { FrachteinheitenBildungNichtMöglich, KeinWegVorhanden }

    public class TransportplanungMeldung
    {
        public TransportplanungMeldungTag Tag { get; private set; }
        public string Meldungstext { get; private set; }

        public TransportplanungMeldung(TransportplanungMeldungTag tag, string meldungstext)
        {
            this.Tag = tag;
            this.Meldungstext = meldungstext;
        }
    }

    public interface ITransportplanungJob
    {
        int SaNr { get; }
        TransportplanungJobStatusTyp Status { get; }
        List<TransportplanungMeldung> Meldungen { get; }

        void Wait();
    }

    public class TransportplanungJob : ITransportplanungJob
    {
        public int SaNr { get; private set; }
        public TransportplanungJobStatusTyp Status { get; set; }
        public List<TransportplanungMeldung> Meldungen { get; private set; }
        public bool Abort { get; set; }
        public Task Task { get; set; }

        public TransportplanungJob(int saNr)
        {
            this.Status = TransportplanungJobStatusTyp.Erzeugt;
            this.Meldungen = new List<TransportplanungMeldung>();
            this.SaNr = SaNr;
            this.Abort = false;
        }

        public void Wait()
        {
            this.Task.Wait();
        }
    }
}
