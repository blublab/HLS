using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Util.Common.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    public class FrachtauftragDTO : DTOType<FrachtauftragDTO>, ICanConvertToEntity<Frachtauftrag>
    {
        public virtual int FraNr { get; set; }
        public virtual DateTime PlanStartzeit { get; set; }
        public virtual DateTime PlanEndezeit { get; set; }
        public virtual int VerwendeteKapazitaetTEU { get; set; }
        public virtual int VerwendeteKapazitaetFEU { get; set; }
        public virtual byte[] Dokument { get; set; }
        public virtual FrachtfuehrerRahmenvertragDTO FrachtfuehrerRahmenvertrag { get; set; }
        public virtual FrachtauftragStatusTyp Status { get; set; }
        public virtual int SaNr { get; set; }

        public FrachtauftragDTO()
        {
        }

        public virtual Frachtauftrag ToEntity()
        {
            Frachtauftrag fra = new Frachtauftrag();
            fra.FraNr = this.FraNr;
            fra.PlanStartzeit = this.PlanStartzeit;
            fra.PlanEndezeit = this.PlanEndezeit;
            fra.VerwendeteKapazitaetTEU = this.VerwendeteKapazitaetTEU;
            fra.VerwendeteKapazitaetFEU = this.VerwendeteKapazitaetFEU;
            fra.Dokument = this.Dokument;
            fra.FrachtfuehrerRahmenvertrag = this.FrachtfuehrerRahmenvertrag.ToEntity();
            fra.Status = this.Status;
            fra.SaNr = this.SaNr;
            return fra;
        }
    }
}
