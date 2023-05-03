// See https://aka.ms/new-console-template for more information


namespace PkProjekt;

public static class Program
{
    const string charset =
        "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890!@#$%^&*-_=+([{<)]}>'\";:?,./|";

    static void Hashuj2()
    {
        uint[] hash2 = { 0x0F4D8C79, 0xBDAEEE50, 0x19470D10, 0xD2E2E920 };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                char[] word = { charset[i], charset[j] };
                Hasher hasher = new Hasher();
                hasher.hash(word, 2);
                bool found = hash2[0] == hasher.GetA() && hash2[1] == hasher.GetB() &&
                             hash2[2] == hasher.GetC() && hash2[3] == hasher.GetD();
                if (found)
                {
                    Console.WriteLine("The word was found! It is: " + word[0] + word[1]);
                    return;
                }
                else
                {
                    //Console.WriteLine(word[0] + word[1]);
                    //Console.WriteLine(hasher.GetA() + " " + hasher.GetB() + " " + hasher.GetC() + " " + hasher.GetD());
                }
            }
        }
    }

    static void Hashuj3()
    {
        uint[] hash3 = { 0x602064B8, 0x39B20D8B, 0x2ABF78B6, 0xC6C43D71 };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                for (int k = 0; k < charset.Length; k++)
                {
                    char[] word = { charset[i], charset[j], charset[k] };
                    Hasher hasher = new Hasher();
                    hasher.hash(word, 3);
                    bool found = hash3[0] == hasher.GetA() && hash3[1] == hasher.GetB() &&
                                 hash3[2] == hasher.GetC() && hash3[3] == hasher.GetD();
                    if (found)
                    {
                        Console.WriteLine("The word was found! It is: " + word[0] + word[1] + word[2]);
                        return;
                    }
                }
            }
        }
    }

    static void Hashuj4()
    {
        uint[] hash3 = { 0x7D6537CF, 0xF562791F, 0x673BD230, 0xF28ED621 };
        for (int i = 0; i < charset.Length; i++)
        {
            for (int j = 0; j < charset.Length; j++)
            {
                for (int k = 0; k < charset.Length; k++)
                {
                    for (int l = 0; l < charset.Length; l++)
                    {
                        char[] word = { charset[i], charset[j], charset[k], charset[l] };
                        Hasher hasher = new Hasher();
                        hasher.hash(word, 4);
                        bool found = hash3[0] == hasher.GetA() && hash3[1] == hasher.GetB() &&
                                     hash3[2] == hasher.GetC() && hash3[3] == hasher.GetD();
                        if (found)
                        {
                            Console.WriteLine("The word was found! It is: " + word[0] + word[1] + word[2] + word[3]);
                            return;
                        }
                    }
                }
            }
        }
    }

    public static void Main(string[] args)
    {
        //Hashuj2();
        //Hashuj3();
        //Hashuj4();
        //Hashuj5();
        GpuDecryptor decryptor = new GpuDecryptor();
        //decryptor.Start();
        //decryptor.RunKernel6();
        decryptor.RunKernel7();
    }

}