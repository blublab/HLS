using FluentNHibernate.Mapping;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Util.Common.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    public class Frachtfuehrer : ICanConvertToDTO<FrachtfuehrerDTO>
    {
        public virtual int FrfNr { get; set; }

        public Frachtfuehrer()
        {
        }

        public virtual FrachtfuehrerDTO ToDTO()
        {
            FrachtfuehrerDTO frfDTO = new FrachtfuehrerDTO();
            frfDTO.FrfNr = this.FrfNr;
            return frfDTO;
        }
    }

    internal class FrachtfuehrerMap : ClassMap<Frachtfuehrer>
    {
        public FrachtfuehrerMap()
        {
            this.Id(x => x.FrfNr);
        }
    }
}
