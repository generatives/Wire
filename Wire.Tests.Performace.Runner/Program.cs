using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wire.Tests.Performance;

namespace Wire.Tests.Performace.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(500);
            SerializeComplexObjectsBenchmark complexObjects = new SerializeComplexObjectsBenchmark();

            complexObjects.Setup();
            complexObjects.Serialize_LargeObject();
            complexObjects.Cleanup();

            complexObjects.Setup();
            complexObjects.Serialize_LargeStruct();
            complexObjects.Cleanup();
        }
    }
}
