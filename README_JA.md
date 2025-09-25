# CsSqlite
 Extremely fast, robust, and lightweight SQLite bindings for .NET and Unity

[![NuGet](https://img.shields.io/nuget/v/CsSqlite.svg)](https://www.nuget.org/packages/CsSqlite)
[![Releases](https://img.shields.io/github/release/nuskey8/CsSqlite.svg)](https://github.com/nuskey8/CsSqlite/releases)
[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[English](./README.md) | 日本語

![benchmark](./docs/images/img-benchmark.png)

CsSqliteはC#で構築された、非常に高速かつ軽量なSQLiteバインディングです。[Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/?tabs=net-cli)(EFCore SQLiteの基盤となるパッケージ)などと同等のAPIを備えながら、入念にチューニングされた実装により高いパフォーマンスを発揮します。

## 特徴

* `Microsoft.Data.Sqlite`と同等の扱いやすいAPI
* `Span<T>`や`Unsafe`の活用によるハイパフォーマンスな実装
* 追加のメモリ割り当てなし、ゼロアロケーションを達成
* 依存関係なし
* [Cysharp/csbindgen](https://github.com/Cysharp/csbindgen)による堅牢なバインディング生成
* Unity(Mono、IL2CPP)をサポート

## インストール

### NuGet packages

CsSqliteを利用するには.NET Standard2.1以上が必要です。パッケージはNuGetから入手できます。

### .NET CLI

```ps1
dotnet add package CsSqlite
```

### Package Manager

```ps1
Install-Package CsSqlite
```

### Unity

Unityの場合、Package Managerからのインストールが可能です。

1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力する

```
https://github.com/nuskey8/CsSqlite.git?path=src/CsSqlite.Unity/Assets/CsSqlite.Unity
```

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記

```json
{
    "dependencies": {
        "com.nuskey.sqlite3.unity": "https://github.com/nuskey8/CsSqlite.git?path=src/CsSqlite.Unity/Assets/CsSqlite.Unity"
    }
}
```

CsSqlite.Unityは以下のプラットフォームに対応しています。

| プラットフォーム | アーキテクチャ          | サポート    | 備考 |
| ---------------- | ----------------------- | ----------- | ---- |
| Windows          | x64                     | ✅           |      |
|                  | x86                     | ✅           |      |
|                  | arm64                   | ✅           |      |
| macOS            | x64                     | ✅           |      |
|                  | arm64  (Apple Silicon)  | ✅           |      |
|                  | Universal (x64 + arm64) | ✅           |      |
| Linux            | x64                     | ✅ (未検証)  |      |
|                  | arm64                   | ✅  (未検証) |      |
| iOS              | arm64                   | ✅           |      |
|                  | x64                     | ✅           |      |
| Android          | arm64                   | ✅           |      |

## クイックスタート

```cs
using CsSqlite;

// SqliteConnectionをOpen
using var connection = new SqliteConnection("example.db");
connection.Open();

// SQLの実行
connection.ExecuteNonQuery("""
CREATE TABLE IF NOT EXISTS user (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    age INTEGER NOT NULL,
    name TEXT NOT NULL
);
""");

// UTF-8テキストを受け入れるオーバーロード
connection.ExecuteNonQuery("""
INSERT INTO user (id, name, age)
VALUES (1, 'Alice', 18),
       (2, 'Bob', 32),
       (3, 'Charlie', 25);
"""u8);

// readerを作成
using var reader = connection.ExecuteReader("""
SELECT name
FROM user
""");

// Read() / GetXXX(column)で値を読み取る
while (reader.Read())
{
    Console.WriteLine($"{reader.GetString(0)}!");
}
```

## SqliteCommand

同じクエリを再利用するには`SqliteCommand`を利用します。

```cs
using var command = connection.CreateCommand("""
SELECT name
FROM user
""");

using var reader = command.ExecuteReader();
```

`SqliteCommand`にはパラメータを追加することも可能です。

```cs
using var command = conn.CreateCommand("INSERT INTO t(val) VALUES($foo);");
command.Parameters.Add("$foo", "foo");
command.ExecuteNonQuery();
```

## 例外処理

実行中に何らかのエラーが発生した場合は`SqliteException`をスローします。これをcatchすることで例外処理が可能です。

```cs
try
{
    // ...
}
catch (SqliteException ex)
{
    Console.WriteLine(ex.ErrorCode);
    Console.WriteLine(ex.Message);
}
```

## ライセンス

このライブラリは[MITライセンス](LICENSE)の下で提供されています。