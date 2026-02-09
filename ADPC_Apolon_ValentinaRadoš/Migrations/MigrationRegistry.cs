using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrations
{
    public static class MigrationRegistry
    {
        public static IEnumerable<Migration> All
        {
            get
            {
                var folder = "Schema";
                if (!Directory.Exists(folder))
                    yield break;

                foreach (var upFile in Directory.GetFiles(folder, "*_up.sql"))
                {
                    var name = Path.GetFileName(upFile).Replace("_up.sql", "");
                    var downFile = Path.Combine(folder, $"{name}_down.sql");
                    yield return new Migration
                    {
                        Name = name,
                        UpFile = upFile,
                        DownFile = downFile
                    };
                }
            }
        }

        public static Migration GetByName(string name)
            => All.First(m => m.Name == name);
    }
}
