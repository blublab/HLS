using ApplicationCore.SendungKomponente.DataAccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.SendungKomponente.BusinessLogicLayer
{
    internal class SendungKomponenteBusinessLogic
    {
        private readonly SendungRepository se_REPO;

        internal SendungKomponenteBusinessLogic(SendungRepository se_REPO)
        {
            Check.Argument(se_REPO != null, "se_REPO != null");

            this.se_REPO = se_REPO;
        }
    }
}
