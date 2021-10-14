using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;

namespace DeepVoiceSharp
{
	internal class AudioData : ISampleProvider
	{
		private WaveFormat format;
		private float[] samples;
		public AudioData(string wavFilePath)
		{
			using (var fileStream = File.OpenRead(wavFilePath))
			{
				using (var reader = new WaveFileReader(fileStream))
				{
					var sampleProvider = reader.ToSampleProvider().ToMono();
					int sampleCount = (int)reader.SampleCount;
					format = reader.WaveFormat;
					samples = new float[sampleCount];
					sampleProvider.Read(samples, 0, sampleCount);
				}
			}
		}

		public AudioData(float[] samples, WaveFormat waveFormat)
		{
			if (samples == null)
				throw new ArgumentNullException(nameof(samples));
			if (waveFormat == null)
				throw new ArgumentNullException(nameof(waveFormat));

			this.samples = samples;
			format = waveFormat;
		}

		public WaveFormat WaveFormat => format;
		public int SampleCount => samples.Length;

		public float[] GetSamples()
		{
			float[] result = new float[samples.Length];
			samples.CopyTo(result, 0);
			return result;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int outIndex = offset;
			int maxSamples = Math.Min(count, samples.Length);
			for (int n = 0; n < maxSamples; n++)
			{
				buffer[outIndex++] = samples[n];
			}
			return maxSamples;
		}

		public AudioData Resample(int newSampleRate)
		{
			if (format.SampleRate == newSampleRate)
			{
				return this;
			}
			else
			{
				var resampler = new WdlResamplingSampleProvider(this, newSampleRate);
				int newSampleCount = (int)((long)SampleCount * newSampleRate / format.SampleRate);
				float[] newSamples = ReadSamples(resampler, newSampleCount);
				return new AudioData(newSamples, resampler.WaveFormat);
			}
		}

		public void WriteToFile(string path)
		{
			using (WaveFileWriter writer = new WaveFileWriter(path, format))
			{
				writer.WriteSamples(samples, 0, samples.Length);
			}
		}

		private static float[] ReadSamples(ISampleProvider sampleProvider, int count)
		{
			float[] buffer = new float[count];
			sampleProvider.Read(buffer, 0, count);
			return buffer;
		}
	}
}
