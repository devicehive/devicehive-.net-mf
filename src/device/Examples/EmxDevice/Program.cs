using System;
using Microsoft.SPOT;

namespace EmxDevice
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                PrototypeDevice emx = new PrototypeDevice();
                emx.Init();
                bool rv = false;
                do
                {
                    rv = emx.Connect();
                    while (rv)
                    {
                        rv = emx.ProcessCommands();
                    }

                }
                while (rv);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

    }
}
