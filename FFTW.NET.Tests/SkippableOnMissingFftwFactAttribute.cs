namespace FFTW.NET.Tests
{
    /// <summary>
    /// Marks a test that requires an actual native FFTW library to be installed
    /// (e.g. "libfftw3-3-x64.dll" on Windows, or the "libfftw3-3"/"libfftw3-dev" package
    /// on Linux). The test is skipped instead of failing when no such library can be found,
    /// so the suite stays green in environments (such as CI images) that do not have FFTW
    /// installed, while still running - and providing real coverage of the DFT logic -
    /// wherever it is present.
    /// </summary>
    public sealed class SkippableOnMissingFftwFactAttribute : FactAttribute
    {
        public SkippableOnMissingFftwFactAttribute()
        {
            if (!FftwInterop.IsAvailable)
                Skip = "FFTW native library not found on this machine.";
        }
    }
}
