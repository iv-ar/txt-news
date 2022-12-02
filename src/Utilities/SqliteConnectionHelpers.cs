using Dapper;
using Microsoft.Data.Sqlite;

namespace I2R.LightNews.Utilities;

public static class SqliteConnectionHelpers
{
    public static bool TableExists(this SqliteConnection db, string tableName) {
        return db.QueryFirstOrDefault<string>(
            "select name from sqlite_master where type='table' and name=@tableName"
            , new {tableName}
        ).HasValue();
    }
}