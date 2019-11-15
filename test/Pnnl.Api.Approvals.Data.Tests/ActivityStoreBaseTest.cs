using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
//using FluentAssertions;

namespace Pnnl.Api.Approvals.Data.Tests
{
    [TestClass]
    public class ActivityStoreBaseTest
    {
        private readonly ActivityStoreBase _baseStore;

        public ActivityStoreBaseTest()
        {
            var mock = new Mock<ActivityStoreBase> { CallBase = true };
            _baseStore = mock.Object;
        }

        private Activity CreateTestItem()
        {
            var testItem = new Activity { ActivityId = 1 };
            return testItem;
        }

        private RoutingItem CreateTestRoutingItem()
        {
            var testItem = new RoutingItem {
                DocumentTypeName = "Test Routing"
            };
            return testItem;
        }

        [TestMethod]
        public void GetByIdAsync_NullId_ArgumentExeption()
        {
            var result = _baseStore.GetAsync(null, 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByIdAsync_OffsetGreaterThanResults_ArgumentExeption()
        {

            var result = _baseStore.GetAsync(new List<int>() { 1, },2,0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByIdAsync_LimitIsZero_ArgumentExeption()
        {

            var result = _baseStore.GetAsync(new List<int>() { 1, },2,0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }
    }
}