namespace FFTW.NET.Tests
{
    /// <summary>
    /// These tests exercise the native library resolution logic itself. They must pass
    /// on every OS/architecture regardless of whether an FFTW native library is actually
    /// installed: probing for the library and reporting "not available" is expected behavior,
    /// only crashing (e.g. with <see cref="PlatformNotSupportedException"/> on non-x86/x64
    /// architectures) is a bug.
    /// </summary>
    public class FftwInteropTests
    {
        [Fact]
        public void IsAvailable_DoesNotThrow()
        {
            // On any supported OS (Windows, Linux, macOS) probing for the native library
            // must never throw, whether or not the library is actually installed.
            bool isAvailable = FftwInterop.IsAvailable;
            Assert.Equal(isAvailable, FftwInterop.Version != null);
        }

        [SkippableOnMissingFftwFact]
        public void Version_IsPopulated_WhenNativeLibraryAvailable()
        {
            Assert.NotNull(FftwInterop.Version);
        }
    }
}
