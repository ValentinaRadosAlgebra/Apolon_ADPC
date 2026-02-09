using Apolon_ADPC.Models;
using Migrations;
using System.Text.Json;

var cs = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password";

var runner = new MigrationRunner(cs);

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run migrate | rollback");
    return;
}

ModelSnapshot? LoadLastSnapshot()
{
    const string file = "snapshot.json";
    if (!File.Exists(file)) return null;
    var json = File.ReadAllText(file);
    return JsonSerializer.Deserialize<ModelSnapshot>(json);
}

void SaveSnapshot(ModelSnapshot snapshot)
{
    const string file = "snapshot.json";
    var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(file, json);
}

switch (args[0])
{
    case "migrate":
        {
            // Load old snapshot (from last migration or empty)
            var oldSnap = LoadLastSnapshot() ?? new ModelSnapshot();

            // Generate new snapshot from your current entity classes
            var newSnap = SnapshotGenerator.Generate(
                typeof(Patient),
                typeof(Checkups),
                typeof(Medication),
                typeof(Prescription)
            );

            // Generate migration SQL
            var (upSql, downSql) = MigrationDiff.GenerateSql(oldSnap, newSnap);

            // Create a Migration object dynamically
            var migration = new Migration
            {
                Name = $"AutoMigration_{DateTime.Now:yyyyMMdd_HHmmss}",
                UpSql = upSql,
                DownSql = downSql
            };

            // Apply migration
            runner.Apply(migration);

            // Save new snapshot for next run
            SaveSnapshot(newSnap);

            Console.WriteLine("Migrations applied");
            break;
        }


    case "rollback":
        runner.RollbackLast();
        Console.WriteLine("Rolled back last migration");
        break;
}
