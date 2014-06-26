using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using System;
using System.Runtime.CompilerServices;
using Util.Common.Interfaces;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.BusinessLogicLayer
{
    internal class FrachtauftragDetail : DTOType<FrachtauftragDetail>
    {
        public int FaNr { get; set; }
        public int FrvNr { get; set; }
        public DateTime PlanStartzeit { get; set; }
        public DateTime PlanEndezeit { get; set; }
        public int VerwendeteKapazitaetTEU { get; set; }
        public int VerwendeteKapazitaetFEU { get; set; }
    }

    internal class UnterbeauftragungKomponenteBusinessLogic
    {
        private readonly FrachtauftragRepository fa_REPO;
        private readonly FrachtfuehrerRahmenvertragRepository frv_REPO;
        private readonly IFrachtfuehrerServicesFürUnterbeauftragung frachtfuehrerServices;

        public UnterbeauftragungKomponenteBusinessLogic(IPersistenceServices persistenceService, IFrachtfuehrerServicesFürUnterbeauftragung frachtfuehrerServices)
        {
            this.fa_REPO = new FrachtauftragRepository(persistenceService);
            this.frv_REPO = new FrachtfuehrerRahmenvertragRepository(persistenceService);
            this.frachtfuehrerServices = frachtfuehrerServices;
        }

        public int BeaufrageTransport(FrachtfuehrerRahmenvertrag frv, DateTime planStartzeit, DateTime planEndezeit, int verwendeteKapazitaetTEU, int verwendeteKapazitaetFEU, int saNr)
        {
            Frachtauftrag fra = new Frachtauftrag();
            fra.FrachtfuehrerRahmenvertrag = frv;
            fra.PlanStartzeit = planStartzeit;
            fra.PlanEndezeit = planEndezeit;
            fra.VerwendeteKapazitaetTEU = verwendeteKapazitaetTEU;
            fra.VerwendeteKapazitaetFEU = verwendeteKapazitaetFEU;
            fra.CreateDokument();
            fra.SaNr = saNr;
            this.fa_REPO.Add(fra);

            this.frachtfuehrerServices.SendeFrachtauftragAnFrachtfuehrer(fra.ToDTO());

            return fra.FraNr;
        }
    }
}
