using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sylabs.Optimizer
{
    public static class NativeLibrary
    {
#if WINDOWS
        [DllImport("kernel32", EntryPoint = "LoadLibrary", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);
#endif

#if WINDOWS
        // Defines the location of the core native DLL.
        public const string LibraryName = "libcore.dll";
#endif

        /// <summary>
        /// Defines the calling convention for P/Invoking the native core methods.
        /// </summary>
        public const CallingConvention CallConvention = CallingConvention.Cdecl;

        /// <summary>
        /// Try to preload the library.
        /// This is useful when we want to have AnyCPU .NET and CPU-specific native code.
        /// Only available on Windows for now.
        /// </summary>
        /// <param name="libraryName">Name of the library.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static void PreloadLibrary(string libraryName)
        {
#if WINDOWS
            var systemInfo = new SYSTEM_INFO();
            GetNativeSystemInfo(out systemInfo);

            string cpu;
            if (systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE.PROCESSOR_ARCHITECTURE_ARM)
                cpu = "ARM";
            else
                cpu = IntPtr.Size == 8 ? "x64" : "x86";
            var libraryFilename = Path.Combine(Path.GetDirectoryName(typeof(NativeLibrary).Assembly.Location), cpu, libraryName);
            var result = LoadLibrary(libraryFilename);

            if (result == IntPtr.Zero)
                throw new InvalidOperationException(string.Format("Could not load native library {0} using CPU architecture {1}.", libraryName, cpu));
#endif
        }

#if WINDOWS
        private const string SYSINFO_FILE = "kernel32.dll";
#endif

#if WINDOWS
        [DllImport(SYSINFO_FILE)]
        static extern void GetNativeSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_INFO
        {
            public PROCESSOR_ARCHITECTURE processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        enum PROCESSOR_ARCHITECTURE : ushort
        {
            PROCESSOR_ARCHITECTURE_AMD64 = 9,
            PROCESSOR_ARCHITECTURE_ARM = 5,
            PROCESSOR_ARCHITECTURE_IA64 = 6,
            PROCESSOR_ARCHITECTURE_INTEL = 0,
            PROCESSOR_ARCHITECTURE_UNKNOWN = 0xffff
        }
    }
#endif
}
