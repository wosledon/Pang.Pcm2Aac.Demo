using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Extend;
using FFMpegCore.Pipes;
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
        public async void Mp3ToAac()
        {
            var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/1.mp3");
            var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/test.aac");

            _testOutputHelper.WriteLine($"file path: {inputPath}");

            var mediaInfo = await FFProbe.AnalyseAsync(inputPath);

            await FFMpegArguments.FromFileInput(inputPath)
                .OutputToFile(outputPath, false, opts =>
                {
                    opts.WithAudioCodec(AudioCodec.Aac);
                }).ProcessAsynchronously();
        }

        [Fact]
        public async void PcmToAac()
        {
            var inputData =
                await File.ReadAllBytesAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/testpacket.pcm"));
            var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/test2.aac");

            var samples = new List<IAudioSample>()
            {
                new PcmAudioSampleWrapper(inputData)
            };

            var audioSamplesSource = new RawAudioPipeSource(samples)
            {
                Channels = 1,
                Format = "s16le",
                SampleRate = 8000
            };

            await FFMpegArguments.FromPipeInput(audioSamplesSource)
                .OutputToFile(outputPath, false, opts =>
                {
                    opts.WithAudioCodec(AudioCodec.Aac);
                }).ProcessAsynchronously();
        }

        [Fact]
        public async Task PcmToAac2()
        {
            var inputData =
                await File.ReadAllBytesAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/testpacket.pcm"));
            var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/test3.aac");

            if (File.Exists(outputPath)) File.Delete(outputPath);

            var offset = 0;
            var step = 2048;

            var stopwatch = new Stopwatch();

            while (offset + step < inputData.Length)
            {
                var stream = new MemoryStream();
                stopwatch.Start();
                var samples = new List<IAudioSample>()
                {
                    new PcmAudioSampleWrapper(new Span<byte>(inputData).Slice(offset, step).ToArray())
                };

                var audioSamplesSource = new RawAudioPipeSource(samples)
                {
                    Channels = 1,
                    Format = "s16le",
                    SampleRate = 8000
                };

                await FFMpegArguments.FromPipeInput(audioSamplesSource)
                    .OutputToPipe(new StreamPipeSink(stream), opts =>
                    {
                        opts.WithAudioCodec(AudioCodec.Aac);
                    }).ProcessAsynchronously();
                stopwatch.Stop();

                offset += step;
            }
        }

        [Fact]
        public async Task PcmToAac3()
        {
            byte[] inputData =
                await File.ReadAllBytesAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/testpacket.pcm"));

            var step = 2048;
            var offset = 0;

            var stopwatch = new Stopwatch();

            while (offset + step < inputData.Length)
            {
                stopwatch.Start();
                var stream = new MemoryStream();

                await FFMpegArguments.FromPipeInput(new StreamPipeSource(new MemoryStream(new Span<byte>(inputData).Slice(offset, step).ToArray())), opts => opts.ForceFormat("s16le"))
                    .OutputToPipe(new StreamPipeSink(stream), opts => opts.ForceFormat("fltp").WithAudioCodec(AudioCodec.Aac))
                    .ProcessAsynchronously();
                stopwatch.Stop();

                offset += step;
            }
        }

        [Fact]
        public async Task PcmToAac4()
        {
            await using var file = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/testpacket.pcm"), FileMode.Open);
            var memoryStream = new MemoryStream();
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(file), options => options.ForceFormat("s16le"))
                .OutputToPipe(new StreamPipeSink(memoryStream), options => options.WithAudioCodec(AudioCodec.Aac))
                .ProcessAsynchronously();
        }
    }
}