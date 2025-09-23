using BenchmarkDotNet.Attributes;

namespace Benchmark;

[MemoryDiagnoser]
public class Benchmarks
{
    [Benchmark(Description = "Microsoft.Data.Sqlite")]
    public void Microsoft_Data_Sqlite_Insert()
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE t(id INTEGER PRIMARY KEY, val TEXT);";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO t(val) VALUES($foo), ($bar);";
        cmd.Parameters.AddWithValue("$foo", "foo");
        cmd.Parameters.AddWithValue("$bar", "bar");
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT * FROM t;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _ = reader.GetInt32(0);
            _ = reader.GetString(1);
        }
    }

    [Benchmark(Description = "System.Data.SQLite")]
    public void System_Data_SQLite_Insert()
    {
        using var conn = new System.Data.SQLite.SQLiteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE t(id INTEGER PRIMARY KEY, val TEXT);";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO t(val) VALUES($foo), ($bar);";
        cmd.Parameters.AddWithValue("$foo", "foo");
        cmd.Parameters.AddWithValue("$bar", "bar");
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT * FROM t;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _ = reader.GetInt32(0);
            _ = reader.GetString(1);
        }
    }

    [Benchmark(Description = "sqlite-net (sqlite-net-pcl)")]
    public void SqliteNet_Insert()
    {
        using var conn = new SQLite.SQLiteConnection(":memory:");

        conn.Execute("CREATE TABLE t(id INTEGER PRIMARY KEY, val TEXT);");

        var cmd = conn.CreateCommand("INSERT INTO t(val) VALUES ($foo), ($bar);");
        cmd.Bind("$foo", "foo");
        cmd.Bind("$bar", "bar");
        cmd.ExecuteNonQuery();

        conn.Query<Item>("SELECT * FROM t;");
    }

    class Item
    {
        [SQLite.PrimaryKey]
        public int Id { get; set; }
        public string Val { get; set; } = "";
    }

    [Benchmark(Description = "CsSqlite")]
    public void CsSqlite_Insert()
    {
        using var conn = new CsSqlite.SqliteConnection(":memory:");

        conn.ExecuteNonQuery("CREATE TABLE t(id INTEGER PRIMARY KEY, val TEXT);");

        using var cmd = conn.CreateCommand("INSERT INTO t(val) VALUES ($foo), ($bar);");
        cmd.Parameters.Add("$foo"u8, "foo"u8);
        cmd.Parameters.Add("$bar"u8, "bar"u8);
        cmd.ExecuteNonQuery();

        using var reader = conn.ExecuteReader("SELECT * FROM t;");
        while (reader.Read())
        {
            _ = reader.GetInt(0);
            _ = reader.GetString(1);
        }
    }
}
