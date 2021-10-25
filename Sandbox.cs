using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheLeftExit.Memory {
    class Sandbox {
        static void Main(string[] args) {
            Process growtopia = Process.GetProcessesByName("Growtopia").Single();
            ProcessMemory mem = new ProcessMemory(growtopia);

            while (true) {
                (float X, float Y) pos = mem.Root["App"][0xAB0]["NetAvatar"].Read<(float, float)>(8);
                Console.WriteLine(pos);
                Thread.Sleep(100);
            }
        }
    }
}
