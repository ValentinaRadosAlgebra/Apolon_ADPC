using Apolon_ADPC.Models;
using Apolon_ADPC.ORM.Core;
using Apolon_ADPC.ORM.Queries;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connString = builder.Configuration.GetConnectionString("ConnectionApolon");

if (string.IsNullOrEmpty(connString))
    throw new InvalidOperationException("Connection string 'ConnectionApolon' is not set");

// run ddl on strart
//using (var conn = new NpgsqlConnection(connString))
//{
//    conn.Open();

//    using var cmd = conn.CreateCommand();

//    cmd.CommandText = SqlGenerator.GenerateCreateTable<Patient>();
//    cmd.ExecuteNonQuery();

//    cmd.CommandText = SqlGenerator.GenerateCreateTable<Medication>();
//    cmd.ExecuteNonQuery();

//    cmd.CommandText = SqlGenerator.GenerateCreateTable<Checkups>();
//    cmd.ExecuteNonQuery();

//    cmd.CommandText = SqlGenerator.GenerateCreateTable<Prescription>();
//    cmd.ExecuteNonQuery();

//}

// ORM usage only AFTER schema exists
builder.Services.AddScoped<UnitOfWork>(_ =>
    new UnitOfWork(connString)
);

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    }); //for the enums


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
