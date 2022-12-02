using System.Text.Json;
using Dapper;
using I2R.LightNews.Utilities;
using Microsoft.Data.Sqlite;

namespace I2R.LightNews;

public static class RadioIndexDb
{
    private static readonly string ConnectionString = "data source=AppData/radio-index.db";

    public static void AddEntry(RadioSeries entry) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("series")) return;
        var addOperation = db.Execute(@"insert series(name,description,canonical_url,episodes) values (@name,@description,@canonical_url,@episodes)", new {
            name = entry.Name,
            description = entry.Description,
            canonical_url = entry.CanonicalUri,
            episodes = JsonSerializer.Serialize(entry.Episodes)
        });
        if (addOperation == 0) Console.WriteLine("No rows were added");
    }
    
    public static void DeleteEntry(int id) { }

    public static RadioSeries GetEntry(int id) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("series")) return default;
        return db.QueryFirstOrDefault<RadioSeries>(@"select * from series where id=@id", new {id});
    }

    public static List<RadioSeries> GetEntries(string query, bool includeEpisodes = false) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("series")) return default;
        var selectSet = includeEpisodes ? "*" : "id,name,description,type,canonical_url";
        var result = query.HasValue()
            ? db.Query<RadioSeries>(@$"select {selectSet} from series where name like '@query' || description like '@query' order by name")
            : db.Query<RadioSeries>(@$"select {selectSet} from series order by name");
        return result.ToList();
    }

    public static void CreateIfNotExists() {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("series")) {
            db.Execute(@"
                create table series(
                    id integer primary key autoincrement,
                    name text,
                    description text,
                    type text,
                    canonical_url text,
                    episodes json
                )
            ");
        }
    }
}