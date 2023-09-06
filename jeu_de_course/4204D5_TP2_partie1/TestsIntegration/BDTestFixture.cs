using Microsoft.EntityFrameworkCore;
using SussyKart_Partie1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsIntegration
{
    public class BDTestFixture
    {
        private const string ConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=TP2_SussyKart;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";

        public TP2_SussyKartContext CreateContext()
        {
            return new TP2_SussyKartContext(new DbContextOptionsBuilder<TP2_SussyKartContext>().UseSqlServer(ConnectionString).Options);
        }
    }
}
