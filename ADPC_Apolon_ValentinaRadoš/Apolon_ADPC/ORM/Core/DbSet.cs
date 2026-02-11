using Apolon_ADPC.ORM.Attributes;
using Apolon_ADPC.ORM.Mapping;
using Apolon_ADPC.ORM.Queries;
using Npgsql;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Apolon_ADPC.ORM.Core
{
    public class DbSet<T> where T : class, new()
    {
        private readonly NpgsqlConnection _connection;
        private readonly string _table;

        private readonly UnitOfWork _uow;

        public DbSet(NpgsqlConnection connection, UnitOfWork uow)
        {
            _connection = connection;  //db conn
            _uow = uow; //Unit of Work reference
            _table = EntityMapper.GetTableName(typeof(T)); //db table name
        }

        public List<T> GetAll(string? where = null, string? orderBy = null)
        {
            var sql = $"SELECT * FROM {_table}";
            if (where != null) sql += $" WHERE {where}";
            if (orderBy != null) sql += $" ORDER BY {orderBy}"; //build sql

            using var cmd = new NpgsqlCommand(sql, _connection, _uow.Transaction); //represents SQL query
            using var reader = cmd.ExecuteReader(); //executes the query, returns a data reader

            var result = new List<T>();

            while (reader.Read())
            {
                var entity = Map(reader);
                _uow.Tracker.Track(entity);
                result.Add(entity);
            } //map rows to objects

            return result;
        }

        public List<T> GetAll(Expression<Func<T, bool>> filter) //get all where filter
        {
            var where = ExpressionSqlTranslator.Translate(filter);
            return GetAll(where);
        }

        public T? GetById(int id)
        {
            var sql = $"SELECT * FROM {_table} WHERE id=@id";
            using var cmd = new NpgsqlCommand(sql, _connection, _uow.Transaction);
            cmd.Parameters.AddWithValue("@id", id); //binds the id variable to the @id placeholder in your SQL query

            using var reader = cmd.ExecuteReader(); //execute query and get reader

            if (!reader.Read()) //check if row
                return null;

            var entity = Map(reader); //maps column values into  object
            _uow.Tracker.Track(entity); //adds the entity to the change tracker

            return entity;
        }

        public void Insert(T entity)
        {
            var columns = EntityMapper.GetInsertableColumns(typeof(T)).ToList(); //find all properties
            var names = columns.Select(c => c.Column!.Name); // list of column names, 
            var parms = columns.Select(c => "@" + c.Column!.Name);
            var sql = $"""
                INSERT INTO {_table} ({string.Join(",", names)})
                VALUES ({string.Join(",", parms)})
                RETURNING id
            """;

            using var cmd = new NpgsqlCommand(sql, _connection, _uow.Transaction);


            foreach (var c in columns)
            {
                var rawValue = c.Property.GetValue(entity); //ex. "Alice" for Name column
                object value = rawValue ?? DBNull.Value; //allows null

                if (c.Property.PropertyType.IsEnum)
                    value = (int)rawValue!; //converts enum into int

                cmd.Parameters.AddWithValue("@" + c.Column!.Name, value); //adds a parameter to the SQL command
            }

            // execute + get PK
            var idObj = cmd.ExecuteScalar();

            //validate if we got the pk
            if (idObj == null || idObj == DBNull.Value)
                throw new Exception(
                    $"Insert into {_table} did not return a generated id. Check schema and defaults."
                );

            // find PK property
            var pkProp = typeof(T).GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            if (pkProp == null)
                throw new Exception($"Entity {typeof(T).Name} has no PrimaryKey attribute");

            pkProp.SetValue(entity, Convert.ToInt32(idObj)); //sets id into entitty
        }


        public void Update(int id, T entity)
        {
            var columns = EntityMapper.GetInsertableColumns(typeof(T)); //get columns

            var sets = columns.Select(c => $"{c.Column.Name}=@{c.Column.Name}"); //set a=@a

            var sql = $"""
                UPDATE {_table}
                SET {string.Join(",", sets)}
                WHERE id=@id
            """;

            using var cmd = new NpgsqlCommand(sql, _connection, _uow.Transaction); //create comand

            foreach (var c in columns)
            {
                var rawValue = c.Property.GetValue(entity);

                object value;

                if (rawValue == null)
                {
                    value = DBNull.Value; //allows null
                }
                else if (rawValue.GetType().IsEnum)
                {
                    value = (int)rawValue; // enum to int
                }
                else
                {
                    value = rawValue;
                }

                cmd.Parameters.AddWithValue("@" + c.Column.Name, value); //add as SQL parameter (@a)
            }

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var cmd = new NpgsqlCommand(
                $"DELETE FROM {_table} WHERE id=@id", _connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteWhere(string where)
        {
            var sql = $"DELETE FROM {_table} WHERE {where}";
            using var cmd = new NpgsqlCommand(sql, _connection, _uow.Transaction);
            cmd.ExecuteNonQuery();
        }

        private T Map(NpgsqlDataReader reader) //turns db row into object
        {
            var entity = new T();

            foreach (var prop in typeof(T).GetProperties())
            {
                // handle pk
                if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                {
                    prop.SetValue(entity, Convert.ToInt32(reader["id"]));
                    continue;
                }

                // handle normal
                var col = prop.GetCustomAttribute<ColumnAttribute>();
                if (col == null) continue;

                //handle null
                var val = reader[col.Name];
                if (val == DBNull.Value) continue;

                //handle enum
                if (prop.PropertyType.IsEnum)
                    prop.SetValue(entity, Enum.ToObject(prop.PropertyType, val));
                else
                    prop.SetValue(entity, val);
            }

            return entity;
        }

    }

}
