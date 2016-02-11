using System;
using System.IO;
using Stardust.Core.Sandbox.Serializer;
using Xunit;

namespace Stardust.Core.SandboxTest
{
    public class FlatFileSerializerTest
    {
        private const string FilePath = @"C:\Temp\ffSerializeTest.txt";
        [Fact]
        [Trait("Flat file serializer", "Serialize")]
        public void CreateFileWith2LinesAndHeaders()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            FlatFileSerializer.Serialize(FilePath, List );
            var content = File.ReadAllLines(FilePath);
            Assert.Equal(3, content.Length);
        }

        private static TestDataItem[] List
        {
            get
            {
                return new[] { new TestDataItem { Address = "Setesdalsveien 503", City = "Kristiansand", Name = "Jonas Syrstad", ZipCode = "4619" }, new TestDataItem { Address = "Setesdalsveien 503", City = "Kristiansand", Name = "Kristine Ormshammer", ZipCode =null} };
            }
        }

        [Fact]
        [Trait("Flat file serializer", "Deserialize")]
        public void ReadFileWith2LinesAndHeaders()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            FlatFileSerializer.Serialize(FilePath, List);
            var content = File.ReadAllText(FilePath);
            var deserialized = FlatFileSerializer.Deserialize<TestDataItem>(content);
            Assert.Equal(2, deserialized.Length);
        }

        [Fact]
        [Trait("Flat file serializer", "Deserialize")]
        public void CreateFileWithNullThrowException()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            Assert.Throws<NullReferenceException>(()=> FlatFileSerializer.Serialize<TestDataItem>(FilePath, null));
        }
    }
}