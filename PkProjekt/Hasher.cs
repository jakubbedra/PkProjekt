namespace PkProjekt;

class Hasher
{
    private const int N = 30;
    private uint _a;
    private uint _b;
    private uint _c;
    private uint _d;

    private static readonly uint[] Padding = new uint[]
    {
        0xB3, 0xC5, 0x44, 0x97, 0x42, 0x70, 0x9D,
        0x88, 0x1B, 0x6A, 0xCE, 0x10, 0x13, 0xA8,
        0x5F, 0x57, 0x8F, 0x0C, 0x24, 0xF1, 0x9F,
        0xE9, 0xA5, 0xCD, 0xD1, 0xDC, 0xD2, 0x6E,
        0x16, 0xA7, 0xBB, 0xE5
    };

    public Hasher()
    {
        _a = 0x5AC24860;
        _b = 0xDA545106;
        _c = 0x716ADFDB;
        _d = 0x4DA893CC;
    }

    private uint rol(uint x, int shift)
    {
        const int mask = 8 * sizeof(uint) - 1;
        shift &= (int)mask;
        return (x << shift) | (x >> (-shift & mask));
    }

    private void hashBlock(uint[] w)
    {
        // i = 0 .. 9
        for (int i = 0; i < 10; i++)
        {
            if (i >= 6)
            {
                w[i] = rol((w[i - 6] ^ w[i - 5] ^ (w[i - 3] + w[i - 1])), 3);
            }

            uint tmp = (_a & _b) + (rol(_c, 4) ^ (~_d)) + w[i] + 0xFE887401;
            _a = _b;
            _b = _c;
            _c = _d;
            _d = tmp;
        }

        // i = 10 .. 19
        for (int i = 10; i < 20; i++)
        {
            w[i] = rol((w[i - 6] ^ w[i - 5] ^ (w[i - 3] + w[i - 1])), 3);
            uint tmp = (_a & _b) ^ ((~_a) & _c) ^ (_c & (rol(_d, 2))) ^ w[i] ^ 0x44C38316;
            _a = _b;
            _b = _c;
            _c = _d;
            _d = tmp;
        }

        // i = 20 .. 29
        for (int i = 20; i < 30; i++)
        {
            w[i] = rol((w[i - 6] ^ w[i - 5] ^ (w[i - 3] + w[i - 1])), 3);
            uint tmp = ((_a ^ rol(_b, 2)) ^ (rol(_c, 4)) ^ (rol(_d, 7))) + (w[i] ^ 0x21221602);
            _a = _b;
            _b = _c;
            _c = _d;
            _d = tmp;
        }
    }

    public void hash(char[] inputMsg, int length)
    {
        // check if padding is needed, if yes add it
        char[] msg = inputMsg;
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
            hashBlock(w);
        }

        array = null;
    }

    public uint GetA()
    {
        return _a;
    }

    public uint GetB()
    {
        return _b;
    }

    public uint GetC()
    {
        return _c;
    }

    public uint GetD()
    {
        return _d;
    }
}