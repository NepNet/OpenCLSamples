using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Compute.OpenCL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageFormat = OpenTK.Compute.OpenCL.ImageFormat;
using Rectangle = System.Drawing.Rectangle;

namespace OpenCLSamples
{
	public static class Grayscale
	{
		private const float rFact = 0.2126f;
		private const float gFact = 0.7152f;
		private const float bFact = 0.0722f;
		
		public static void ConvertToGrayscale(string inputPath)
		{
			CLResultCode error = CLResultCode.Success;
			
			//Get a platform
			CLPlatform[] platforms = new CLPlatform[1];
			CL.GetPlatformIds(1, platforms, out _);
			
			//Get a device
			CLDevice[] devices = new CLDevice[1];
			CL.GetDeviceIds(platforms[0], DeviceType.Gpu, 1, devices, out _);
			
			//Create a context
			CLContext context = CL.CreateContext(IntPtr.Zero, devices, IntPtr.Zero, IntPtr.Zero, out error);

			if (error != CLResultCode.Success)
			{
				throw new Exception("Error on creating a context");
			}
			
			//Create the program from source
			CLProgram program =
				CL.CreateProgramWithSource(context, File.ReadAllText("Kernels/grayscale.cl"), out error);

			error = CL.BuildProgram(program, 1, devices, null, IntPtr.Zero, IntPtr.Zero);

			if (error != CLResultCode.Success)
			{
				throw new Exception($"Error on building program: {error}");
			}
			
			
			//Get the kernel which we will use
			CLKernel kernel = CL.CreateKernel(program, "grayscale", out error);

			ImageFormat inputImageFormat = new ImageFormat
			{
				ChannelOrder = ChannelOrder.Bgra, 
				ChannelType = ChannelType.UnsignedInteger8
			};
			
			Bitmap inputBitmap = new Bitmap(inputPath);
			BitmapData inputData = inputBitmap.LockBits(new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			
			ImageDescription imageDescription = new ImageDescription()
			{
				ImageType = MemoryObjectType.Image2D,
				Width = (UIntPtr) inputBitmap.Width,
				Height = (UIntPtr) inputBitmap.Height,
				Depth = (UIntPtr) 1
			};

			CLImage inputImage = CL.CreateImage(context, MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr,
				ref inputImageFormat, ref imageDescription, inputData.Scan0, out error);
			
			Bitmap outputBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height, PixelFormat.Format32bppArgb);
			BitmapData outputData = outputBitmap.LockBits(new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height),
				ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			
			CLImage outputImage = CL.CreateImage(context, MemoryFlags.WriteOnly | MemoryFlags.UseHostPtr,
				ref inputImageFormat, ref imageDescription, outputData.Scan0, out error);
			
			CL.SetKernelArg(kernel, 0, rFact);
			CL.SetKernelArg(kernel, 1, gFact);
			CL.SetKernelArg(kernel, 2, bFact);

			CL.SetKernelArg(kernel, 3, inputImage);
			CL.SetKernelArg(kernel, 4, outputImage);
			
			CLCommandQueue queue = CL.CreateCommandQueueWithProperties(context, devices[0], IntPtr.Zero, out error);

			CL.EnqueueNDRangeKernel(queue, kernel, 2, new UIntPtr[] {UIntPtr.Zero, UIntPtr.Zero,}, new[]
			{
				(UIntPtr)inputBitmap.Width,
				(UIntPtr)inputBitmap.Height,
			}, null, 0, null, out _);

			CL.EnqueueReadImage(queue, outputImage, 1, new UIntPtr[] {UIntPtr.Zero, UIntPtr.Zero,},
				new UIntPtr[]
				{
					(UIntPtr)inputBitmap.Width,
					(UIntPtr)inputBitmap.Height,
					(UIntPtr)1,
				}, UIntPtr.Zero, UIntPtr.Zero, outputData.Scan0, 0, null, out _);

			outputBitmap.UnlockBits(outputData);
			
			outputBitmap.Save("grayscale.png", System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}