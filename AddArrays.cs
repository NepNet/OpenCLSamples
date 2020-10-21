using System;
using System.IO;
using OpenTK.Compute.OpenCL;

namespace OpenCLSamples
{
	public static class AddArrays
	{
		public static void Add()
		{
			CLResultCode error = CLResultCode.Success;

			CLPlatform[] platforms = new CLPlatform[1];
			CL.GetPlatformIds(1, platforms, out _);

			CLDevice[] devices = new CLDevice[1];
			CL.GetDeviceIds(platforms[0], DeviceType.All, 1, devices, out _);

			CLContext context = CL.CreateContext(IntPtr.Zero, devices, IntPtr.Zero, IntPtr.Zero, out _);


			CLProgram program =
				CL.CreateProgramWithSource(context, File.ReadAllText("Kernels/add_arrays.cl"), out error);
			error = CL.BuildProgram(program, 1, devices, null, IntPtr.Zero, IntPtr.Zero);

			if (error != CLResultCode.Success)
			{
				throw new Exception(error.ToString());
			}

			CLKernel kernel = CL.CreateKernel(program, "add", out error);

			Span<float> inputA = new float[]
			{
				1, 24, 5, 43, 41, 56
			};
			Span<float> inputB = new float[]
			{
				72, -323, -1, 43, -41, -26
			};
			Span<float> output = stackalloc float[inputA.Length];

			CLBuffer bufferA = CL.CreateBuffer(context, MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr, inputA, out _);
			CLBuffer bufferB = CL.CreateBuffer(context, MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr, inputB, out _);
			
			CLBuffer outputBuffer = CL.CreateBuffer(context, MemoryFlags.WriteOnly, (UIntPtr)(output.Length * sizeof(float)), IntPtr.Zero, out _);
			//For outputs I wouldn't use that also enqueueing a ReadBuffer is needed regardless
			//CLBuffer outputBuffer = CL.CreateBuffer(context, MemoryFlags.WriteOnly | MemoryFlags.UseHostPtr, output, out _);

			CL.SetKernelArg(kernel, 0, bufferA);
			CL.SetKernelArg(kernel, 1, bufferB);
			CL.SetKernelArg(kernel, 2, outputBuffer);

			CLCommandQueue queue = CL.CreateCommandQueueWithProperties(context, devices[0], IntPtr.Zero, out error);

			CL.EnqueueNDRangeKernel(queue, kernel, 1, null, 
				new[] {(UIntPtr) inputA.Length}, null, 0, null, out _);

			CL.EnqueueReadBuffer(queue, outputBuffer, true, UIntPtr.Zero, output, null, out _);

			foreach (float f in output)
			{
				Console.WriteLine(f);
			}


			CL.ReleaseMemoryObject(bufferA);
			CL.ReleaseMemoryObject(bufferB);
			CL.ReleaseMemoryObject(outputBuffer);
			CL.ReleaseCommandQueue(queue);
			CL.ReleaseKernel(kernel);
			CL.ReleaseProgram(program);
			CL.ReleaseContext(context);
		}
	}
}