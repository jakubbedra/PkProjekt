using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;

namespace PkProjekt;

public class GpuDecryptor
{
    //const string charset =
    //    "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890!@#$%^&*-_=+([{<)]}>'\";:?,./|";


    private static uint rol(uint x, int shift)
    {
        const int mask = 8 * sizeof(uint) - 1;
        shift &= (int)mask;
        return (x << shift) | (x >> (-shift & mask));
    }

    public static uint[] hash(byte[] inputMsg, int length)
    {
        uint _a = 0x5AC24860;
        uint _b = 0xDA545106;
        uint _c = 0x716ADFDB;
        uint _d = 0x4DA893CC;

        uint[] Padding = new uint[]
        {
            0xB3, 0xC5, 0x44, 0x97, 0x42, 0x70, 0x9D,
            0x88, 0x1B, 0x6A, 0xCE, 0x10, 0x13, 0xA8,
            0x5F, 0x57, 0x8F, 0x0C, 0x24, 0xF1, 0x9F,
            0xE9, 0xA5, 0xCD, 0xD1, 0xDC, 0xD2, 0x6E,
            0x16, 0xA7, 0xBB, 0xE5
        };

        // check if padding is needed, if yes add it
        byte[] msg = inputMsg;
        int paddingLength = 24 - length % 24;
        int paddingCtr = 0;
        // convert char array to int array
        uint[] array = new uint[(length + paddingLength) / 4];
        for (int i = 0; i < (length + paddingLength) / 4; i++)
        {
            array[i] = 0x00000000;

            uint tmp0 = i * 4 + 0 < length ? Convert.ToUInt32(msg[i * 4 + 0]) : Padding[paddingCtr++];
            array[i] = array[i] | tmp0;
            array[i] = array[i] << 8;
            uint tmp1 = i * 4 + 1 < length ? Convert.ToUInt32(msg[i * 4 + 1]) : Padding[paddingCtr++];
            array[i] = array[i] | tmp1;
            array[i] = array[i] << 8;
            uint tmp2 = i * 4 + 2 < length ? Convert.ToUInt32(msg[i * 4 + 2]) : Padding[paddingCtr++];
            array[i] = array[i] | tmp2;
            array[i] = array[i] << 8;
            uint tmp3 = i * 4 + 3 < length ? Convert.ToUInt32(msg[i * 4 + 3]) : Padding[paddingCtr++];
            array[i] = array[i] | tmp3;
        }

        // divide msg into blocks
        for (int i = 0; i < (length + paddingLength) / 24; i++)
        {
            uint[] w =
            {
                array[i * 6], array[i * 6 + 1], array[i * 6 + 2],
                array[i * 6 + 3], array[i * 6 + 4], array[i * 6 + 5],
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            // hashing block

            // i = 0 .. 9
            for (int j = 0; j < 10; j++)
            {
                if (j >= 6)
                {
                    w[j] = rol((w[j - 6] ^ w[j - 5] ^ (w[j - 3] + w[j - 1])), 3);
                }

                uint tmp = (_a & _b) + (rol(_c, 4) ^ (~_d)) + w[j] + 0xFE887401;
                _a = _b;
                _b = _c;
                _c = _d;
                _d = tmp;
            }

            // i = 10 .. 19
            for (int j = 10; j < 20; j++)
            {
                w[j] = rol((w[j - 6] ^ w[j - 5] ^ (w[j - 3] + w[j - 1])), 3);
                uint tmp = (_a & _b) ^ ((~_a) & _c) ^ (_c & (rol(_d, 2))) ^ w[j] ^ 0x44C38316;
                _a = _b;
                _b = _c;
                _c = _d;
                _d = tmp;
            }

            // i = 20 .. 29
            for (int j = 20; j < 30; j++)
            {
                w[j] = rol((w[j - 6] ^ w[j - 5] ^ (w[j - 3] + w[j - 1])), 3);
                uint tmp = ((_a ^ rol(_b, 2)) ^ (rol(_c, 4)) ^ (rol(_d, 7))) + (w[j] ^ 0x21221602);
                _a = _b;
                _b = _c;
                _c = _d;
                _d = tmp;
            }
        }

        return new uint[]
        {
            _a, _b, _c, _d
        };

        //return new uint[] {0,0,0,0 };
    }


    private static void Kernel(Index2D index, ArrayView<byte> charset, ArrayView<byte> output)
    {
        int m = index.X;
        int n = index.Y;
        uint[] hash3 = { 0xD7B71AEB, 0x106BBB43, 0x00EA99FB, 0x0F307A0C };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                for (int k = 0; k < charset.Length; k++)
                {
                    for (int l = 0; l < charset.Length; l++)
                    {
                        byte[] word =
                        {
                            charset[i], charset[j], charset[k], charset[l], charset[m], charset[n]
                        };
                        uint[] result = hash(word, 6);
                        bool found = hash3[0] == result[0] && hash3[1] == result[1] &&
                                     hash3[2] == result[2] && hash3[3] == result[3];

                        // begin


                        // end

                        if (found)
                        {
                            output[0] = (byte)word[0];
                            output[1] = (byte)word[1];
                            output[2] = (byte)word[2];
                            output[3] = (byte)word[3];
                            output[4] = (byte)word[4];
                            output[5] = (byte)word[5];
                            return;
                        }
                    }
                }
            }
        }
    }

    private static void Kernel6(Index2D index, ArrayView<byte> charset, ArrayView<byte> output, int start)
    {
        int m = index.X;
        int n = index.Y;
        uint[] hash3 = { 0xD7B71AEB, 0x106BBB43, 0x00EA99FB, 0x0F307A0C };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                for (int k = 0; k < charset.Length; k++)
                {
                    for (int l = start; l < start + 10 && l < charset.Length; l++)
                    {
                        byte[] word =
                        {
                            charset[i], charset[j], charset[k], charset[l], charset[m], charset[n]
                        };
                        uint[] result = hash(word, 6);
                        bool found = hash3[0] == result[0] && hash3[1] == result[1] &&
                                     hash3[2] == result[2] && hash3[3] == result[3];

                        if (found)
                        {
                            output[0] = (byte)word[0];
                            output[1] = (byte)word[1];
                            output[2] = (byte)word[2];
                            output[3] = (byte)word[3];
                            output[4] = (byte)word[4];
                            output[5] = (byte)word[5];
                            return;
                        }
                    }
                }
            }
        }
    }

    private static void Kernel7(Index2D index, ArrayView<byte> charset, ArrayView<byte> output, int start, int o)
    {
        int m = index.X;
        int n = index.Y;
        uint[] hash3 = { 0x177A204C, 0xE0607B16, 0x1C76E7D0, 0xE557E452 };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                for (int k = 0; k < charset.Length; k++)
                {
                    for (int l = start; l < start + 10 && l < charset.Length; l++)
                    {
                        byte[] word =
                        {
                            charset[i], charset[j], charset[k], charset[l], charset[m], charset[n], charset[o]
                        };
                        uint[] result = hash(word, 7);
                        bool found = hash3[0] == result[0] && hash3[1] == result[1] &&
                                     hash3[2] == result[2] && hash3[3] == result[3];

                        if (found)
                        {
                            output[0] = word[0];
                            output[1] = word[1];
                            output[2] = word[2];
                            output[3] = word[3];
                            output[4] = word[4];
                            output[5] = word[5];
                            output[6] = word[6];
                            return;
                        }
                    }
                }
            }
        }
    }

    private static void Kernel5(Index2D index, ArrayView<byte> charset, ArrayView<byte> output)
    {
        int m = index.X;
        int n = index.Y;
        uint[] hash3 = { 0x14B9DA32, 0x60940AB9, 0xB9424733, 0xB53FF846 };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                for (int k = 0; k < charset.Length; k++)
                {
                    byte[] word =
                    {
                        charset[i], charset[j], charset[k], charset[m], charset[n]
                    };
                    uint[] result = hash(word, 5);
                    bool found = hash3[0] == result[0] && hash3[1] == result[1] &&
                                 hash3[2] == result[2] && hash3[3] == result[3];

                    if (found)
                    {
                        output[0] = (byte)word[0];
                        output[1] = (byte)word[1];
                        output[2] = (byte)word[2];
                        output[3] = (byte)word[3];
                        output[4] = (byte)word[4];
                        output[5] = (byte)word[5];
                        return;
                    }
                }
            }
        }
    }

    private static void Kernel4(Index2D index, ArrayView<byte> charset, ArrayView<byte> output)
    {
        int m = index.X;
        int n = index.Y;
        uint[] hash3 = { 0x7D6537CF, 0xF562791F, 0x673BD230, 0xF28ED621 };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                byte[] word =
                {
                    charset[i], charset[j], charset[m], charset[n]
                };
                uint[] result = hash(word, 4);
                bool found = hash3[0] == result[0] && hash3[1] == result[1] &&
                             hash3[2] == result[2] && hash3[3] == result[3];

                if (found)
                {
                    output[0] = (byte)word[0];
                    output[1] = (byte)word[1];
                    output[2] = (byte)word[2];
                    output[3] = (byte)word[3];
                    output[4] = (byte)word[4];
                    output[5] = (byte)word[5];
                    return;
                }
            }
        }
    }

    private static void Kernel2(Index2D index, ArrayView<byte> charset, ArrayView<byte> output)
    {
        int m = index.X;
        int n = index.Y;
        uint[] hash3 = { 0x0F4D8C79, 0xBDAEEE50, 0x19470D10, 0xD2E2E920 };

        byte[] word =
        {
            charset[m], charset[n]
        };
        uint[] result = hash(word, 2);
        bool found = hash3[0] == result[0] && hash3[1] == result[1] &&
                     hash3[2] == result[2] && hash3[3] == result[3];

        if (found)
        {
            output[0] = (byte)word[0];
            output[1] = (byte)word[1];
            output[2] = (byte)word[2];
            output[3] = (byte)word[3];
            output[4] = (byte)word[4];
            output[5] = (byte)word[5];
        }
    }

    private readonly char[] _charArray =
    {
        'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x',
        'c', 'v', 'b', 'n', 'm', 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H',
        'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '!',
        '@', '#', '$', '%', '^', '&', '*', '-', '_', '=', '+', '(', '[', '{', '<', ')', ']', '}', '>', '\'', '"',
        ';', ':', '?', ',', '.', '/', '|'
    };

    public void RunKernel6()
    {
        byte[] bytes = new byte[_charArray.Length];
        for (int i = 0; i < _charArray.Length; i++)
        {
            bytes[i] = (byte)_charArray[i];
        }

        // Initialize ILGPU.
        Context context = Context.Create(builder => builder.Cuda().CPU().EnableAlgorithms());
        Accelerator accelerator = context.GetPreferredDevice(preferCPU: false)
            .CreateAccelerator(context);

        // Load the data.
        MemoryBuffer1D<byte, Stride1D.Dense> deviceData = accelerator.Allocate1D(bytes);
        MemoryBuffer1D<byte, Stride1D.Dense> deviceOutput = accelerator.Allocate1D<byte>(6);

// run multiple kernels in row
        for (int l = 0; l < deviceData.Length; l += 10)
        {
            Action<Index2D, ArrayView<byte>, ArrayView<byte>, int> loadedKernel =
                accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView<byte>, ArrayView<byte>, int>(Kernel6);
            loadedKernel(new Index2D((int)deviceData.Length, (int)deviceData.Length), deviceData.View,
                deviceOutput.View, l);

            // wait for the accelerator to be finished with whatever it's doing
            // in this case it just waits for the kernel to finish.
            accelerator.Synchronize();
            // moved output data from the GPU to the CPU for output to console
            byte[] hostOutput = deviceOutput.GetAsArray1D();

            if (bytes.Contains(hostOutput[0]))
            {
                Console.Write($"Found word: ");
                for (int i = 0; i < 6; i++)
                {
                    Console.Write((char)hostOutput[i]);
                }

                Console.WriteLine();
            }
        }

        Console.WriteLine();
        accelerator.Dispose();
        context.Dispose();
    }

    public void RunKernel7()
    {
        byte[] bytes = new byte[_charArray.Length];
        for (int i = 0; i < _charArray.Length; i++)
        {
            bytes[i] = (byte)_charArray[i];
        }

        // Initialize ILGPU.
        Context context = Context.Create(builder => builder.Cuda().CPU().EnableAlgorithms());
        Accelerator accelerator = context.GetPreferredDevice(preferCPU: false)
            .CreateAccelerator(context);

        // Load the data.
        MemoryBuffer1D<byte, Stride1D.Dense> deviceData = accelerator.Allocate1D(bytes);
        MemoryBuffer1D<byte, Stride1D.Dense> deviceOutput = accelerator.Allocate1D<byte>(7);
        Action<Index2D, ArrayView<byte>, ArrayView<byte>, int, int> loadedKernel =
            accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView<byte>, ArrayView<byte>, int, int>(Kernel7);

        // run multiple kernels in row
        for (int o = 87; o < deviceData.Length; o++)
        {
            Console.WriteLine($"Iteration {o}/{deviceData.Length}");
            for (int l = 0; l < deviceData.Length; l += 10)
            {
                loadedKernel(new Index2D((int)deviceData.Length, (int)deviceData.Length), deviceData.View,
                    deviceOutput.View, l, o);
                accelerator.Synchronize();
                byte[] hostOutput = deviceOutput.GetAsArray1D();

                if (bytes.Contains(hostOutput[0]))
                {
                    Console.Write($"Found word: ");
                    for (int i = 0; i < 7; i++)
                    {
                        Console.Write((char)hostOutput[i]);
                    }

                    Console.WriteLine();
                    accelerator.Dispose();
                    context.Dispose();
                    return;
                }
            }
        }
        Console.WriteLine();
        accelerator.Dispose();
        context.Dispose();
    }

    public void Start()
    {
        byte[] bytes = new byte[_charArray.Length];
        for (int i = 0; i < _charArray.Length; i++)
        {
            bytes[i] = (byte)_charArray[i];
        }

        // Initialize ILGPU.
        Context context = Context.Create(builder => builder.Cuda().EnableAlgorithms());
        Accelerator accelerator = context.GetPreferredDevice(preferCPU: false)
            .CreateAccelerator(context);

        // Load the data.
        MemoryBuffer1D<byte, Stride1D.Dense> deviceData = accelerator.Allocate1D(bytes);
        MemoryBuffer1D<byte, Stride1D.Dense> deviceOutput = accelerator.Allocate1D<byte>(5);


        Action<Index2D, ArrayView<byte>, ArrayView<byte>> loadedKernel =
            accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView<byte>, ArrayView<byte>>(Kernel5);
        loadedKernel(new Index2D((int)deviceData.Length, (int)deviceData.Length), deviceData.View, deviceOutput.View);

        // wait for the accelerator to be finished with whatever it's doing
        // in this case it just waits for the kernel to finish.
        accelerator.Synchronize();

        // moved output data from the GPU to the CPU for output to console
        byte[] hostOutput = deviceOutput.GetAsArray1D();

        Console.WriteLine("Found word:");
        for (int i = 0; i < 5; i++)
        {
            Console.Write((char)hostOutput[i]);
        }

        Console.WriteLine();
        accelerator.Dispose();
        context.Dispose();
    }
    
    public void RunKernel4()
    {
        byte[] bytes = new byte[_charArray.Length];
        for (int i = 0; i < _charArray.Length; i++)
        {
            bytes[i] = (byte)_charArray[i];
        }

        // Initialize ILGPU.
        Context context = Context.Create(builder => builder.Cuda().EnableAlgorithms());
        Accelerator accelerator = context.GetPreferredDevice(preferCPU: false)
            .CreateAccelerator(context);

        // Load the data.
        MemoryBuffer1D<byte, Stride1D.Dense> deviceData = accelerator.Allocate1D(bytes);
        MemoryBuffer1D<byte, Stride1D.Dense> deviceOutput = accelerator.Allocate1D<byte>(4);


        Action<Index2D, ArrayView<byte>, ArrayView<byte>> loadedKernel =
            accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView<byte>, ArrayView<byte>>(Kernel4);
        loadedKernel(new Index2D((int)deviceData.Length, (int)deviceData.Length), deviceData.View, deviceOutput.View);

        // wait for the accelerator to be finished with whatever it's doing
        // in this case it just waits for the kernel to finish.
        accelerator.Synchronize();

        // moved output data from the GPU to the CPU for output to console
        byte[] hostOutput = deviceOutput.GetAsArray1D();

        Console.WriteLine("Found word:");
        for (int i = 0; i < 4; i++)
        {
            Console.Write((char)hostOutput[i]);
        }

        Console.WriteLine();
        accelerator.Dispose();
        context.Dispose();
    }
}