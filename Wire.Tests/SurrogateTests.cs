﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.Serialization;

namespace Wire.Tests
{
    public interface IOriginal
    {
        ISurrogate ToSurrogate();
    }

    public interface ISurrogate
    {
        IOriginal FromSurrogate();
    }

    public class Foo : IOriginal
    {
        public string Bar { get; set; }

        public ISurrogate ToSurrogate()
        {
            return new FooSurrogate
            {
                Bar = Bar
            };
        }
    }

    public class FooSurrogate : ISurrogate
    {
        public string Bar { get; set; }

        public IOriginal FromSurrogate()
        {
            return Restore();
        }

        public static FooSurrogate FromFoo(Foo foo)
        {
            return new FooSurrogate
            {
                Bar = foo.Bar
            };
        }

        public Foo Restore()
        {
            return new Foo
            {
                Bar = Bar
            };
        }
    }

    [TestClass]
    public class SurrogateTests
    {
        [TestMethod]
        public void CanSerializeWithSurrogate()
        {
            var surrogateHasBeenInvoked = false;
            var surrogates = new[]
            {
                Surrogate.Create<Foo, FooSurrogate>(FooSurrogate.FromFoo, surrogate =>
                {
                    surrogateHasBeenInvoked = true;
                    return surrogate.Restore();
                })
            };
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(surrogates: surrogates));
            var foo = new Foo
            {
                Bar = "I will be replaced!"
            };

            serializer.Serialize(foo, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Foo>(stream);
            Assert.AreEqual(foo.Bar, actual.Bar);
            Assert.IsTrue(surrogateHasBeenInvoked);
        }

        [TestMethod]
        public void CanSerializeWithInterfaceSurrogate()
        {
            var surrogateHasBeenInvoked = false;
            var surrogates = new[]
            {
                Surrogate.Create<IOriginal, ISurrogate>(from => from.ToSurrogate(), surrogate =>
                {
                    surrogateHasBeenInvoked = true;
                    return surrogate.FromSurrogate();
                })
            };
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(surrogates: surrogates));
            var foo = new Foo
            {
                Bar = "I will be replaced!"
            };

            serializer.Serialize(foo, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Foo>(stream);
            Assert.AreEqual(foo.Bar, actual.Bar);
            Assert.IsTrue(surrogateHasBeenInvoked);
        }
    }
}
