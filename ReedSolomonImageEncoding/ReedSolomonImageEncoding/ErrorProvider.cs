using System;

namespace ReedSolomonImageEncoding
{
    public static class ErrorProvider
    {
        public static void FillInErrorsForEveryBlock(int[] byteArray, int errorsCount, int blockSize)
        {
            if (errorsCount > blockSize)
            {
                throw new ArgumentOutOfRangeException("errorsCount", "Errors count must be less than block size!");
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < byteArray.Length; i += blockSize - 1)
            {
                for (var j = 0; j < errorsCount; j++)
                {
                    byteArray[i + random.Next(blockSize - 1)] = random.Next(256);
                }
            }
        }

        public static void FillInErrors(int[] byteArray, int errorsCount)
        {
            if (errorsCount > byteArray.Length)
            {
                throw new ArgumentOutOfRangeException("errorsCount", "Errors count must be less than array size!");
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < errorsCount; i++)
            {
                byteArray[random.Next(byteArray.Length)] = random.Next(256);
            }
        }

        public static void FillInPercentageOfErrors(int[] byteArray, int errorsPercentage)
        {
            if (errorsPercentage < 0 || errorsPercentage > 100)
            {
                throw new ArgumentOutOfRangeException("errorsPercentage", "Percentage of errors must be between 0 and 100!");
            }

            var errorsCount = byteArray.Length*(errorsPercentage/100);

            FillInErrors(byteArray, errorsCount);
        }

        public static void FillInErrorsWithProbability(int[] byteArray, double probability)
        {
            if (probability > 1 || probability < 0)
            {
                throw new ArgumentOutOfRangeException("probability", "Probability must be between 0.0 and 1.0!");
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < byteArray.Length; i++)
            {
                if (random.NextDouble() <= probability)
                {
                    byteArray[i] = random.Next(256);
                }
            }
        }
    }
}
