using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedZxingReedSolomon
{
    public class SimpleRSDecoder
    {
        private readonly GenericGF _field;
        private readonly IList<GenericGFPoly> _cachedGenerators;

        public static void Main()
        {
            
        }

        public SimpleRSDecoder()
            : this(GenericGF.DATA_MATRIX_FIELD_256)
        {
        }

        public SimpleRSDecoder(GenericGF field)
        {
            _field = field;
            _cachedGenerators = new List<GenericGFPoly> { new GenericGFPoly(field, new int[] {1}) };
        }

        private GenericGFPoly BuildGenerator(int degree)
        {
            if (degree >= _cachedGenerators.Count)
            {
                var lastGenerator = _cachedGenerators[_cachedGenerators.Count - 1];
                for (int d = _cachedGenerators.Count; d <= degree; d++)
                {
                    var nextGenerator = lastGenerator.multiply(new GenericGFPoly(_field, new int[] { 1, _field.exp(d - 1 + _field.GeneratorBase) }));
                    _cachedGenerators.Add(nextGenerator);
                    lastGenerator = nextGenerator;
                }
            }
            return _cachedGenerators[degree];
        }

        public bool Decode(int[] received, int ecBytes)
        {
            if (ecBytes == 0)
            {
                throw new ArgumentException("No error correction bytes");
            }

            var dataBytes = received.Length - ecBytes;
            if (dataBytes <= 0)
            {
                throw new ArgumentException("No data bytes provided");
            }

            var errorsCorrected = false;
            var generator = BuildGenerator(ecBytes);
            var count = 0;
            var temp = new int[received.Length];
            var tempInfo = new GenericGFPoly(_field, temp);
            var remainder = new GenericGFPoly(_field, new int[ecBytes]);
            var first = received[0];

            while (count < dataBytes)
            {
                Array.Copy(received, count, temp, 0, received.Length - count);
                Array.Copy(received, 0, temp, received.Length - count, count);

                tempInfo = new GenericGFPoly(_field, temp);

                if (errorsCorrected && count==1)
                {
                    break;
                }

                remainder = tempInfo.divide(generator)[1];

                if (errorsCorrected)
                {
                    break;
                }

                if (remainder.Coefficients.Count() <= ecBytes / 2)
                {
                    errorsCorrected = true;
                }

                count++;
            }

            if (!errorsCorrected)
            {
                return false;
            }

            var correctedInfo = tempInfo.addOrSubtract(remainder);
            temp = correctedInfo.Coefficients;

            if (temp.Length < count)
                return true;

            Array.Copy(temp, temp.Length - count, received, count > 1 ? 1 : 0, count > 1 ? count - 1 : count);
            Array.Copy(temp, 0, received, count, temp.Length - count);
            if (count > 1)
                received[0] = first;

            return true;
        }
    }
}