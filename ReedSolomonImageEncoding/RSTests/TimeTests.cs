﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReedSolomonImageEncoding;

namespace RSTests
{

    public class TimeTests : IRsTests
    {
        private readonly IProducerConsumerCollection<IList<int>> _results = new ConcurrentQueue<IList<int>>();
        private const string ImageFileName0 = "sandwich0.bmp";
        private const string ImageFileName1 = "sandwich1.bmp";
        public const string Folder = "Results";
        public const string CsvExtension = ".csv";
        private readonly Bitmap _originalImage0;
        private readonly Bitmap _originalImage1;
        private readonly IList<Params> _paramses = new List<Params>();
        private readonly IList<int> _errorMeasureValues = new List<int>
        {
            1, 2, 5, 10
        };
        private readonly IList<int> _correctionBytesCounts = new List<int>
        {
            10, 16, 24, 32, 40
        };

        private readonly int _fileSize;

        public TimeTests()
        {
            var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + ImageFileName0, FileMode.Open, FileAccess.Read);
            using (fileStream)
            {
                _originalImage0 = new Bitmap(fileStream);
                _fileSize = _originalImage0.Height * _originalImage0.Width;
            }
            fileStream.Close();

            fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + ImageFileName1, FileMode.Open, FileAccess.Read);
            using (fileStream)
            {
                _originalImage1 = new Bitmap(fileStream);
            }
            fileStream.Close();
        }

        public void Initialize()
        {
            var orderNo = 1;
            foreach (var decoderType in Enum.GetValues(typeof(DecoderType)))
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
                                orderNo++)
                                );
                    }
                }
            }
        }

        private int _iterator;

        public void Perform()
        {
            Parallel.ForEach(_paramses, new ParallelOptions {MaxDegreeOfParallelism = 2}, currentParams =>
            {
                Interlocked.Increment(ref _iterator);
                ProcessImageTransmissionSimulation(currentParams, _iterator);
            });
        }

        private void ProcessImageTransmissionSimulation(Params parms, int iterator)
        {
            var errorMeasureValue = parms.ErrorMeasureValue;
            var correctionBytesCount = parms.CorrectionBytesCount;
            var decoderType = parms.RsDecoderType;
            var orderNo = parms.OrderNo;

            var tries = 0;

            object clonedImage = null;
            while (clonedImage == null && tries < 5)
            {
                try
                {
                    clonedImage = iterator % 2 == 0 ? _originalImage0.Clone() : _originalImage1.Clone();
                }
                catch (InvalidOperationException ioe)
                {
                    tries++;
                }
            }
            if (tries >= 5)
                return;
            
            var image = (Bitmap)clonedImage;
            var stopwatch = new Stopwatch();
            var reedSolomon = new ReedSolomon(correctionBytesCount);
            var data = ImageProcessing.GetRawBytesFromRGBImage(image);

            stopwatch.Reset();
            stopwatch.Start();
            var modifiedData = reedSolomon.EncodeRawBytesArray(data);
            stopwatch.Stop();

            var errorsCount = ErrorProvider.FillInPercentageOfErrors(modifiedData, errorMeasureValue);
            
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
                    (int)ErrorProviderType.PercentageOfErrors,
                    correctionBytesCount,
                    (int)decoderType,
                    0,
                    0,
                    0,
                    orderNo
                };

                _results.TryAdd(singleResultException);
                return;
            }

            var singleResult = new List<int>
            {
                errorMeasureValue,
                (int)ErrorProviderType.PercentageOfErrors,
                correctionBytesCount,
                (int)decoderType,
                errorsCount,
                (int) stopwatch.ElapsedMilliseconds,
                diffCount,
                orderNo
            };
            _results.TryAdd(singleResult);
        }

        public void SaveResults()
        {
            var resultFileName = string.Format(@"{0}/TimeTests{1}", Folder, CsvExtension);
            var fs = new StreamWriter(resultFileName, false);
            fs.WriteLine("OrderNo;ErrorsValue;ErrorProviderType;CorrectionBytesCount;DecoderType;ProvidedErrorsCount;ErrorsAfterDecoding;Time [ms];FileSize [pix]");

            foreach (var result in _results.ToArray())
            {
                var s = string.Format(@"{7};{0:0.##};{1};{2};{3};{4};{5};{6};{8}", result[1] == (int)ErrorProviderType.ErrorsWithProbability ? (double)result[0] / 100 : result[0], (ErrorProviderType)result[1], result[2], (DecoderType)result[3], result[4], result[6], result[5], result[7], _fileSize);
                fs.WriteLine(s);
            }
            fs.Close();
        }
    }
}
