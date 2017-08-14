﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wire.Tests
{
    [TestClass]
    public class CyclicTests
    {
        [TestMethod]
        public void CanSerializeDeepCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));
            var root = new Root();
            var bar = new Bar();
            bar.Self = bar;
            bar.XYZ = 234;
            root.B1 = bar;
            root.B2 = bar;

            serializer.Serialize(root, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Root>(stream);
            Assert.AreEqual(actual.B1, actual.B1);
            Assert.AreEqual(actual.B1, actual.B2);
            Assert.AreEqual(actual.B1, actual.B1.Self);
            Assert.AreEqual(actual.B1, actual.B2.Self);
        }

        [TestMethod]
        public void CanSerializeDictionaryPreserveObjectReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));

            var arr1 = new[] {1, 2, 3};
            var arr2 = new[] { 1, 2, 3 };
            var obj = new Dictionary<int, int[]>
            {
                [1] = arr1,
                [2] = arr2,
                [3] = arr1,
            };

            serializer.Serialize(obj, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Dictionary<int, int[]>>(stream);

            Assert.AreEqual(res[1], res[3]);
            Assert.AreNotEqual(res[1], res[2]);
        }

        //From Orleans
        [TestMethod]
        public void CanSerializeDictionaryPreserveObjectReferences2()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));

            var val = new List<string> { "first", "second" };

            var val2 = new List<string> { "first", "second" };

            var source = new Dictionary<string, List<string>>
            {
                ["one"] = val,
                ["two"] = val,
                ["three"] = val2
            };


            serializer.Serialize(source, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Dictionary<string, List<string>>>(stream);

            Assert.AreEqual(res["one"], res["two"]);
            Assert.AreNotEqual(res["one"], res["three"]);
        }


        [TestMethod]
        public void CanSerializeCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));
            var bar = new Bar();
            bar.Self = bar;
            bar.XYZ = 234;

            serializer.Serialize(bar, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Bar>(stream);
            Assert.AreEqual(actual, actual.Self);
            Assert.AreEqual(bar.XYZ, actual.XYZ);
        }

        [TestMethod]
        public void CanSerializeMultiLevelCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(preserveObjectReferences: true));
            var a = new A();
            var b = new B();
            a.B = b;
            b.A = a;

            serializer.Serialize(a, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<A>(stream);
            Assert.AreEqual(actual, actual.B.A);
        }
    }

    public class Root
    {
        public Bar B1 { get; set; }
        public long Baz { get; set; }
        public Bar B2 { get; set; }
    }

    public class Bar
    {
        public long Boo { get; set; }
        public Bar Self { get; set; }
        public int XYZ { get; set; }
    }

    public class A
    {
        public B B { get; set; }
    }

    public class B
    {
        public A A { get; set; }
    }
}
 