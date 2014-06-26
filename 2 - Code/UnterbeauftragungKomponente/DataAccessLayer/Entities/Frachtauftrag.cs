using FluentNHibernate.Mapping;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Util.Common.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    public enum FrachtauftragStatusTyp { NichtAbgeschlossen, Abgeschlossen }

    public class Frachtauftrag : ICanConvertToDTO<FrachtauftragDTO>
    {
        public virtual int FraNr { get; set; }
        public virtual DateTime PlanStartzeit { get; set; }
        public virtual DateTime PlanEndezeit { get; set; }
        public virtual int VerwendeteKapazitaetTEU { get; set; }
        public virtual int VerwendeteKapazitaetFEU { get; set; }
        public virtual byte[] Dokument { get; set; }
        public virtual FrachtfuehrerRahmenvertrag FrachtfuehrerRahmenvertrag { get; set; }
        public virtual FrachtauftragStatusTyp Status { get; protected internal set; }
        public virtual int SaNr { get; set; }

        public Frachtauftrag()
        {
        }

        public virtual void CreateDokument()
        {
            PdfDocument auftragDokument = new PdfDocument();
            
            PdfPage page = auftragDokument.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
            gfx.DrawString("Frachtauftrag!", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

            string tempFile = Path.GetTempFileName();
            auftragDokument.Save(tempFile);
            FileStream fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
            this.Dokument = new byte[fs.Length];
            fs.Read(this.Dokument, 0, System.Convert.ToInt32(fs.Length));
            fs.Close();
            File.Delete(tempFile);
        }

        public virtual FrachtauftragDTO ToDTO()
        {
            FrachtauftragDTO fraDTO = new FrachtauftragDTO();
            fraDTO.FraNr = this.FraNr;
            fraDTO.PlanStartzeit = this.PlanStartzeit;
            fraDTO.PlanEndezeit = this.PlanEndezeit;
            fraDTO.VerwendeteKapazitaetTEU = this.VerwendeteKapazitaetTEU;
            fraDTO.VerwendeteKapazitaetFEU = this.VerwendeteKapazitaetFEU;
            fraDTO.Dokument = this.Dokument;
            fraDTO.FrachtfuehrerRahmenvertrag = this.FrachtfuehrerRahmenvertrag.ToDTO();
            fraDTO.Status = this.Status;
            fraDTO.SaNr = this.SaNr;
            return fraDTO;
        }
    }

    internal class FrachtauftragMap : ClassMap<Frachtauftrag>
    {
        public FrachtauftragMap()
        {
            this.Id(x => x.FraNr);
            this.Map(x => x.PlanStartzeit);
            this.Map(x => x.PlanEndezeit);
            this.Map(x => x.VerwendeteKapazitaetTEU);
            this.Map(x => x.VerwendeteKapazitaetFEU);
            this.Map(x => x.Dokument);
            this.Map(x => x.Status);
            this.References(x => x.FrachtfuehrerRahmenvertrag);
            this.Map(x => x.SaNr);
        }
    }
}
