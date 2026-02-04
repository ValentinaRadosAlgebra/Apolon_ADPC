using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrations
{
    public static class MigrationRegistry
    {
        private static readonly List<Migration> _migrations = new()
        {
            new Migration
            {
                Name = "001_initial_schema",
                UpSql = """
                    CREATE TABLE IF NOT EXISTS patients (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(255) NOT NULL,
                        surname VARCHAR(255) NOT NULL,
                        date_of_birth TIMESTAMP NOT NULL,
                        gender INT NOT NULL,
                        address VARCHAR(255),
                        phone VARCHAR(50),
                        email VARCHAR(255) UNIQUE,
                        emergency_contact VARCHAR(255),
                        profile_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS medications (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(255) NOT NULL,
                        description VARCHAR(255),
                        manufacturer VARCHAR(255)
                    );

                    CREATE TABLE IF NOT EXISTS checkups (
                        id SERIAL PRIMARY KEY,
                        patient_id INT NOT NULL REFERENCES patients(id),
                        type INT NOT NULL,
                        checkup_date TIMESTAMP NOT NULL,
                        notes VARCHAR(255),
                        diagnosis VARCHAR(255)
                    );

                    CREATE TABLE IF NOT EXISTS prescriptions (
                        id SERIAL PRIMARY KEY,
                        patient_id INT NOT NULL REFERENCES patients(id),
                        medication_id INT NOT NULL REFERENCES medications(id),
                        dosage VARCHAR(255) NOT NULL,
                        start_date TIMESTAMP NOT NULL,
                        end_date TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS migrations (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(255) UNIQUE,
                        applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );
                """,
                DownSql = """
                    DROP TABLE IF EXISTS prescriptions;
                    DROP TABLE IF EXISTS checkups;
                    DROP TABLE IF EXISTS medications;
                    DROP TABLE IF EXISTS patients;
                    DROP TABLE IF EXISTS migrations;
                """
            }
        };

        public static Migration GetByName(string name)
            => _migrations.First(m => m.Name == name);

        public static IEnumerable<Migration> All => _migrations;
    }
}
