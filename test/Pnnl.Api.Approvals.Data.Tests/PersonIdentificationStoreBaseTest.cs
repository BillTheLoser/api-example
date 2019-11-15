using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Caching.Memory;
//using FluentAssertions;

namespace Pnnl.Api.Approvals.Data.Tests
{
    [TestClass]
    public class PersonIdentificationStoreBaseTest
    {
        private readonly PersonIdentificationStoreBase _baseStore;

        public PersonIdentificationStoreBaseTest()
        {
            var mockCache = new Mock<IMemoryCache>(MockBehavior.Loose);
            var mock = new Mock<PersonIdentificationStoreBase>(MockBehavior.Loose, mockCache.Object);
            _baseStore = mock.Object;
        }

        private PersonIdentification CreateTestItem()
        {
            var testItem = new PersonIdentification
            {
                EmployeeId = "99999",
                HanfordId = "7777777",
                Domain = "PNL",
                NetworkId = "LLLLDDDD"
            };

            return testItem;
        }

        private RoutingItem CreateTestRoutingItem()
        {
            var testItem = new RoutingItem
            {
                DocumentTypeName = "Test Routing"
            };
            return testItem;
        }

        [TestMethod]
        public void GetByIdAsync_NullId_ArgumentExeption()
        {
            var result = _baseStore.GetByIdAsync(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByEmployeeIdAsync_NullId_ArgumentExeption()
        {
            var result = _baseStore.GetByEmployeeIdAsync(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByHanfordIdAsync_NullId_ArgumentExeption()
        {
            var result = _baseStore.GetByEmployeeIdAsync(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByNetworkIdAsync_NullNetworkId_ArgumentExeption()
        {
            var result = _baseStore.GetByNetworkIdAsync(It.IsAny<string>(), null);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByNetworkIdAsync_NullDomain_ArgumentExeption()
        {
            var result = _baseStore.GetByNetworkIdAsync(null, It.IsAny<string>());
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }
    }
}