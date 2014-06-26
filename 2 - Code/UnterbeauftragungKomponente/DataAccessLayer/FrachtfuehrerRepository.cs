using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    internal class FrachtfuehrerRepository
    {
        private readonly IPersistenceServices persistenceService;

        public FrachtfuehrerRepository(IPersistenceServices persistenceService)
        {
            this.persistenceService = persistenceService;
        }

        public void Save(Frachtfuehrer frf)
        {
            persistenceService.Save(frf);
        }

        public Frachtfuehrer FindByFrfNr(int frfNr)
        {
            return persistenceService.GetById<Frachtfuehrer, int>(frfNr);
        }
    }
}
