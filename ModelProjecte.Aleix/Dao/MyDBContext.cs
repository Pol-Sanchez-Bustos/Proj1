using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class MyDBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            string connectionString =
                "User Id=alumne;Password=alumne;Data Source=10.2.243.152:1521/XEPDB1;";

            optionBuilder.UseOracle(connectionString);
        }
    }
}
