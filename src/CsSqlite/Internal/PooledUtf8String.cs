using System.Buffers;
using System.Text;

namespace CsSqlite;

internal ref struct PooledUtf8String
{
    byte[]? buffer;
    readonly int count;

    public PooledUtf8String(ReadOnlySpan<char> str)
    {
        buffer = ArrayPool<byte>.Shared.Rent(str.Length * 3);
        count = Encoding.UTF8.GetBytes(str, buffer);
    }

    public ReadOnlySpan<byte> AsSpan() => buffer.AsSpan(0, count);

    public void Dispose()
    {
        if (buffer == null) return;
        ArrayPool<byte>.Shared.Return(buffer);
    }
}