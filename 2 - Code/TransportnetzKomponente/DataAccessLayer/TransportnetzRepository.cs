using Neo4jClient;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ApplicationCore.TransportnetzKomponente.DataAccessLayer
{
    internal class TranspornetzRepository
    {
        private GraphClient gc = null;

        public TranspornetzRepository()
        {
            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["Neo4j"];
            Contract.Assert(connectionSettings != null, "A Neo4j connection setting needs to be defined in the App.config.");
            string connectionString = connectionSettings.ConnectionString;
            Contract.Assert(connectionString != null, "A Neo4j connection string needs to be defined in the App.config.");
            
            gc = new GraphClient(new Uri(connectionString));
            gc.Connect();

            gc.CreateIndex("LokationIdx", new IndexConfiguration() { Provider = IndexProvider.lucene, Type = IndexType.fulltext }, IndexFor.Node);
        }

        public void Save(Lokation lokation)
        {
            Contract.Requires(lokation != null);

            IEnumerable<IndexEntry> indexEntries = new[]
                {
                    new IndexEntry("LokationIdx")
                    {
                        { "Name", lokation.Name }
                    }
                };
            NodeReference<Lokation> nodeRef = gc.Create(
                lokation,
                new IRelationshipAllowingParticipantNode<Lokation>[0],
                indexEntries);
            lokation.LokNr = nodeRef.Id;
        }

        public void Save(Transportbeziehung tb)
        {
            Contract.Requires(tb != null);

            RelationshipReference relRef = gc.CreateRelationship(new NodeReference<Lokation>(tb.Start.LokNr), tb);
            tb.TbNr = relRef.Id;
        }

        public Lokation FindByLokNr(long lokNr)
        {
            Contract.Requires(lokNr >= 0);

            Node<Lokation> nodeLok = gc.Get<Lokation>(new NodeReference(lokNr));
            if (nodeLok == null)
            {
                return null;
            }

            Lokation lok = nodeLok.Data;
            lok.LokNr = lokNr;

            return lok;
        }

        public Transportbeziehung FindByTbNr(long tbNr)
        {
            Contract.Requires(tbNr >= 0);

            RelationshipReference tbRef = new RelationshipReference(tbNr);
            RelationshipInstance<Transportbeziehung> tbRelShipInstance = gc.Get<Transportbeziehung>(tbRef);
            if (tbRelShipInstance == null)
            {
                return null;
            }

            Transportbeziehung tb = tbRelShipInstance.Data;
            tb.TbNr = tbNr;

            var result
                 = gc.Cypher
                     .Start("r", tbRef)
                     .Match("(a)-[r]->(b)")
                     .Return((a, b, r) => new
                     {
                         A = a.Node<Lokation>(),
                         B = b.Node<Lokation>()
                     })
                     .Results
                     .Single();

            tb.Start = FindByLokNr(result.A.Reference.Id);
            tb.Ziel  = FindByLokNr(result.B.Reference.Id);

            return tb;
        }

        public void DeleteTransportnetz()
        {
            IEnumerable<Node<Lokation>> nodes = gc.ExecuteGetAllNodesGremlin<Lokation>("g.V", null);

            foreach (Node<Lokation> node in nodes)
            {
                gc.Delete(node.Reference, DeleteMode.NodeAndRelationships);
            }
        }

        public List<List<Transportbeziehung>> GeneriereAllePfadeVonBis(long startLokation, long zielLokation)
        {
            Contract.Requires(startLokation >= 0);
            Contract.Requires(zielLokation >= 0);

            ICypherFluentQuery<PathsResult> query
                = gc.Cypher
                    .Start(new Dictionary<string, object>
                        {
                            { "s", new NodeReference(startLokation) },
                            { "z", new NodeReference(zielLokation)  }
                        })
                    .Match("p = s-[:TRANSPORTBEZIEHUNG*]->z")
                    .Return<PathsResult>("p");
            IEnumerable<PathsResult> res = query.Results;
            List<List<Transportbeziehung>> ltp = new List<List<Transportbeziehung>>();
            foreach (PathsResult pr in res)
            {
                List<Transportbeziehung> tpf = new List<Transportbeziehung>();
                foreach (string relationshipUri in pr.Relationships)
                {
                    System.Uri uri = new System.Uri(relationshipUri);
                    string relationshipId = uri.Segments.Last();
                    Transportbeziehung tb = FindByTbNr(long.Parse(relationshipId));
                    tpf.Add(tb);
                }
                ltp.Add(tpf);
            }

            return ltp;
        }

        public IList<LokationDTO> Select()
        {
            ////TODO:Verify correct working -flo
            IEnumerable<Node<Lokation>> nodes = gc.ExecuteGetAllNodesGremlin<Lokation>("g.V", null);
            IList<LokationDTO> lokList = new List<LokationDTO>();
            foreach (Node<Lokation> node in nodes)
            {
                lokList.Add(node.Data.ToDTO());
            }

            return lokList;
        }
    }
}
