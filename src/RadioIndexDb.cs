using System.Text.Json;
using Dapper;
using I2R.LightNews.Utilities;
using Microsoft.Data.Sqlite;

namespace I2R.LightNews;

public static class RadioIndexDb
{
    private static readonly string ConnectionString = "data source=AppData/radio-index.db";

    public static int AddSeries(RadioSeries entry) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("series")) return -1;
        return db.ExecuteScalar<int>(@"
insert into series(name,description,canonical_url,nrk_id) values (@name,@description,@canonical_url,@nrk_id);
select last_insert_rowid();", new {
            name = entry.Name,
            description = entry.Description,
            canonical_url = entry.CanonicalUrl,
            nrk_id = entry.NrkId
        });
    }

    public static int AddSeason(RadioSeason entry) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("seasons")) return -1;
        return db.ExecuteScalar<int>(@"
insert into seasons(name,description,canonical_url,nrk_id,series_id) values (@name,@description,@canonical_url,@nrk_id,@series_id);
select last_insert_rowid();", new {
            name = entry.Name,
            description = entry.Description,
            canonical_url = entry.CanonicalUrl,
            nrk_id = entry.NrkId,
            series_id = entry.SeriesId
        });
    }

    public static int AddEpisode(RadioEpisode entry) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("episodes")) return -1;
        return db.ExecuteScalar<int>(@"
insert into episodes(name,description,canonical_url,nrk_id,series_id,season_id,source_url) values (@name,@description,@canonical_url,@nrk_id,@series_id,@season_id,@source_url);
select last_insert_rowid();", new {
            name = entry.Name,
            description = entry.Description,
            canonical_url = entry.CanonicalUrl,
            nrk_id = entry.NrkId,
            series_id = entry.SeriesId,
            season_id = entry.SeasonId,
            source_url = entry.SourceUrl,
        });
    }

    public static RadioSeries GetSeriesByNrkId(string nrkId) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("series")) return default;
        return db.QueryFirstOrDefault<RadioSeries>(@"select * from series where nrk_id=@nrkId", new {nrkId});
    }

    public static RadioSeason GetSeasonByNrkId(string nrkId) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("seasons")) return default;
        return db.QueryFirstOrDefault<RadioSeason>(@"select * from seasons where nrk_id=@nrkId", new {nrkId});
    }

    public static RadioEpisode GetEpisodeByNrkId(string nrkId) {
        using var db = new SqliteConnection(ConnectionString);
        if (!db.TableExists("episodes")) return default;
        return db.QueryFirstOrDefault<RadioEpisode>(@"select * from episodes where nrk_id=@nrkId", new {nrkId});
    }

    public static List<RadioSeries> GetSeries(string query, bool includeEpisodes = false) {
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
                    nrk_id text
                )
            ");
        }

        if (!db.TableExists("seasons")) {
            db.Execute(@"
                create table seasons(
                    id integer primary key autoincrement,
                    series_id integer,
                    name text,
                    description text,
                    canonical_url text,
                    nrk_id text
                )
            ");
        }

        if (!db.TableExists("episodes")) {
            db.Execute(@"
                create table episodes(
                    id integer primary key autoincrement,
                    series_id integer,
                    season_id integer,
                    name text,
                    description text,
                    canonical_url text,
                    source_url text,
                    nrk_id text
                )
            ");
        }
    }
}