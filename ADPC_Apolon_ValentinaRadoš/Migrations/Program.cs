using Apolon_ADPC.Models;
using Microsoft.Extensions.Configuration;
using Migrations;
using System.Text.Json;

//var cs = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password";
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var cs = config.GetConnectionString("ConnectionApolon");

if (string.IsNullOrWhiteSpace(cs))
    throw new Exception("Connection string 'ConnectionApolon' not found");

var runner = new MigrationRunner(cs);

// Ensure the migrations table exists first
runner.EnsureMigrationsTable();

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

(string upPath, string downPath) SaveMigrationFiles(string name, string upSql, string downSql)
{
    const string folder = "AutoMigrations";
    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

    string upFile = Path.Combine(folder, $"{name}_up.sql");
    string downFile = Path.Combine(folder, $"{name}_down.sql");

    File.WriteAllText(upFile, upSql);
    File.WriteAllText(downFile, downSql);

    return (upFile, downFile);
}

switch (args[0].ToLower())
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

            if (string.IsNullOrWhiteSpace(upSql))
            {
                Console.WriteLine("No schema changes detected. No migration created.");
                return;
            }

            // Save SQL files in AutoMigrations/
            var migrationName = $"AutoMigration_{DateTime.Now:yyyyMMdd_HHmmss}";
            var (upFilePath, downFilePath) = SaveMigrationFiles(migrationName, upSql, downSql);

            //create migration object
            var migration = new Migration
            {
                Name = migrationName,
                UpFile = upFilePath,
                DownFile = downFilePath
            };

            // Apply migration -> after migration object exists
            runner.Apply(migration);

            // Save snapshot for next run
            SaveSnapshot(newSnap);

            Console.WriteLine("Migrations applied");
            break;
        }

    case "rollback":
        runner.RollbackLast();
        if (File.Exists("snapshot.json"))
            File.Delete("snapshot.json");
        Console.WriteLine("Rolled back last migration");
        break;

    default:
        Console.WriteLine("Unknown command. Use migrate or rollback.");
        break;
}
