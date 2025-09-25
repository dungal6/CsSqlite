using System.Runtime.CompilerServices;
using static CsSqlite.NativeMethods;

namespace CsSqlite;

public readonly unsafe ref struct SpliteParameters
{
    readonly SqliteConnection connection;
    readonly sqlite3_stmt* stmt;

    internal SpliteParameters(SqliteConnection connection, sqlite3_stmt* stmt)
    {
        this.connection = connection;
        this.stmt = stmt;
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            connection.ThrowIfDisposed();
            return sqlite3_bind_parameter_count(stmt);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        connection.ThrowIfDisposed();
        sqlite3_clear_bindings(stmt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int value)
    {
        BindParameter(Count + 1, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(long value)
    {
        BindParameter(Count + 1, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> text)
    {
        using var utf8Text = new PooledUtf8String(text);
        BindText(Count + 1, utf8Text.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> utf8Text)
    {
        BindText(Count + 1, utf8Text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> name, int value)
    {
        connection.ThrowIfDisposed();
        using var utf8Name = new PooledUtf8String(name);
        BindParameter(GetParameterIndex(utf8Name.AsSpan()), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> utf8Name, int value)
    {
        connection.ThrowIfDisposed();
        BindParameter(GetParameterIndex(utf8Name), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> name, long value)
    {
        connection.ThrowIfDisposed();
        using var utf8Name = new PooledUtf8String(name);
        BindParameter(GetParameterIndex(utf8Name.AsSpan()), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> utf8Name, long value)
    {
        connection.ThrowIfDisposed();
        BindParameter(GetParameterIndex(utf8Name), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> name, double value)
    {
        connection.ThrowIfDisposed();
        using var utf8Name = new PooledUtf8String(name);
        BindParameter(GetParameterIndex(utf8Name.AsSpan()), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> utf8Name, double value)
    {
        connection.ThrowIfDisposed();
        BindParameter(GetParameterIndex(utf8Name), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        connection.ThrowIfDisposed();
        using var utf8Name = new PooledUtf8String(name);
        BindText(GetParameterIndex(utf8Name.AsSpan()), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> utf8Name, ReadOnlySpan<byte> value)
    {
        connection.ThrowIfDisposed();
        BindText(GetParameterIndex(utf8Name), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBytes(ReadOnlySpan<byte> utf8Name, ReadOnlySpan<byte> value)
    {
        connection.ThrowIfDisposed();
        BindBlob(GetParameterIndex(utf8Name), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBytes(ReadOnlySpan<char> name, ReadOnlySpan<byte> value)
    {
        connection.ThrowIfDisposed();
        using var utf8Name = new PooledUtf8String(name);
        BindBlob(GetParameterIndex(utf8Name.AsSpan()), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int GetParameterIndex(ReadOnlySpan<byte> utf8Name)
    {
        fixed (byte* name = utf8Name)
        {
            return sqlite3_bind_parameter_index(stmt, name);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void BindParameter(int index, int value)
    {
        var code = sqlite3_bind_int(stmt, index, value);
        HandleErrorCode(code);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void BindParameter(int index, long value)
    {
        var code = sqlite3_bind_int64(stmt, index, value);
        HandleErrorCode(code);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void BindParameter(int index, double value)
    {
        var code = sqlite3_bind_double(stmt, index, value);
        HandleErrorCode(code);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void BindText(int index, ReadOnlySpan<byte> utf8Text)
    {
        fixed (byte* ptr = utf8Text)
        {
            var code = sqlite3_bind_text(stmt, index, ptr, utf8Text.Length, null);
            HandleErrorCode(code);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void BindText(int index, ReadOnlySpan<char> text)
    {
        fixed (char* ptr = text)
        {
            var code = sqlite3_bind_text16(stmt, index, ptr, text.Length * 2, null);
            HandleErrorCode(code);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void BindBlob(int index, ReadOnlySpan<byte> blob)
    {
        fixed (byte* ptr = blob)
        {
            var code = sqlite3_bind_blob(stmt, index, ptr, blob.Length, null);
            HandleErrorCode(code);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void HandleErrorCode(int code)
    {
        if (code != Constants.SQLITE_OK)
        {
            throw new SqliteException(code, "Could not add SQL parameter.");
        }
    }
}