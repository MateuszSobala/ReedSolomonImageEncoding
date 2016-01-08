using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReedSolomonImageEncoding;

namespace RSTests
{
    public class OptimalTests : IRsTests
    {
        private readonly IProducerConsumerCollection<IList<int>> _results = new ConcurrentQueue<IList<int>>();
        private readonly IList<string> _imageFileNames = new List<string>
        {
            /*"sandwich",
            "rgbImage",*/
            "lenaColor",
            "lenaGray"
        };
        public const string Folder = "Results";
        public const string CsvExtension = ".csv";
        public const string BmpExtension = ".bmp";
        private Bitmap _originalImage0;
        private Bitmap _originalImage1;
        private readonly IList<Params> _paramses = new List<Params>();
        private readonly IList<int> _errorMeasureValues = new List<int>
        {
            1/*, 2, 5, 10, 16, 20*/
        };
        private readonly IList<int> _errorMeasurePercentageValues = new List<int>
        {
            1/*, 2, 5, 10*/
        };
        private readonly IList<int> _correctionBytesCounts = new List<int>
        {
            10/*, 16, 24, 32, 40*/
        };
        private readonly IList<ErrorProviderType> _errorProviderTypes = new List<ErrorProviderType>
        {
            ErrorProviderType.GroupErrorsForEveryBlock,
            ErrorProviderType.SingleErrorsForEveryBlock
        };
        private readonly IList<ErrorProviderType> _errorPercentageProviderTypes = new List<ErrorProviderType>
        {
            ErrorProviderType.ErrorsWithProbability,
            ErrorProviderType.PercentageOfErrors,
        };

        public void Initialize()
        {
            var orderNo = 1;
            /*foreach (var decoderType in Enum.GetValues(typeof(DecoderType)))
            {
                foreach (var errorProviderType in _errorProviderTypes)
                {
                    foreach (var errorMeasureValue in _errorMeasureValues)
                    {
                        foreach (var correctionBytesCount in _correctionBytesCounts)
                        {
                            _paramses.Add(
                                new Params(
                                    errorMeasureValue,
                                    correctionBytesCount,
                                    (DecoderType) decoderType,
                                    errorProviderType,
                                    orderNo++)
                                );
                        }
                    }
                }
            }*/

            foreach (var decoderType in Enum.GetValues(typeof(DecoderType)))
            {
                foreach (var errorProviderType in _errorPercentageProviderTypes)
                {
                    foreach (var errorMeasureValue in _errorMeasurePercentageValues)
                    {
                        foreach (var correctionBytesCount in _correctionBytesCounts)
                        {
                            _paramses.Add(
                                new Params(
                                    errorMeasureValue,
                                    correctionBytesCount,
                                    (DecoderType)decoderType,
                                    errorProviderType,
                                    orderNo++)
                                );
                        }
                    }
                }
            }
        }

        private int _iterator;
        private IList<int> _fileSizes; 

        public void Perform()
        {
            var fileOrder = 0;
            _fileSizes = new List<int>();
            foreach (var imageFileName in _imageFileNames)
            {
                var filePath = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory);
                filePath.Append(imageFileName);
                filePath.Append(0);
                filePath.Append(BmpExtension);
                var fileStream = new FileStream(filePath.ToString(), FileMode.Open, FileAccess.Read);
                using (fileStream)
                {
                    _originalImage0 = new Bitmap(fileStream);
                    _fileSizes.Add(_originalImage0.Height*_originalImage0.Width);
                }
                fileStream.Close();

                filePath = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory);
                filePath.Append(imageFileName);
                filePath.Append(1);
                filePath.Append(BmpExtension);
                fileStream = new FileStream(filePath.ToString(), FileMode.Open, FileAccess.Read);
                using (fileStream)
                {
                    _originalImage1 = new Bitmap(fileStream);
                }
                fileStream.Close();

                var order = fileOrder;
                Parallel.ForEach(_paramses, new ParallelOptions { MaxDegreeOfParallelism = 2 }, currentParams =>
                {
                    Interlocked.Increment(ref _iterator);
                    ProcessImageTransmissionSimulation(currentParams, _iterator, order);
                });
                fileOrder++;
            }
        }

        private void ProcessImageTransmissionSimulation(Params parms, int iterator, int fileOrder)
        {
            var errorMeasureValue = parms.ErrorMeasureValue;
            var correctionBytesCount = parms.CorrectionBytesCount;
            var decoderType = parms.RsDecoderType;
            var errorProviderType = parms.RsErrorProviderType;
            var orderNo = parms.OrderNo;
            const int blockSize = 256;
            var tries = 0;

            object clonedImage = null;
            while (clonedImage == null && tries<5)
            {
                try
                {
                    clonedImage = iterator%2 == 0 ? _originalImage0.Clone() : _originalImage1.Clone();
                }
                catch (InvalidOperationException ioe)
                {
                }
                finally
                {
                    tries++;
                }
            }
            
            var image = (Bitmap)clonedImage;
            var stopwatch = new Stopwatch();
            var reedSolomon = new ReedSolomon(correctionBytesCount);
            var data = ImageProcessing.GetRawBytesFromRGBImage(image);

            stopwatch.Reset();
            stopwatch.Start();
            var modifiedData = reedSolomon.EncodeRawBytesArray(data);
            stopwatch.Stop();

            var errorsCount = 0;
            switch (errorProviderType)
            {
                case ErrorProviderType.ErrorsTotalCount:
                    errorsCount = ErrorProvider.FillInErrors(modifiedData, errorMeasureValue);
                    break;
                case ErrorProviderType.PercentageOfErrors:
                    errorsCount = ErrorProvider.FillInPercentageOfErrors(modifiedData, errorMeasureValue);
                    break;
                case ErrorProviderType.SingleErrorsForEveryBlock:
                    errorsCount = ErrorProvider.FillInErrorsForEveryBlock(modifiedData, errorMeasureValue, blockSize);
                    break;
                case ErrorProviderType.ErrorsWithProbability:
                    errorsCount = ErrorProvider.FillInErrorsWithProbability(modifiedData, (double)errorMeasureValue/100);
                    break;
                case ErrorProviderType.GroupErrorsForEveryBlock:
                    errorsCount = ErrorProvider.FillInGroupErrorsForEveryBlock(modifiedData, errorMeasureValue, blockSize);
                    break;
            }
            
            stopwatch.Start();
            if (decoderType.Equals(DecoderType.Extended))
            {
                reedSolomon.DecodeRawBytesArray(modifiedData, data);
            }
            else
            {
                reedSolomon.SimplyDecodeRawBytesArray(modifiedData, data);
            }
            stopwatch.Stop();

            var processedImage = ImageProcessing.GetRGBImageFromRawBytes(image, data);

            var diffCount = 0;
            try
            {
                Bitmap diffImage;
                diffCount = ImageProcessing.Compare(processedImage, image, out diffImage);
            }
            catch (InvalidOperationException ioe)
            {
                var singleResultException = new List<int>
                {
                    errorMeasureValue,
                    (int)errorProviderType,
                    correctionBytesCount,
                    (int)decoderType,
                    0,
                    0,
                    0,
                    orderNo,
                    fileOrder
                };

                _results.TryAdd(singleResultException);
                return;
            }

            var singleResult = new List<int>
            {
                errorMeasureValue,
                (int)errorProviderType,
                correctionBytesCount,
                (int)decoderType,
                errorsCount,
                (int)stopwatch.ElapsedMilliseconds,
                diffCount,
                orderNo,
                fileOrder
            };
            _results.TryAdd(singleResult);
        }

        public void SaveResults()
        {
            var resultFileName = string.Format(@"{0}/OptimalTests{1}", Folder, CsvExtension);
            var fs = new StreamWriter(resultFileName, false);
            fs.WriteLine("OrderNo;FileName;ErrorsValue;ErrorProviderType;CorrectionBytesCount;DecoderType;ProvidedErrorsCount;ErrorsAfterDecoding;Time [ms];FileSize [pix]");

            foreach (var result in _results.ToArray())
            {
                var s = string.Format(@"{7};{8};{0:0.##};{1};{2};{3};{4};{5};{6};{9}", result[1] == (int)ErrorProviderType.ErrorsWithProbability ? (double)result[0] / 100 : result[0], (ErrorProviderType)result[1], result[2], (DecoderType)result[3], result[4], result[6], result[5], result[7], _imageFileNames[result[8]], _fileSizes[result[8]]);
                fs.WriteLine(s);
            }
            fs.Close();
        }
    }
}
