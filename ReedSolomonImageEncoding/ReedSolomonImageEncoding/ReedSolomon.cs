using System.Threading;
using System.Threading.Tasks;
using ExtendedZxingReedSolomon;
using ZXing.Common.ReedSolomon;
using GenericGF = ZXing.Common.ReedSolomon.GenericGF;

namespace ReedSolomonImageEncoding
{
    public class ReedSolomon
    {
        private readonly int _messageLength;
        private readonly int _informationLength;
        private readonly int _correctionLength;
        private readonly ReedSolomonEncoder _reedSolomonEncoder;
        private readonly ReedSolomonDecoder _reedSolomonDecoder;
        private readonly SimpleRSDecoder _simpleRsDecoder;

        public ReedSolomon()
        {
            var galoisField = GenericGF.DATA_MATRIX_FIELD_256;
            _messageLength = galoisField.Size - 1;
            _correctionLength = 32;
            _informationLength = _messageLength - _correctionLength;
            _reedSolomonEncoder = new ReedSolomonEncoder(galoisField);
            _reedSolomonDecoder = new ReedSolomonDecoder(galoisField);
            _simpleRsDecoder = new SimpleRSDecoder();
        }

        public ReedSolomon(int correctionBytes)
        {
            var galoisField = GenericGF.DATA_MATRIX_FIELD_256;
            _messageLength = galoisField.Size - 1;
            _correctionLength = correctionBytes;
            _informationLength = _messageLength - _correctionLength;
            _reedSolomonEncoder = new ReedSolomonEncoder(galoisField);
            _reedSolomonDecoder = new ReedSolomonDecoder(galoisField);
            _simpleRsDecoder = new SimpleRSDecoder();
        }

        public ReedSolomon(GenericGF galoisField, int correctionBytes)
        {
            _messageLength = galoisField.Size - 1;
            _correctionLength = correctionBytes;
            _informationLength = _messageLength - _correctionLength;
            _reedSolomonEncoder = new ReedSolomonEncoder(galoisField);
            _reedSolomonDecoder = new ReedSolomonDecoder(galoisField);
            _simpleRsDecoder = new SimpleRSDecoder();
        }

        public int[] EncodeRawBytesArray(int[] data)
        {
            var length = data.Length*_messageLength/_informationLength;
            length += (255 - (length%255))%255;

            var modifiedData = new int[length];
            var processedBytes = 0;

            for (var i = 0; i < data.Length; i += _informationLength)
            {
                var tempData = new int[_messageLength];

                var remainder = (data.Length - i < _informationLength) ? data.Length - i : _informationLength;

                for (var j = 0; j < remainder; j++)
                {
                    tempData[j] = data[i + j];
                }

                for (var j = remainder; j < tempData.Length; j++)
                {
                    tempData[j] = 0;
                }

                _reedSolomonEncoder.encode(tempData, _correctionLength);

                remainder = remainder >= _informationLength ? _messageLength : modifiedData.Length - processedBytes;

                for (var j = 0; j < remainder; j++)
                {
                    modifiedData[processedBytes + j] = tempData[j];
                }
                processedBytes += _messageLength;
            }

            return modifiedData;
        }

        public void DecodeRawBytesArray(int[] modifiedData, int[]data)
        {
            var processedBytes = 0;

            for (var i = 0; i < modifiedData.Length; i += _messageLength)
            {
                var tempData = new int[_messageLength];

                var remainder = (modifiedData.Length - i < _messageLength) ? modifiedData.Length - i : _messageLength;

                for (var j = 0; j < remainder; j++)
                {
                    tempData[j] = modifiedData[i + j];
                }

                _reedSolomonDecoder.decode(tempData, _correctionLength);

                remainder = remainder >= (data.Length - processedBytes)
                    ? (data.Length - processedBytes)
                    : _informationLength;

                for (var j = 0; j < remainder; j++)
                {
                    data[processedBytes + j] = tempData[j];
                }
                processedBytes += _informationLength;
            }
        }

        public void SimplyDecodeRawBytesArray(int[] modifiedData, int[] data)
        {
            var processedBytes = 0;

            for (var i = 0; i < modifiedData.Length; i += _messageLength)
            {
                var tempData = new int[_messageLength];

                var remainder = (modifiedData.Length - i < _messageLength) ? modifiedData.Length - i : _messageLength;

                for (var j = 0; j < remainder; j++)
                {
                    tempData[j] = modifiedData[i + j];
                }

                _simpleRsDecoder.Decode(tempData, _correctionLength);

                remainder = remainder >= (data.Length - processedBytes)
                    ? (data.Length - processedBytes)
                    : _informationLength;

                for (var j = 0; j < remainder; j++)
                {
                    data[processedBytes + j] = tempData[j];
                }
                processedBytes += _informationLength;
            }
        }
    }
}
