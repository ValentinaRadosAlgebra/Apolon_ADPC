using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrations
{
    public class Migration
    {
        public string Name { get; set; }
        public string UpSql { get; set; }
        public string DownSql { get; set; }
    }
}
