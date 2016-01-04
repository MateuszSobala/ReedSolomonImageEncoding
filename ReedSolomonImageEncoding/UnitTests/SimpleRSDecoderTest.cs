using ReedSolomonImageEncoding;
using System;
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
            var data = new int[224];

            for (var i = 0; i < 224; i++)
            {
                data[i] = i+10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            var simpleDecoder = new SimpleRSDecoder();

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyFindsErrorInAMessage()
        {
            var reedSolomon = new ReedSolomon(32);
            var data = new int[224];

            for (var i = 0; i < 224; i++)
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
            var reedSolomon = new ReedSolomon();
            var data = new int[224];

            for (var i = 0; i < 224; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[10] += 100;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessage2()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[224];

            for (var i = 0; i < 224; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[12] += 87;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessage3()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[224];

            for (var i = 0; i < 224; i++)
            {
                data[i] = i + 15;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[12] += 182;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessage4()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[224];

            for (var i = 0; i < 224; i++)
            {
                data[i] = ((i + 50) % 255) + 1;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[2] = 222;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrectsErrorInAMessageLoop()
        {
            var random = new Random(1);

            for (var j = 0; j < 10;)
            {
                var reedSolomon = new ReedSolomon();
                var data = new int[224];

                for (var i = 0; i < 224; i++)
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
                    for (var i = 0; i < 224; i++)
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
            var data = new int[224];

            for (var i = 0; i < 224; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[10] += 100;
            modifiedData[11] += 100;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects3ErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[224];

            for (var i = 0; i < 224; i++)
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

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects5ErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[224];

            for (var i = 0; i < 224; i++)
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

            for (var i = 0; i < 224; i++)
            {
                Assert.AreEqual(data[i], modifiedData[i]);
            }
        }

        [TestMethod]
        public void ProperlyCorrects2FarErrorsInAMessage()
        {
            var reedSolomon = new ReedSolomon();
            var data = new int[224];

            for (var i = 0; i < 224; i++)
            {
                data[i] = i + 10;
            }

            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            modifiedData[1] += 100;
            modifiedData[11] += 100;

            var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

            var response = simpleDecoder.Decode(modifiedData, 32);

            Assert.IsTrue(response);

            for (var i = 0; i < 224; i++)
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
                var data = new int[224];

                for (var i = 0; i < 224; i++)
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
                modifValue = random.Next(255);
                while (modifIndex == modifIndex2)
                {
                    modifIndex2 = random.Next(255);
                }
                while (modifValue == modifiedData[modifIndex2])
                {
                    modifValue = random.Next(255);
                }
                modifiedData[modifIndex2] = modifValue;

                var simpleDecoder = new SimpleRSDecoder(GenericGF.DATA_MATRIX_FIELD_256);

                var response = simpleDecoder.Decode(modifiedData, 32);

                if (response)
                {
                    j++;
                    for (var i = 0; i < 224; i++)
                    {
                        Assert.AreEqual(data[i], modifiedData[i]);
                    }
                }
            }
        }
    }
}
