using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Util.Common.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    public class FrachtfuehrerDTO : DTOType<FrachtfuehrerDTO>, ICanConvertToEntity<Frachtfuehrer>
    {
        public virtual int FrfNr { get; set; }

        public FrachtfuehrerDTO()
        {
        }

        public virtual Frachtfuehrer ToEntity()
        {
            Frachtfuehrer frf = new Frachtfuehrer();
            frf.FrfNr = this.FrfNr;
            return frf;
        }
    }
}
