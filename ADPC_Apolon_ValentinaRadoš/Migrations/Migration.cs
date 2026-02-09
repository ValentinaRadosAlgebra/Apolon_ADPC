using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrations
{
    public class Migration
    {
        public required string Name { get; set; }
        public required string UpFile { get; set; }
        public required string DownFile { get; set; }

        public string UpSql => File.Exists(UpFile)
            ? File.ReadAllText(UpFile)
            : throw new FileNotFoundException($"Migration file not found: {UpFile}");

        public string DownSql => File.Exists(DownFile)
            ? File.ReadAllText(DownFile)
            : throw new FileNotFoundException($"Migration file not found: {DownFile}");
    }

}
