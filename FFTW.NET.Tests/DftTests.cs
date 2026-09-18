using System.Numerics;

namespace FFTW.NET.Tests
{
    /// <summary>
    /// These tests actually call into the native FFTW library, so they are the real
    /// end-to-end proof that the p/invoke layer works on the current OS/architecture
    /// (this is what fails with a <see cref="PlatformNotSupportedException"/> or a
    /// <see cref="DllNotFoundException"/> if the native library can't be resolved).
    /// They are skipped - not failed - when no native FFTW library is installed.
    /// </summary>
    public class DftTests
    {
        const double Tolerance = 1e-9;

        [SkippableOnMissingFftwFact]
        public void FFT_FollowedByIFFT_ReconstructsOriginalSignal_ComplexToComplex()
        {
            var input = new Complex[16];
            for (int i = 0; i < input.Length; i++)
                input[i] = new Complex(Math.Sin(2 * Math.PI * i / input.Length), 0);

            var output = new Complex[input.Length];

            using var pinIn = new PinnedArray<Complex>(input);
            using var pinOut = new PinnedArray<Complex>(output);

            DFT.FFT(pinIn, pinOut);
            DFT.IFFT(pinOut, pinOut);

            for (int i = 0; i < input.Length; i++)
            {
                // IFFT(FFT(x)) == length * x for FFTW's unnormalized transforms
                Complex expected = input[i] * input.Length;
                Assert.True((pinOut[i] - expected).Magnitude < Tolerance,
                    $"Mismatch at index {i}: expected {expected}, got {pinOut[i]}");
            }
        }

        [SkippableOnMissingFftwFact]
        public void FFT_FollowedByIFFT_ReconstructsOriginalSignal_RealToComplex()
        {
            var input = new double[16];
            for (int i = 0; i < input.Length; i++)
                input[i] = Math.Cos(2 * Math.PI * i / input.Length);

            using var pinIn = new PinnedArray<double>(input);
            using var complex = new FftwArrayComplex(DFT.GetComplexBufferSize(pinIn.GetSize()));
            using var pinOut = new PinnedArray<double>(input.Length);

            DFT.FFT(pinIn, complex);
            DFT.IFFT(complex, pinOut);

            for (int i = 0; i < input.Length; i++)
                Assert.True(Math.Abs(pinOut[i] - input[i] * input.Length) < Tolerance,
                    $"Mismatch at index {i}: expected {input[i] * input.Length}, got {pinOut[i]}");
        }

        [SkippableOnMissingFftwFact]
        public void FftwPlanC2C_Execute_ProducesNonTrivialOutput()
        {
            using var timeDomain = new FftwArrayComplex(32);
            using var frequencyDomain = new FftwArrayComplex(timeDomain.GetSize());

            using (var plan = FftwPlanC2C.Create(timeDomain, frequencyDomain, DftDirection.Forwards))
            {
                // Set the input after the plan was created: with the default PlannerFlags.Measure,
                // FFTW overwrites the input/output arrays while measuring candidate algorithms.
                for (int i = 0; i < timeDomain.Length; i++)
                    timeDomain[i] = i % 5;

                plan.Execute();
            }

            // A non-constant input must produce at least one non-zero frequency bin.
            bool anyNonZero = false;
            for (int i = 0; i < frequencyDomain.Length; i++)
                anyNonZero |= frequencyDomain[i] != Complex.Zero;

            Assert.True(anyNonZero);
        }
    }
}
