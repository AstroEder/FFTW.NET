namespace FFTW.NET.Tests
{
    public class PinnedArrayTests
    {
        [Fact]
        public void Indexer_RoundTripsValues_ForOneDimensionalArray()
        {
            using var array = new PinnedArray<double>(4);
            for (int i = 0; i < array.Length; i++)
                array[i] = i * 1.5;

            for (int i = 0; i < array.Length; i++)
                Assert.Equal(i * 1.5, array[i]);
        }

        [Fact]
        public void Indexer_RoundTripsValues_ForTwoDimensionalArray()
        {
            using var array = new PinnedArray<double>(3, 4);
            for (int row = 0; row < array.GetLength(0); row++)
                for (int col = 0; col < array.GetLength(1); col++)
                    array[row, col] = row * 10 + col;

            for (int row = 0; row < array.GetLength(0); row++)
                for (int col = 0; col < array.GetLength(1); col++)
                    Assert.Equal(row * 10 + col, array[row, col]);
        }

        [Fact]
        public void GetSize_ReturnsLengthPerDimension()
        {
            using var array = new PinnedArray<double>(2, 5);
            Assert.Equal(new[] { 2, 5 }, array.GetSize());
            Assert.Equal(10, array.Length);
        }

        [Fact]
        public void Dispose_SetsIsDisposed()
        {
            var array = new PinnedArray<double>(4);
            Assert.False(array.IsDisposed);
            array.Dispose();
            Assert.True(array.IsDisposed);
        }

        [Fact]
        public void Constructor_Throws_WhenArrayElementTypeDoesNotMatch()
        {
            // float[3], not int[3]: an int[] would bind to the `params int[] lengths`
            // overload instead of the `Array array` one this test targets.
            Assert.Throws<ArgumentException>(() => new PinnedArray<double>(new float[3]));
        }
    }
}
