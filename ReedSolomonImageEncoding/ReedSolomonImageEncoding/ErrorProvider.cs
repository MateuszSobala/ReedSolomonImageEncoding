using System;

namespace ReedSolomonImageEncoding
{
    public static class ErrorProvider
    {
        public static int FillInErrorsForEveryBlock(int[] byteArray, int errorsCount, int blockSize)
        {
            if (errorsCount > blockSize)
            {
                throw new ArgumentOutOfRangeException("errorsCount", "Errors count must be less than block size!");
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < byteArray.Length; i += blockSize)
            {
                var arrayRemainder = (byteArray.Length - i < blockSize) ? byteArray.Length - i : blockSize;

                for (var j = 0; j < errorsCount; j++)
                {
                    byteArray[i + random.Next(arrayRemainder)] = random.Next(256);
                }
            }
            return (int)(errorsCount * Math.Ceiling((decimal)byteArray.Length / blockSize));
        }

        public static int FillInGroupErrorsForEveryBlock(int[] byteArray, int errorsCount, int blockSize)
        {
            if (errorsCount > blockSize)
            {
                throw new ArgumentOutOfRangeException("errorsCount", "Errors count must be less than block size!");
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < byteArray.Length; i += blockSize)
            {
                var arrayRemainder = ((byteArray.Length - i < blockSize) ? byteArray.Length - i : blockSize) - errorsCount;

                if (arrayRemainder < 0)
                    break;

                var groupStartIndex = random.Next(arrayRemainder);

                for (var j = groupStartIndex; j < groupStartIndex+errorsCount; j++)
                {
                    byteArray[i + j] = random.Next(256);
                }
            }
            return (int)(errorsCount * Math.Ceiling((decimal)byteArray.Length / blockSize));
        }

        public static int FillInErrors(int[] byteArray, int errorsCount)
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
            return errorsCount;
        }

        public static int FillInPercentageOfErrors(int[] byteArray, int errorsPercentage)
        {
            if (errorsPercentage < 0 || errorsPercentage > 100)
            {
                throw new ArgumentOutOfRangeException("errorsPercentage", "Percentage of errors must be between 0 and 100!");
            }

            var errorsCount = byteArray.Length*errorsPercentage/100;

            FillInErrors(byteArray, errorsCount);
            return errorsCount;
        }

        public static int FillInErrorsWithProbability(int[] byteArray, double probability)
        {
            if (probability > 1 || probability < 0)
            {
                throw new ArgumentOutOfRangeException("probability", "Probability must be between 0.0 and 1.0!");
            }
            var errorsCount = 0;
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < byteArray.Length; i++)
            {
                if (random.NextDouble() <= probability)
                {
                    byteArray[i] = random.Next(256);
                    errorsCount++;
                }
            }

            return errorsCount;
        }
    }
}
