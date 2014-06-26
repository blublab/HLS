using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Util.BackgroundQueue;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace ApplicationCore.TransportplanungKomponente.BusinessLogicLayer
{
    internal class TransportplanungKomponenteBusinessLogic
    {
        private readonly TransportplanRepository tp_REPO;
        private readonly ITransactionServices transactionService;
        private readonly IAuftragServicesFürTransportplanung auftragServices;
        private readonly IUnterbeauftragungServicesFürTransportplanung unterbeauftragungServices;
        private readonly ITransportnetzServicesFürTransportplanung transportnetzServices;
        private readonly ITimeServices timeServices;
        private readonly BackgroundQueue planungsQueue;

        public TransportplanungKomponenteBusinessLogic(TransportplanRepository tp_REPO, ITransactionServices transactionService, IAuftragServicesFürTransportplanung auftragServices, IUnterbeauftragungServicesFürTransportplanung unterbeauftragungServices, ITransportnetzServicesFürTransportplanung transportnetzServices, ITimeServices timeServices)
        {
            Check.Argument(tp_REPO != null, "tp_REPO != null");
            Check.Argument(transactionService != null, "transactionService != null");
            Check.Argument(auftragServices != null, "auftragServices != null");
            Check.Argument(unterbeauftragungServices != null, "unterbeauftragungsServices != null");
            Check.Argument(transportnetzServices != null, "transportnetzServices != null");
            Check.Argument(timeServices != null, "timeServices != null");

            this.tp_REPO = tp_REPO;
            this.transactionService = transactionService;
            this.auftragServices = auftragServices;
            this.unterbeauftragungServices = unterbeauftragungServices;
            this.transportnetzServices = transportnetzServices;
            this.timeServices = timeServices;
            this.planungsQueue = new BackgroundQueue();
        }

        internal ITransportplanungJob StarteTransportplanungAsync(int saNr)
        {
            // TODO check ob TP schon in Ausführung?
            Contract.Requires(saNr > 0);

            TransportplanungJob job = new TransportplanungJob(saNr);
            Task planungsTask = planungsQueue.QueueTask(() =>
            {
                job.Status = TransportplanungJobStatusTyp.Gestartet;
                try
                {
                    transactionService.ExecuteTransactional(
                        () =>
                        {
                            LöscheTransportpläne(saNr);
                        });
                    transactionService.ExecuteTransactional(
                        () =>
                        {
                            Sendungsanfrage sa = this.auftragServices.FindSendungsanfrageEntity(saNr);
                            Contract.Assume(sa != null);

                            ErzeugeTransportpläneZuSendungsanfrage(sa, job);
                            this.auftragServices.UpdateSendungsanfrageStatus(saNr, SendungsanfrageStatusTyp.Geplant);
                        });
                    job.Status = (job.Meldungen.Count == 0) ? TransportplanungJobStatusTyp.BeendetOk : TransportplanungJobStatusTyp.BeendetNok;
                }
                catch (Exception)
                {
                    job.Status = TransportplanungJobStatusTyp.BeendetNok;
                    throw;
                }
            });
            job.Task = planungsTask;

            return job;
        }

        /// <summary>
        /// Erzeugt Frachteinheiten (TEU, FEU) für Sendungspositionen.
        /// </summary>
        /// <pre>sps.Count > 0</pre>
        private List<Frachteinheit> ErzeugeFrachteinheitenFür(IList<Sendungsposition> sps, TransportplanungJob job)
        {
            Contract.Requires(sps.Count > 0);

            List<Frachteinheit> lfe = new List<Frachteinheit>();
            
            // TODO: besserer Algorithmus nötig; Volumen der Fracht wird hier nicht beachtet, sondern nur das Gewicht.
            decimal restKapazität = 0m;
            Frachteinheit fe = null;  
            foreach (Sendungsposition sp in sps)
            {
                if (sp.Bruttogewicht > FEU.MAXZULADUNG_TONS)
                {
                    // Ware zu schwer; kann nicht transportiert werden.
                    job.Meldungen.Add(new TransportplanungMeldung(
                        TransportplanungMeldungTag.FrachteinheitenBildungNichtMöglich,
                        "Das Bruttogewicht der Sendungsposition " + sp.SendungspositionsNr + " ist zu hoch."));
                    return new List<Frachteinheit>();
                }

                // Falls noch Restkapazität vorhanden und nicht die erste zu erstellende Frachteinheit
                if (restKapazität - sp.Bruttogewicht < 0 && fe != null)
                {
                    lfe.Add(fe);
                    fe = null;
                }

                if (fe == null)
                {
                    // Neue Frachteinheit anlegen, Typ (TEU, FEU) ist abhängig von Gewicht der Sendungsposition
                    if (sp.Bruttogewicht > TEU.MAXZULADUNG_TONS)
                    {
                        fe = new Frachteinheit(FrachteinheitTyp.FEU);
                        restKapazität = FEU.MAXZULADUNG_TONS;
                    }
                    else
                    {
                        fe = new Frachteinheit(FrachteinheitTyp.TEU);
                        restKapazität = TEU.MAXZULADUNG_TONS;
                    }
                }
                
                fe.Sendungspositionen.Add(sp.SendungspositionsNr);
                restKapazität = restKapazität - sp.Bruttogewicht;
            }

            // evtl. letzte erstellte Frachteinheit noch hinzunehmen
            if (fe.Sendungspositionen.Count > 0)
            {
                lfe.Add(fe);
            }

            Contract.Ensures(lfe.Count > 0);

            return lfe;
        }

        private void ErzeugeTransportpläneZuSendungsanfrage(Sendungsanfrage sa, TransportplanungJob job)
        {
            Contract.Requires(sa != null);

            List<Frachteinheit> fe = null;
            fe = ErzeugeFrachteinheitenFür(sa.Sendungspositionen, job);
            if (job.Abort)
            {
                return;
            }

            List<List<Transportbeziehung>> p = transportnetzServices.GeneriereAllePfadeVonBis(sa.StartLokation, sa.ZielLokation);
            if (p.Count == 0)
            {
                job.Meldungen.Add(new TransportplanungMeldung(
                    TransportplanungMeldungTag.KeinWegVorhanden,
                    "Kein Weg von " + sa.StartLokation + " bis " + sa.ZielLokation + " vorhanden."));
                job.Abort = true;

                return;
            }

            List<Transportplan> ltp = new List<Transportplan>();
            foreach (List<Transportbeziehung> pfad in p)
            {
                List<TransportplanSchritt> tps = ErzeugePlanFür(pfad, fe, sa.AbholzeitfensterStart, sa.AbholzeitfensterEnde);

                if (tps != null)
                {
                    Transportplan tp = new Transportplan();
                    tp.TransportplanSchritte = tps;
                    tp.Frachteinheiten = fe.Select(_fe => (Frachteinheit)_fe.Clone()).ToList();
                    tp.SaNr = sa.SaNr;
                    tp.UpdateStatus(TransportplanStatusTyp.Geplant);

                    ltp.Add(tp);
                }
            }

            Contract.Ensures(ltp != null);

            // Füge die erzeugten Transportpläne dem Repository hinzu
            ltp.ForEach((tp) => { this.tp_REPO.Save(tp); });
        }

        private List<TransportplanSchritt> ErzeugePlanFür(
            List<Transportbeziehung> pfad,
            List<Frachteinheit> fe,
            DateTime frühesterStart,
            DateTime spätesterStart)
        {
            Contract.Requires(pfad != null);
            Contract.Requires(fe != null);

            // am Ende des Weges angelangt: fertig
            if (pfad.Count == 0)
            {
                return new List<TransportplanSchritt>();
            }
            
            Transportbeziehung tb = pfad.First();

            // prüfe für alle möglichen Abfahrtszeiten, ob Transport möglich (d.h. Kapazitäten in gegebenen Vertrag vorhanden)
            List<FrachtfuehrerRahmenvertrag> lfrv = unterbeauftragungServices.FindGültigFür(tb.TbNr, frühesterStart, spätesterStart);

            // falls keine gültigen Vertäge für dieses Teilstück vorhanden -> keine Fortsetzung des Weges möglich
            if (lfrv.Count == 0)
            {
                return null;
            }

            // TODO mehrere Verträge einbeziehen
            if (lfrv.Count > 1)
            {
                throw new InvalidOperationException("Es darf während eines Zeitraums nur ein FRV gültig sein. Zeitraum:" + frühesterStart.ToString() + "-" + spätesterStart.ToString() + ", Transportbeziehung: " + tb.ToString());
            }
            FrachtfuehrerRahmenvertrag frv = lfrv[0];

            // prüfe für alle möglichen Abfahrtszeiten, ob Transport möglich (d.h. Kapazitäten in gegebenen Vertrag vorhanden)
            List<DateTime> abfahrtsZeitenAbsolut = frv.GetAbfahrtszeitenAbsolut(frühesterStart, DateTime.Compare(spätesterStart, frv.GueltigkeitBis) < 0 ? spätesterStart : frv.GueltigkeitBis);
            foreach (DateTime abfahrtsZeit in abfahrtsZeitenAbsolut)
            {
                List<TransportAktivitaet> lta = this.tp_REPO.FindZuVertragUndStartzeit(frv.FrvNr, abfahrtsZeit);
                int restkapazitätTEU = frv.KapazitaetTEU - lta.Aggregate(0, (_sum, _ta) => { return _sum + _ta.VerwendeteKapazitaetTEU + (2 * _ta.VerwendeteKapazitaetFEU); });

                // falls keine ausreichende Kapazität mehr vorhanden -> nächste Abfahrtszeit testen
                if (fe.FindAll((_fe) => { return _fe.FraeTyp == FrachteinheitTyp.TEU; }).Count
                    + (2 * fe.FindAll((_fe) => { return _fe.FraeTyp == FrachteinheitTyp.FEU; }).Count) > restkapazitätTEU)
                {
                    continue;
                }

                // es ist noch Platz für den Transport
                TransportAktivitaet ta = new TransportAktivitaet();
                ta.FrachtfuehrerRahmenvertrag = frv.FrvNr;
                ta.FuerTransportAufTransportbeziehung = tb.TbNr;
                ta.PlanStartzeit = abfahrtsZeit;
                ta.PlanEndezeit = ta.PlanStartzeit + frv.Zeitvorgabe;
                ta.SpaetesterStart = DateTime.Compare(spätesterStart, frv.GueltigkeitBis) < 0 ? spätesterStart : frv.GueltigkeitBis;
                ta.WartezeitAnStart = abfahrtsZeit - frühesterStart;
                
                ta.VerwendeteKapazitaetTEU = fe.FindAll((_fe) => { return _fe.FraeTyp == FrachteinheitTyp.TEU; }).Count;
                ta.VerwendeteKapazitaetFEU = fe.FindAll((_fe) => { return _fe.FraeTyp == FrachteinheitTyp.FEU; }).Count;

                ta.Kosten = frv.KostenFix + (ta.VerwendeteKapazitaetTEU * frv.KostenProTEU)
                    + (ta.VerwendeteKapazitaetFEU * frv.KostenProFEU)
                    + ((int)ta.WartezeitAnStart.TotalHours * tb.Start.LagerKostenProStunde);

                // Weiterplanung auf diesem Weg ausgehend von der Annahme, dass es keine Verzögerungen gibt
                List<TransportplanSchritt> tps = ErzeugePlanFür(
                    pfad.Skip(1).ToList(), 
                    fe,
                    ta.PlanEndezeit,
                    ta.PlanEndezeit.Add(tb.Ziel.MaxLagerZeit));

                if (tps == null)
                {
                    return null;
                }
                else
                {
                    tps.Insert(0, ta);
                    return tps;
                }
            }

            // keine Fortführung des Weges hier möglich -> fail
            return null;
        }

        public void FühreTransportplanAus(Transportplan gewählterPlan, Sendungsanfrage sa)
        {
            Contract.Requires(gewählterPlan != null);
            Contract.Requires(sa != null);

            this.auftragServices.UpdateSendungsanfrageStatus(sa.SaNr, SendungsanfrageStatusTyp.InAusfuehrung);
            gewählterPlan.UpdateStatus(TransportplanStatusTyp.InAusfuehrung);

            if (this.timeServices.Now > gewählterPlan.AbholungAm)
            {
                gewählterPlan.Meldungen.Add(new TransportplanMeldungTyp(this.timeServices.Now, TransportplanMeldungTyp.MeldungTag.AbholzeitUeberschritten, "Ausführung des Plans " + gewählterPlan.TpNr + " unterbrochen, da die Abholzeit " + sa.AbholzeitfensterEnde + " überschritten wurde."));
                this.auftragServices.UpdateSendungsanfrageStatus(sa.SaNr, SendungsanfrageStatusTyp.Unterbrochen);
                return;
            }

            // beauftrage alle (Teil-)Transporte und sichere die Frachtauftragsnummer in der Transportaktivität
            foreach (TransportAktivitaet ta in gewählterPlan.TransportplanSchritte)
            {
                ta.Frachtauftrag = unterbeauftragungServices.BeauftrageTransport(ta.FrachtfuehrerRahmenvertrag, ta.PlanStartzeit, ta.PlanEndezeit, ta.VerwendeteKapazitaetTEU, ta.VerwendeteKapazitaetFEU, sa.SaNr);
            }

            // lösche alle anderen mit derselben Sendungsanfrage assoziierten alternativen Transportpläne (dies gibt die geplanten Ressourcen frei)
            List<Transportplan> ltp = this.tp_REPO.SucheZuSendungsanfrage(gewählterPlan.SaNr);
            ltp.ForEach((plan) =>
                {
                    if (plan.TpNr != gewählterPlan.TpNr)
                    {
                        this.tp_REPO.Delete(plan);
                    }
                });
        }

        public void LöscheTransportpläneAsync(int saNr)
        {
            Task löschTask = planungsQueue.QueueTask(() =>
            {
                transactionService.ExecuteTransactional(
                   () =>
                   {                 
                       LöscheTransportpläne(saNr);
                   });
            });
            löschTask.Wait();
        }

        private void LöscheTransportpläne(int saNr)
        {
            Contract.Requires(saNr > 0);

            List<Transportplan> ltp = this.tp_REPO.SucheZuSendungsanfrage(saNr);
            List<Frachteinheit> lfrae = null;
            ltp.ForEach((plan) =>
            {
                Contract.Requires(plan.Status == TransportplanStatusTyp.Geplant);
                if (lfrae != null)
                {
                    lfrae = plan.Frachteinheiten.ToList();
                }
                this.tp_REPO.Delete(plan);
            });

            if (lfrae != null)
            {
                lfrae.ForEach((frae) =>
                    {
                        this.tp_REPO.Delete(frae);
                    });
            }
        }

        public void UpdateTransportplanstatus(Transportplan gewählterPlan, TransportplanStatusTyp tpStTyp)
        {
            Contract.Requires(gewählterPlan != null);

            gewählterPlan.Status = tpStTyp;
        }
    }
}
