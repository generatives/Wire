using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wire.Tests.Performance;
using Wire.Tests.Performance.Types;

namespace Wire.Tests.Performace.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var _serializer = new Serializer();
            var _stream = new MemoryStream();
            var _testPerson = TypicalPersonData.MakeRandom();

            Stopwatch serializeWatch = new Stopwatch();
            Stopwatch deserializeWatch = new Stopwatch();
            float times = 2f;
            for(int i = 0; i < times; i++)
            //while (true)
            {
                serializeWatch.Start();
                _serializer.Serialize(_testPerson, _stream);
                serializeWatch.Stop();

                _stream.Seek(0, SeekOrigin.Begin);

                deserializeWatch.Start();
                _serializer.Deserialize(_stream);
                deserializeWatch.Stop();

                _stream.Seek(0, SeekOrigin.Begin);
            }

            Console.WriteLine($"Serialize Avg. Time: {serializeWatch.ElapsedMilliseconds / times}ms");
            Console.WriteLine($"Deserialize Avg. Time: {deserializeWatch.ElapsedMilliseconds / times}ms");
            Console.ReadKey();
        }
    }
}
