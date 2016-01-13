namespace RSTests
{
    enum DecoderType
    {
        Simple,
        Extended
    }

    enum ErrorProviderType
    {
        SingleErrorsForEveryBlock,
        GroupErrorsForEveryBlock,
        ErrorsTotalCount,
        PercentageOfErrors,
        ErrorsWithProbability
    }

    class Params
    {
        public int ErrorMeasureValue;
        public int CorrectionBytesCount;
        public DecoderType RsDecoderType;
        public ErrorProviderType RsErrorProviderType;
        public int OrderNo;

        public Params(int errorMeasureValue, int correctionBytesCount, DecoderType rsDecoderType, int orderNo)
        {
            ErrorMeasureValue = errorMeasureValue;
            CorrectionBytesCount = correctionBytesCount;
            RsDecoderType = rsDecoderType;
            OrderNo = orderNo;
        }

        public Params(int errorMeasureValue, int correctionBytesCount, DecoderType rsDecoderType, ErrorProviderType rsErrorProviderType)
        {
            ErrorMeasureValue = errorMeasureValue;
            CorrectionBytesCount = correctionBytesCount;
            RsDecoderType = rsDecoderType;
            RsErrorProviderType = rsErrorProviderType;
        }

        public Params(int errorMeasureValue, int correctionBytesCount, DecoderType rsDecoderType, ErrorProviderType rsErrorProviderType, int orderNo)
        {
            ErrorMeasureValue = errorMeasureValue;
            CorrectionBytesCount = correctionBytesCount;
            RsDecoderType = rsDecoderType;
            RsErrorProviderType = rsErrorProviderType;
            OrderNo = orderNo;
        }
    }

    public interface IRsTests
    {
        void Initialize();

        void Perform();

        void SaveResults();
    }
}