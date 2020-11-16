
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Spend.Data
{

  public interface IDatabaseBootstrap
  {
    void Setup();
  }

  public class DatabaseBootstrap : IDatabaseBootstrap
  {
    private readonly DatabaseConfig _databaseConfig;

    public DatabaseBootstrap(DatabaseConfig databaseConfig)
    {
      _databaseConfig = databaseConfig;
    }

    public void Setup()
    {
      using(var connection = new SqliteConnection(_databaseConfig.Name))
      {
        var table = connection.Query<String>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'Entry';");
        var tableName = table.FirstOrDefault();
        if (!string.IsNullOrEmpty(tableName) && tableName == "Entry")
          return;

      connection.Execute(
        @"Create Table Entry (
          Name VARCHAR(100) NOT NULL,
          Description VARCHAR(1000) NULL,
          Amount DECIMAL(10,2) NULL,
          Entered DATETIME NULL,
          FromPhone NVARCHAR(20));"
        );
      }
    }
  }
}