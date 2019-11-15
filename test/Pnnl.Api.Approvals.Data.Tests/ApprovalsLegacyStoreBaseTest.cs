using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pnnl.Api.Approvals;

namespace Pnnl.Api.Approvals.Data.Tests
{
    public class ApprovalsLegacyStoreBaseTest
    {
        private readonly ApprovalsLegacyStoreBase _baseStore;

        public ApprovalsLegacyStoreBaseTest()
        {
            var mock = new Mock<ApprovalsLegacyStoreBase> { CallBase = true };
            _baseStore = mock.Object;
        }

        private Process CreateTestItem()
        {
            var testItem = new Process { ProcessId = 1 };
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
        public void CreateAsync_NullRoutingItem_ArgumentNullException()
        {

            var result = _baseStore.CreateRoutingAsync(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void CreateAsync_RoutingItem_ArgumentNullException()
        {
            RoutingItem routingItem = CreateTestRoutingItem();

            var result = _baseStore.CreateRoutingAsync(routingItem);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(AggregateException), result.Exception.InnerException.GetType());
        }
        [TestMethod]
        public void CopyOf_CreateAsync_RoutingItem_ArgumentNullException()
        {
            RoutingItem routingItem = CreateTestRoutingItem();

            var result = _baseStore.CreateRoutingAsync(routingItem);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(AggregateException), result.Exception.InnerException.GetType());
        }
    }
}
