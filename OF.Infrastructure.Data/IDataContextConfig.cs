using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OF.Infrastructure.Data
{
    public interface IDataContextConfig
    {
        public string ConnectionString { get; set; }
        Driver Dialect { get; set; }
        bool OwnConnection { get; }
    }
}
