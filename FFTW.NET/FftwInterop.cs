#region Copyright and License
/*
This file is part of FFTW.NET, a wrapper around the FFTW library
for the .NET framework.
Copyright (C) 2017 Tobias Meyer

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
#if !NETSTANDARD2_0
using System.Reflection;
#endif

namespace FFTW.NET
{
    public enum DftDirection : int
    {
        Forwards = -1,
        Backwards = 1
    }

    [Flags]
    public enum PlannerFlags : uint
    {
        Default = Measure,

        /// <summary>
        /// <see cref="Measure"/> tells FFTW to find an optimized plan by actually
        /// computing several FFTs and measuring their execution time.
        /// Depending on your machine, this can take some time (often a few seconds).
        /// </summary>
        Measure = (0U),

        /// <summary>
        /// <see cref="Exhaustive"/> is like <see cref="Patient"/>,
        /// but considers an even wider range of algorithms,
        /// including many that we think are unlikely to be fast,
        /// to produce the most optimal plan but with a substantially increased planning time. 
        /// </summary>
        Exhaustive = (1U << 3),

        /// <summary>
        /// <see cref="Patient"/> is like <see cref="Measure"/>,
        /// but considers a wider range of algorithms and often produces
        /// a “more optimal” plan (especially for large transforms),
        /// but at the expense of several times longer planning time
        /// (especially for large transforms). 
        /// </summary>
        Patient = (1U << 5),

        /// <summary>
        /// <see cref="Estimate"/> specifies that,
        /// instead of actual measurements of different algorithms,
        /// a simple heuristic is used to pick a (probably sub-optimal) plan quickly.
        /// With this flag, the input/output arrays are not overwritten during planning.
        /// </summary>
        Estimate = (1U << 6),

        /// <summary>
        /// <see cref="WisdomOnly"/> is a special planning mode in which
        /// the plan is only created if wisdom is available for the given problem,
        /// and otherwise a <c>null</c> plan is returned. This can be combined
        /// with other flags, e.g. '<see cref="WisdomOnly"/> | <see cref="Patient"/>'
        /// creates a plan only if wisdom is available that was created in
        /// <see cref="Patient"/> or <see cref="Exhaustive"/> mode.
        /// The <see cref="WisdomOnly"/> flag is intended for users who need to
        /// detect whether wisdom is available; for example, if wisdom is not
        /// available one may wish to allocate new arrays for planning so that
        /// user data is not overwritten. 
        /// </summary>
        WisdomOnly = (1U << 21)
    }

    public static partial class FftwInterop
    {
#if !NETSTANDARD2_0
        // Logical library name used by the p/invoke declarations in FftwInterop.g.cs.
        // The actual file is resolved for the current OS/architecture by ResolveNativeLibrary.
        internal const string NativeLibraryName = "fftw3";
#endif

        static readonly Version _version = GetVersionAndInitialize();

        public static Version Version => _version;

        public static bool IsAvailable => _version != null;

        /// <summary>
        /// Whether the loaded native FFTW library exports the threading functions
        /// (<see cref="fftw_init_threads"/>/<see cref="fftw_plan_with_nthreads"/>).
        /// On Linux these typically live in a separate shared library
        /// (e.g. "libfftw3_threads.so"/"libfftw3_omp.so") which is not loaded by this class,
        /// unlike the official Windows binaries which bundle them into the main DLL.
        /// </summary>
        internal static bool IsThreadingAvailable { get; private set; }

        internal static object Lock
        {
            get
            {
                if (!IsAvailable)
                    throw new InvalidOperationException($"{nameof(FftwInterop.IsAvailable)} is false.");
                return _version;
            }
        }

        public delegate void WriteCharHandler(byte c, IntPtr ptr);

        static Version GetVersionAndInitialize()
        {
#if !NETSTANDARD2_0
            NativeLibrary.SetDllImportResolver(typeof(FftwInterop).Assembly, ResolveNativeLibrary);
#endif

            string version;
            try { version = GetVersion(); }
            catch (DllNotFoundException) { return null; }

            // On Linux, threading support ("fftw_init_threads") typically lives in a separate
            // shared library (e.g. "libfftw3_threads.so"/"libfftw3_omp.so") that is not loaded
            // by default, unlike the official Windows binaries which bundle it into the main DLL.
            // Its absence does not mean FFTW itself is unavailable, only that
            // FftwInterop.fftw_plan_with_nthreads cannot be used.
            try
            {
                fftw_init_threads();
                IsThreadingAvailable = true;
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }

            return new Version(version);
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Maps the logical <see cref="NativeLibraryName"/> to the actual FFTW shared library
        /// for the current OS, trying the conventional names in turn. This allows a single
        /// p/invoke declaration to work across Windows, Linux and macOS (including on architectures
        /// other than x86/x64, e.g. ARM64), instead of hard-coding Windows-style DLL names.
        /// </summary>
        static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != NativeLibraryName)
                return IntPtr.Zero;

            foreach (string candidate in GetCandidateNativeLibraryNames())
            {
                if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out IntPtr handle))
                    return handle;
            }

            return IntPtr.Zero;
        }

        static IEnumerable<string> GetCandidateNativeLibraryNames()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Historically, this library expected the caller to supply the official FFTW
                // Windows binaries renamed to disambiguate the two bitnesses, since both can
                // otherwise not be shipped side by side in the same output directory.
                yield return RuntimeInformation.ProcessArchitecture == Architecture.X86
                    ? "libfftw3-3-x86.dll"
                    : "libfftw3-3-x64.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Names used by the "libfftw3-3"/"libfftw3-double3" distro packages
                // (e.g. `apt install libfftw3-3`) and by "libfftw3-dev".
                yield return "libfftw3.so.3";
                yield return "libfftw3.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Name used e.g. by the Homebrew "fftw" formula.
                yield return "libfftw3.3.dylib";
                yield return "libfftw3.dylib";
            }
        }
#endif

        public static string fftw_export_wisdom_to_string()
        {
            // We cannot use the fftw_export_wisdom_to_string function here
            // because we have no way of releasing the returned memory.
            StringBuilder sb = new StringBuilder();
            FftwInterop.WriteCharHandler writeChar = (c, ptr) => sb.Append((char)c);
            FftwInterop.fftw_export_wisdom(writeChar, IntPtr.Zero);
            return sb.ToString();
        }

        static string GetVersion()
        {
            const string VersionPrefix = "fftw-";
            const byte WhiteSpace = (byte)' ';
            byte[] prefix = Encoding.ASCII.GetBytes(VersionPrefix);
            int i = 0;
            StringBuilder sb = new StringBuilder();
            FftwInterop.WriteCharHandler writeChar = (c, ptr) =>
                {
                    if (i < 0)
                        return;

                    if (i == VersionPrefix.Length)
                    {
                        if (c == WhiteSpace)
                            i = -1;
                        else
                            sb.Append((char)c);
                    }
                    else if (c == prefix[i])
                        i++;
                    else
                        i = 0;
                };
            // This is only called on initialization, so no synchronization/lock is required
            FftwInterop.fftw_export_wisdom(writeChar, IntPtr.Zero);
            return sb.ToString();
        }
    }
}
