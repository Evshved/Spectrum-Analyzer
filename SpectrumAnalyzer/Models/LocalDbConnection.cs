using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Models
{
    public partial class LocalDbConnection : DbContext
    {
        public LocalDbConnection()
            : base("name=LocalDbConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }

        public virtual DbSet<Spectrums> Spectrums { get; set; }
    }
}
