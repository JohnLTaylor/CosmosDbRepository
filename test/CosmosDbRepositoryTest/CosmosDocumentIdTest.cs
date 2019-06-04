using CosmosDbRepository;
using CosmosDbRepository.Types;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace CosmosDbRepositoryTest
{
    [TestClass]
    public class CosmosDocumentIdTest
    {
        [TestMethod]
        public void GuidIdTests()
        {
            Guid value = Guid.NewGuid();
            DocumentId id = value;
            (id == value).Should().BeTrue();
            (id != value).Should().BeFalse();
            id.Equals(value).Should().BeTrue();
            id.Equals((object)value).Should().BeTrue();
        }

        [TestMethod]
        public void StringIdTests()
        {
            string value = "MyId";
            DocumentId id = value;
            (id == value).Should().BeTrue();
            (id != value).Should().BeFalse();
            id.Equals(value).Should().BeTrue();
            id.Equals((object)value).Should().BeTrue();
        }

        [TestMethod]
        public void IntIdTests()
        {
            int value = 123;
            DocumentId id = value;
            (id == value).Should().BeTrue();
            (id != value).Should().BeFalse();
            id.Equals(value).Should().BeTrue();
            id.Equals((object)value).Should().BeTrue();
        }
    }
}
