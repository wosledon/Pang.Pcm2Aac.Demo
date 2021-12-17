using System;
using System.IO;
using FFMpegCore;
using FFMpegCore.Enums;
using Xunit;
using Xunit.Abstractions;

namespace Pcm2Aac.Test
{
    public class AudioTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AudioTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async void PcmToAac()
        {
            var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/testpacket.pcm");
            var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/test.aac");

            _testOutputHelper.WriteLine($"file path: {inputPath}");

            // var mediaInfo = await FFProbe.AnalyseAsync(inputPath);

            await FFMpegArguments.FromFileInput(inputPath)
                .OutputToFile(outputPath, false, opts =>
                {
                    opts.WithAudioCodec(AudioCodec.Aac);
                }).ProcessAsynchronously();
        }
    }
}