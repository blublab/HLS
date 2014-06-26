using System.Collections.Generic;
using ApplicationCore.TransportnetzKomponente;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using System.Diagnostics.Contracts;
using Common.Implementations;

namespace ApplicationCore.TransportnetzKomponente.AccessLayer
{
    public class TransportnetzKomponenteFacade : ITransportnetzServices, ITransportnetzServicesFürTransportplanung
    {
        private readonly TranspornetzRepository tn_REPO;

        public TransportnetzKomponenteFacade()
        {
            this.tn_REPO = new TranspornetzRepository();
        }

        public void CreateLokation(ref LokationDTO lokDTO)
        {
            Check.Argument(lokDTO != null, "lokDTO != null");
            Check.Argument(lokDTO.LokNr == -1, "lokDTO.LokNr == -1");

            Lokation lok = lokDTO.ToEntity();
            this.tn_REPO.Save(lok);
            lokDTO = lok.ToDTO();
        }

        public void CreateTransportbeziehung(ref TransportbeziehungDTO tbDTO)
        {
            Check.Argument(tbDTO != null, "tbDTO != null");
            Check.Argument(tbDTO.TbNr == -1, "tbDTO.TbNr == -1");

            Transportbeziehung tb = tbDTO.ToEntity();
            this.tn_REPO.Save(tb);
            tbDTO = tb.ToDTO();
        }

        public LokationDTO FindLokation(long lokNr)
        {
            Check.Argument(lokNr >= 0, "lokNr >= 0");

            Lokation lok = this.tn_REPO.FindByLokNr(lokNr);
            if (lok == null)
            {
                return null;
            }
            return lok.ToDTO();
        }

        public TransportbeziehungDTO FindTransportbeziehung(long tbNr)
        {
            Check.Argument(tbNr >= 0, "tbNr >= 0");

            Transportbeziehung tb = this.tn_REPO.FindByTbNr(tbNr);
            if (tb == null)
            {
                return null;
            }
            return tb.ToDTO();
        }

        public void DeleteTransportnetz()
        {
            this.tn_REPO.DeleteTransportnetz();
        }

        public List<List<Transportbeziehung>> GeneriereAllePfadeVonBis(long startLokation, long zielLokation)
        {
            if (this.tn_REPO.FindByLokNr(startLokation) == null)
            {
                throw new LokationNichtGefundenException(startLokation);
            }
            if (this.tn_REPO.FindByLokNr(zielLokation) == null)
            {
                throw new LokationNichtGefundenException(zielLokation);
            }

            return this.tn_REPO.GeneriereAllePfadeVonBis(startLokation, zielLokation);
        }

        public IList<LokationDTO> SelectLokationen()
        {
            return tn_REPO.Select();
        }
    }
}
