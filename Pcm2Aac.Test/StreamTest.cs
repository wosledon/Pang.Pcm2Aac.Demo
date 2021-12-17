using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Pcm2Aac.Test
{
    public class StreamTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public StreamTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test1()
        {
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10 };
            MemoryStream ms = new MemoryStream(data);

            var output = new byte[data.Length];
            ms.Read(output);

            _testOutputHelper.WriteLine($"ms.length: {ms.Length} output: {output[0]} {output[1]} {output[9]}");
        }
    }
}