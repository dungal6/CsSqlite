namespace CsSqlite;

public sealed class SqliteException(int errorCode, string? message) : Exception(message)
{
    public int ErrorCode { get; } = errorCode;
}