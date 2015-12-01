using ZXing.Common.ReedSolomon;

namespace ReedSolomonImageEncoding
{
    class ReedSolomon
    {
        private readonly int _messageLength;
        private readonly int _informationLength;
        private readonly int _correctionLength;
        private readonly ReedSolomonEncoder _reedSolomonEncoder;
        private readonly ReedSolomonDecoder _reedSolomonDecoder;

        public ReedSolomon()
        {
            var galoisField = GenericGF.QR_CODE_FIELD_256;
            _messageLength = galoisField.Size;
            _correctionLength = 32;
            _informationLength = _messageLength - _correctionLength;
            _reedSolomonEncoder = new ReedSolomonEncoder(galoisField);
            _reedSolomonDecoder = new ReedSolomonDecoder(galoisField);
        }

        public ReedSolomon(GenericGF galoisField, int correctionBytes)
        {
            _messageLength = galoisField.Size;
            _correctionLength = correctionBytes;
            _informationLength = _messageLength - _correctionLength;
            _reedSolomonEncoder = new ReedSolomonEncoder(galoisField);
            _reedSolomonDecoder = new ReedSolomonDecoder(galoisField);
        }

        public int[] EncodeRawBytesArray(int[] data)
        {
            var modifiedData = new int[data.Length * (_messageLength - 1) / (_informationLength - 1)];
            var processedBytes = 0;

            for (var i = 0; i < data.Length; i += (_informationLength - 1))
            {
                var tempData = new int[(_messageLength - 1)];

                var remainder = (data.Length - i < (_informationLength - 1)) ? data.Length - i : (_informationLength - 1);

                for (var j = 0; j < remainder; j++)
                {
                    tempData[j] = data[i + j];
                }

                for (var j = remainder; j < tempData.Length; j++)
                {
                    tempData[j] = 0;
                }

                _reedSolomonEncoder.encode(tempData, 32);

                remainder = remainder >= (_informationLength - 1) ? (_messageLength - 1) : modifiedData.Length - processedBytes - 1;

                for (var j = 0; j < remainder; j++)
                {
                    modifiedData[processedBytes + j] = tempData[j];
                }
                processedBytes += (_messageLength - 1);
            }

            return modifiedData;
        }

        public void DecodeRawBytesArray(int[] modifiedData, int[]data)
        {
            var processedBytes = 0;

            for (var i = 0; i < modifiedData.Length; i += (_messageLength - 1))
            {
                var tempData = new int[(_messageLength - 1)];

                var remainder = (modifiedData.Length - i < (_informationLength - 1)) ? modifiedData.Length - i : (_informationLength - 1);

                for (var j = 0; j < remainder; j++)
                {
                    tempData[j] = modifiedData[i + j];
                }

                _reedSolomonDecoder.decode(tempData, _correctionLength);

                if (remainder < tempData.Length)
                {
                    remainder = (data.Length - processedBytes - 1);
                }

                for (var j = 0; j < remainder; j++)
                {
                    data[processedBytes + j] = tempData[j];
                }
                processedBytes += (_informationLength - 1);
            }
        }
    }
}
