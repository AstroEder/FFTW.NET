using System.Numerics;

namespace FFTW.NET.Tests
{
    public class AlignedArrayTests
    {
        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        public void Pointer_IsAlignedToRequestedBoundary(int alignment)
        {
            using var array = new AlignedArrayDouble(alignment, 128);
            Assert.Equal(0L, array.Pointer.ToInt64() % alignment);
        }

        [Fact]
        public void Indexer_RoundTripsComplexValues()
        {
            using var array = new AlignedArrayComplex(16, 8);
            for (int i = 0; i < array.Length; i++)
                array[i] = new Complex(i, -i);

            for (int i = 0; i < array.Length; i++)
                Assert.Equal(new Complex(i, -i), array[i]);
        }

        [Fact]
        public void Indexer_RoundTripsValues_ForTwoDimensionalArray()
        {
            using var array = new AlignedArrayDouble(16, 4, 8);
            for (int row = 0; row < array.GetLength(0); row++)
                for (int col = 0; col < array.GetLength(1); col++)
                    array[row, col] = row * 100 + col;

            for (int row = 0; row < array.GetLength(0); row++)
                for (int col = 0; col < array.GetLength(1); col++)
                    Assert.Equal(row * 100 + col, array[row, col]);
        }

        [Fact]
        public void Dispose_SetsIsDisposed()
        {
            var array = new AlignedArrayDouble(16, 4);
            Assert.False(array.IsDisposed);
            array.Dispose();
            Assert.True(array.IsDisposed);
        }
    }
}
