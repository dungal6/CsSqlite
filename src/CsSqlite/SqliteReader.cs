using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static CsSqlite.NativeMethods;

namespace CsSqlite;

[StructLayout(LayoutKind.Auto)]
public unsafe readonly struct SqliteReader(SqliteConnection connection, sqlite3_stmt* stmt, bool finalizeStatement) : IDisposable
{
    public int ColumnCount
    {
        get
        {
            connection.ThrowIfDisposed();
            return sqlite3_column_count(stmt);
        }
    }

    public bool Read()
    {
        connection.ThrowIfDisposed();

        var code = sqlite3_step(stmt);
        switch (code)
        {
            case Constants.SQLITE_DONE:
                return false;
            case Constants.SQLITE_ROW:
                return true;
            case Constants.SQLITE_ERROR:
                var msg = sqlite3_errmsg(connection.db);
                var message = Marshal.PtrToStringAnsi((nint)msg);
                sqlite3_free(msg);
                throw new SqliteException(Constants.SQLITE_ERROR, message);
            case Constants.SQLITE_MISUSE:
                throw new SqliteException(Constants.SQLITE_MISUSE, "Invalid SQL statement");
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SqliteType GetColumnType(int column)
    {
        connection.ThrowIfDisposed();
        return (SqliteType)sqlite3_column_type(stmt, column);
    }

    public bool TryGetBytes(int column, Span<byte> destination, out int bytesWritten)
    {
        connection.ThrowIfDisposed();
        var ptr = sqlite3_column_blob(stmt, column);
        var count = sqlite3_column_bytes(stmt, column);

        if (destination.Length <= count)
        {
            bytesWritten = 0;
            return false;
        }

        Unsafe.CopyBlock(ref destination[0], ref Unsafe.AsRef<byte>(ptr), (uint)count);
        bytesWritten = count;
        return true;
    }

    public bool TryGetString(int column, Span<byte> utf8Destination, out int bytesWritten)
    {
        connection.ThrowIfDisposed();
        var ptr = sqlite3_column_text(stmt, column);
        var count = sqlite3_column_bytes(stmt, column);

        if (utf8Destination.Length <= count)
        {
            bytesWritten = 0;
            return false;
        }

        Unsafe.CopyBlock(ref utf8Destination[0], ref Unsafe.AsRef<byte>(ptr), (uint)count);
        bytesWritten = count;
        return true;
    }

    public bool TryGetString(int column, Span<char> destination, out int charsWritten)
    {
        connection.ThrowIfDisposed();
        var ptr = (char*)sqlite3_column_text16(stmt, column);
        var count = sqlite3_column_bytes16(stmt, column) / 2;

        if (destination.Length <= count)
        {
            charsWritten = 0;
            return false;
        }

        new Span<char>(ptr, count).CopyTo(destination);
        charsWritten = count;
        return true;
    }

    public string GetString(int column)
    {
        connection.ThrowIfDisposed();
        var ptr = (char*)sqlite3_column_text16(stmt, column);
        var count = sqlite3_column_bytes16(stmt, column) / 2;

        var buffer = ArrayPool<char>.Shared.Rent(count);
        try
        {
            new Span<char>(ptr, count).CopyTo(buffer);
            return buffer.AsSpan(0, count).ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    public string GetName(int column)
    {
        connection.ThrowIfDisposed();
        var ptr = (char*)sqlite3_column_name16(stmt, column);
        if (ptr == null) return "";

        var count = 0;
        while (ptr[count] != 0)
        {
            count++;
        }

        var buffer = ArrayPool<char>.Shared.Rent(count);
        try
        {
            new Span<char>(ptr, count).CopyTo(buffer);
            return buffer.AsSpan(0, count).ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetInt(int column)
    {
        connection.ThrowIfDisposed();
        return sqlite3_column_int(stmt, column);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long GetInt64(int column)
    {
        connection.ThrowIfDisposed();
        return sqlite3_column_int64(stmt, column);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetDouble(int column)
    {
        connection.ThrowIfDisposed();
        return sqlite3_column_double(stmt, column);
    }

    public void Dispose()
    {
        if (connection.IsDisposed) return;

        if (finalizeStatement)
        {
            sqlite3_finalize(stmt);
        }
        else
        {
            sqlite3_reset(stmt);
        }
    }
}