using Cloo;

class Program
{
    static void Main(string[] args)
    {
        float[] A = [1.0f, 2.0f, 3.0f];
        float[] B = [4.0f, 5.0f, 6.0f];
        float[] C = new float[A.Length];

        ComputeContext context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(ComputePlatform.Platforms[0]), null, IntPtr.Zero);
        ComputeCommandQueue queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

        ComputeBuffer<float> aBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, A);
        ComputeBuffer<float> bBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, B);
        ComputeBuffer<float> cBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly, C.Length);

        string kernelSource = File.ReadAllText("vector_add.cl");
        ComputeProgram program = new ComputeProgram(context, kernelSource);

        try
        {
            program.Build(null, null, null, IntPtr.Zero);
        }
        catch (Exception)
        {
            foreach (var device in context.Devices)
            {
                string buildLog = program.GetBuildLog(device);
                Console.WriteLine($"Build log for device {device.Name}:\n{buildLog}");
                File.WriteAllText($"buildLog_{device.Name.Replace(' ', '_')}.txt", buildLog);
            }
        }

        ComputeKernel kernel = program.CreateKernel("vector_add");
        kernel.SetMemoryArgument(0, aBuffer);
        kernel.SetMemoryArgument(1, bBuffer);
        kernel.SetMemoryArgument(2, cBuffer);

        queue.Execute(kernel, null, [A.Length], null, null);
        queue.ReadFromBuffer(cBuffer, ref C, true, null);

        foreach (var device in context.Devices)
        {
            string buildLog = program.GetBuildLog(device);
            File.WriteAllText($"compileLog_{device.Name.Replace(' ', '_')}.txt", buildLog);
        }

        for (int i = 0; i < C.Length; i++)
        {
            Console.WriteLine($"C[{i}] = {C[i]}");
        }
    }
}