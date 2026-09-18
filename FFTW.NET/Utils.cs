namespace FFTW.NET
{
    static class Utils
    {
        public static int GetTotalSize(params int[] n)
        {
            int result = 1;
            checked
            {
                foreach (var ni in n)
                    result *= ni;
            }
            return result;
        }
    }
}