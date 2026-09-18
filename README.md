# FFTW.NET
C#/.NET wrapper for FFTW (http://www.fftw.org/)

## Installation
Install NuGet package: https://www.nuget.org/packages/FFTW.NET

You also need the native FFTW library itself, which is not included in the package:

### Windows
Download the FFTW binaries ("libfftw3-3.dll") from http://www.fftw.org/download.html,
rename them to "libfftw3-3-x86.dll" and "libfftw3-3-x64.dll" and put them in your application directory.
FFTW.NET will automatically load the right one.

### Linux
Install the FFTW shared library via your distribution's package manager, e.g. on
Debian/Ubuntu:
```bash
sudo apt install libfftw3-double3
```
(older Debian/Ubuntu releases name this package "libfftw3-3" instead).
FFTW.NET will automatically find and load it (looked up as "libfftw3.so.3"/"libfftw3.so").

Note: multi-threaded planning (`nThreads` > 1) additionally requires the FFTW threading
functions to be available. On Linux these usually live in a separate package/library
(e.g. one providing "libfftw3_threads.so"/"libfftw3_omp.so"); without it, FFTW.NET
still works, just single-threaded.

### macOS
Install FFTW, e.g. via Homebrew:
```bash
brew install fftw
```
FFTW.NET will automatically find and load it (looked up as "libfftw3.3.dylib"/"libfftw3.dylib").

`FftwInterop.IsAvailable` reports whether a native library could be found, and can be
checked before using any of the other classes.

## Help
See TestApp/Program.cs for examples on how to use it.
Altough you should be able to use it from looking at the examples,
for a better understanding on how to use it efficiently, it is highly recommended
that you gain a little insight on how FFTW works: http://www.fftw.org/doc/index.html

## Array classes
There are three array classes which you can use to perform transformations:
* AlignedArray<T>: This class guarantees a certain memory alignment.
  This should be the default class to use.
* PinnedArray<T>: Use this class if you want to use an existing .NET array and
  want to avoid copying memory. The drawback is, that the .NET array might not
  be aligned on a 16 bytes boundary and thus FFTW cannot use SIMD.
* FftwArray<T>: This class allocates unmanaged memory using fftw_malloc.
  This class was somewhat rendered obsolete by the introduction of AlignedArray<T>.

If none of these fit your needs, you can always create your own by
implementing the IPinnedArray<T> interface.

## License
FFTW is licensed under the GNU GPL, therefore FFTW.NET as a whole adapts this license.
However, if for some reason you want to use classes/code from this project
without using FFTW, you are free to do so under the Microsoft Reciprocal License (MS-RL).