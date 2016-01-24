using ReedSolomonImageEncoding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ExtendedZxingReedSolomon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SimpleRSDecoderTest
    {
        [TestMethod]
        public void ProperlyDecodesNoErrorMessage()
        {
            var reedSolomon = new ReedSolomon(32);
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i+10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            var simpleDecoder = new SimpleRSDecoder();

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyFindsErrorInAMessage()
        {
            var reedSolomon = new ReedSolomon(32);
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i+10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            for (var i = 10; i < 30; i++)
            {
                modifiedData[i] = i + 5;
            }

            var simpleDecoder = new SimpleRSDecoder();

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsFalse(response);
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessage()
        {
            var reedSolomon = new ReedSolomon(16);
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[10] += 100;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 16);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessage2()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[12] += 87;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessage3()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i + 15;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[12] += 182;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessage4()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = ((i + 50) % 255) + 1;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[1] = 1;
            modifiedData[15] = 16;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyNotCorrectsErrorInAMessage5()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = ((i + 50) % 255) + 1;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[0] = 0;
            modifiedData[170] = 170;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsFalse(response);
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessageLoop()
        {
            var random = new Random(1);

            for (var j = 0; j < 10;)
            {
                var reedSolomon = new ReedSolomon();
                var data = new int[223];

                for (var i = 0; i < 223; i++)
                {
                    data[i] = random.Next(1,255);
                }

                var modifiedData = reedSolomon.EncodeRawBytesArray(data);

                var modifIndex = random.Next(255);
                modifiedData[modifIndex] = random.Next(255);

                var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

                var response = simpleDecoder.Decode(modifiedData, 32);

                if (response)
                {
                    j++;
                    for (var i = 0; i < 223; i++)
                    {
                        Assert.AreEqual(data[i], modifiedData[i]);
                    }
                }
            }
        }

        [TestMethod]
        public void ProperlyCorrects2ErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[10] += 100;
            modifiedData[11] += 100;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects3ErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[10] += 100;
            modifiedData[11] += 100;
            modifiedData[13] += 101;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects5ErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            for (var i = 1; i < 6; i++)
            {
                modifiedData[i] += 100;
            }

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects2WithZerosErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            for (var i = 1; i < 3; i++)
            {
                modifiedData[i] += 100;
            }

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects2WithZerosErrorsInAMessage2()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            for (var i = 0; i < 2; i++)
            {
                modifiedData[i] += 100;
            }

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects2FarErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[1] += 100;
            modifiedData[11] += 100;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects2ErrorsInAMessageLoop()
        {
            var random = new Random(1);

            for (var j = 0; j < 5; )
            {
                var reedSolomon = new ReedSolomon();
                var data = new int[223];

                for (var i = 0; i < 223; i++)
                {
                    data[i] = random.Next(1, 255);
                }

                var modifiedData = reedSolomon.EncodeRawBytesArray(data);

                var modifIndex = random.Next(255);
                var modifValue = random.Next(255);
                while (modifValue == modifiedData[modifIndex])
                {
                    modifValue = random.Next(255);
                }
                modifiedData[modifIndex] = modifValue;

                var modifIndex2 = random.Next(255);
                var modifValue2 = random.Next(255);
                while (modifIndex == modifIndex2)
                {
                    modifIndex2 = random.Next(255);
                }
                while (modifValue2 == modifiedData[modifIndex2])
                {
                    modifValue2 = random.Next(255);
                }
                modifiedData[modifIndex2] = modifValue2;

                var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

                var response = simpleDecoder.Decode(modifiedData, 32);

                var bytesDiffCount = GetDiffTable(data, modifiedData, 223);

                Assert.IsTrue(bytesDiffCount.Count <= 2);
                if (response)
                {
                    j++;
                    for (var i = 0; i < 223; i++)
                    {
                        Assert.AreEqual(data[i], modifiedData[i]);
                    }
                }
            }
        }

        [TestMethod]
        public void ProperlyCorrectsUpto16ErrorsInAMessageLoop()
        {
            var random = new Random(1);

            for (var j = 0; j < 17; j++)
            {
                var reedSolomon = new ReedSolomon();
                var data = new int[223];

                for (var i = 0; i < 223; i++)
                {
                    data[i] = random.Next(255);
                }

                var modifiedData = reedSolomon.EncodeRawBytesArray(data);
                var errorCount = 0;

                var startIndex = random.Next(0,255-16);
                for (var k = startIndex; k < startIndex + j; k++)
                {
                    var modifValue = random.Next(255);
                    while (modifValue == modifiedData[k])
                    {
                        modifValue = random.Next(255);
                    }
                    modifiedData[k] = modifValue;
                    errorCount++;
                }

                var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

                var response = simpleDecoder.Decode(modifiedData, 32);

                var bytesDiffCount = GetDiffTable(data, modifiedData, 223);

                Assert.IsTrue(bytesDiffCount.Count <= errorCount);
                
                for (var i = 0; i < 223; i++)
                {
                    Assert.AreEqual(data[i], modifiedData[i]);
                }
            }
        }

        [TestMethod]
        public void ImageDecodingTest()
        {
            Bitmap originalImage;
            var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "/sandwich.bmp", FileMode.Open, FileAccess.Read);
            using (fileStream)
            {
                originalImage = new Bitmap(fileStream);
            }
            fileStream.Close();

            var reedSolomon = new ReedSolomon(32);
            var data = ImageProcessing.GetRawBytesFromRGBImage(originalImage);

            var originalData = new int[data.Length];
            Array.Copy(data, originalData, data.Length);

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            var errorsCount = ErrorProvider.FillInPercentageOfErrors(modifiedData, 1);

            reedSolomon.SimplyDecodeRawBytesArray(modifiedData, data);

            var processedImage = ImageProcessing.GetRGBImageFromRawBytes(originalImage, data);

            var diffCount = 0;
            Bitmap diffImage;
            diffCount = ImageProcessing.Compare(processedImage, originalImage, out diffImage);

            var bytesDiffCount = GetDiffTable(originalData, data);

            Assert.IsTrue(bytesDiffCount.Count <= errorsCount);
            Assert.IsTrue(diffCount <= errorsCount);
        }

        [TestMethod]
        public void ImageDecodingBlockErrorsTest()
        {
            Bitmap originalImage;
            var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "/sandwich.bmp", FileMode.Open, FileAccess.Read);
            using (fileStream)
            {
                originalImage = new Bitmap(fileStream);
            }
            fileStream.Close();

            var reedSolomon = new ReedSolomon(32);
            var data = ImageProcessing.GetRawBytesFromRGBImage(originalImage);

            var originalData = new int[data.Length];
            Array.Copy(data, originalData, data.Length);

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            var errorsCount = ErrorProvider.FillInGroupErrorsForEveryBlock(modifiedData, 16, 255);

            reedSolomon.SimplyDecodeRawBytesArray(modifiedData, data);

            var processedImage = ImageProcessing.GetRGBImageFromRawBytes(originalImage, data);

            Bitmap diffImage;
            var diffCount = ImageProcessing.Compare(processedImage, originalImage, out diffImage);

            var bytesDiffCount = GetDiffTable(originalData, data);

            Assert.AreEqual(0, bytesDiffCount.Count);
            Assert.AreEqual(0, diffCount);
        }

        private static List<List<int>> GetDiffTable(int[] original, int[] modified, int? length = null)
        {
            var result = new List<List<int>>();
            for (var i = 0; i < (length ?? original.Length); i++)
            {
                if (original[i] != modified[i])
                {
                    var error = new List<int> {i, original[i], modified[i]};
                    result.Add(error);
                }
            }

            return result;
        }

        [TestMethod]
        public void LongDivideProperlyCorrectsErrorInAMessage4()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[223];

            for (var i = 0; i < 223; i++)
            {
                data[i] = ((i + 50) % 255) + 1;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[0] = 1;
            modifiedData[15] = 16;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.LongDecode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 223; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }
    }
}
