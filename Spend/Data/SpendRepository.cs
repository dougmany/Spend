using Dapper;
using Microsoft.Data.Sqlite;
using Spend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spend.Data
{
  public interface ISpendRepository
  {
    Task Create(Entry entry);
    Task<IEnumerable<Entry>> Get();
  }

  public class SpendRepository : ISpendRepository
  {
    private readonly DatabaseConfig _databaseConfig;

    public SpendRepository(DatabaseConfig config)
    {
      _databaseConfig = config;
    }
    public async Task Create(Entry entry)
    {
      using (var connection = new SqliteConnection(_databaseConfig.Name))
      {
        await connection.ExecuteAsync("INSERT INTO Entry (Name, Description, Amount, Entered, FromPhone)" +
                "VALUES (@Name, @Description, @Amount, @Entered, @FromPhone);", entry);

      }
    }

    public async Task<IEnumerable<Entry>> Get()
    {
      using (var connection = new SqliteConnection(_databaseConfig.Name))
      {
        return await connection.QueryAsync<Entry>("SELECT rowid AS Id, Name, Description, Amount, Entered, FromPhone FROM Entry;");
      }
    }
  }
}