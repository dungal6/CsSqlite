#if NET8_0_OR_GREATER

using System.Reflection;
using System.Runtime.InteropServices;

namespace CsSqlite;

public partial class NativeMethods
{
    // https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform
    // Library path will search
    // win => __DllName, __DllName.dll
    // linux, osx => __DllName.so, __DllName.dylib

    static NativeMethods()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
    }

    static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == __DllName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var path = "runtimes/";
                var extension = ".dll";

                path += "win-";

                if (RuntimeInformation.OSArchitecture == Architecture.X86)
                {
                    path += "x86";
                }
                else if (RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    path += "x64";
                }
                else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                {
                    path += "arm64";
                }

                path += "/native/" + __DllName + extension;

                return NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, path), assembly, searchPath);
            }

            return NativeLibrary.Load(libraryName, assembly, searchPath);
        }

        return IntPtr.Zero;
    }
}

#endif