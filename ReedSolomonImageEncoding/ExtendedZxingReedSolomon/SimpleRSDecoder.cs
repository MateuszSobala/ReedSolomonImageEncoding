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
            GenericGFPoly remainder = null;
            var iteratorTemp = new int[received.Length];
            Array.Copy(received, iteratorTemp, received.Length);
            var tempInfo = new GenericGFPoly(_field, iteratorTemp);

            while (count <= dataBytes)
            {
                remainder = tempInfo.divide(generator)[1];

                if (remainder.Coefficients.Count(coeff => coeff != 0) <= ecBytes / 2)
                {
                    errorsCorrected = true;
                    break;
                }

                count++;
                Array.Copy(received, 0, iteratorTemp, count, received.Length - count);
                Array.Copy(received, received.Length - count, iteratorTemp, 0, count);
                tempInfo = new GenericGFPoly(_field, iteratorTemp);
            }

            if (!errorsCorrected)
            {
                return false;
            }
            if (count == 0)
            {
                return true;
            }

            var correctedInfo = tempInfo.addOrSubtract(remainder);

            var missingZerosCorrection = received.Length - correctedInfo.Coefficients.Length;
            correctedInfo = correctedInfo.multiplyByMonomial(missingZerosCorrection, 1);
            correctedInfo = correctedInfo.shiftLeft(count - missingZerosCorrection);

            missingZerosCorrection = received.Length - correctedInfo.Coefficients.Length;
            correctedInfo = correctedInfo.multiplyByMonomial(missingZerosCorrection, 1);

            Array.Copy(correctedInfo.Coefficients, 0, received, missingZerosCorrection, correctedInfo.Coefficients.Length - missingZerosCorrection);
            Array.Copy(correctedInfo.Coefficients, correctedInfo.Coefficients.Length - missingZerosCorrection, received, 0, missingZerosCorrection);

            return true;
        }

        public bool LongDecode(int[] received, int ecBytes)
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
            GenericGFPoly remainder = null;
            var iteratorTemp = new int[received.Length];
            Array.Copy(received, 0, iteratorTemp, 0, received.Length);
            var tempInfo = new GenericGFPoly(_field, iteratorTemp);

            while (count <= dataBytes)
            {
                remainder = tempInfo.longDivide(generator)[1];

                if (remainder.Coefficients.Count(coeff => coeff != 0) <= ecBytes / 2)
                {
                    errorsCorrected = true;
                    break;
                }

                count++;
                Array.Copy(received, 0, iteratorTemp, count, received.Length - count);
                Array.Copy(received, received.Length - count, iteratorTemp, 0, count);
                tempInfo = new GenericGFPoly(_field, iteratorTemp);
            }

            if (!errorsCorrected)
            {
                return false;
            }
            if (count == 0)
            {
                return true;
            }

            var correctedInfo = tempInfo.addOrSubtract(remainder);

            var missingZerosCorrection = received.Length - correctedInfo.Coefficients.Length;
            correctedInfo = correctedInfo.multiplyByMonomial(missingZerosCorrection, 1);
            correctedInfo = correctedInfo.shiftLeft(count - missingZerosCorrection);

            missingZerosCorrection = received.Length - correctedInfo.Coefficients.Length;
            correctedInfo = correctedInfo.multiplyByMonomial(missingZerosCorrection, 1);

            Array.Copy(correctedInfo.Coefficients, 0, received, missingZerosCorrection, correctedInfo.Coefficients.Length - missingZerosCorrection);
            Array.Copy(correctedInfo.Coefficients, correctedInfo.Coefficients.Length - missingZerosCorrection, received, 0, missingZerosCorrection);

            return true;
        }
    }
}