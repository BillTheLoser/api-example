using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pnnl.Api.Approvals;

namespace Pnnl.Api.Approvals.Data.Tests
{
    [TestClass]
    public class ProcessStoreBaseTest
    {
        private readonly ProcessStoreBase _baseStore;

        public ProcessStoreBaseTest()
        {
            var mock = new Mock<ProcessStoreBase> { CallBase = true };
            _baseStore = mock.Object;
        }

        private Process CreateTestItem()
        {
            var testItem = new Process { ProcessId = 1 };
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

        [TestMethod]
        public void SearchWithActorAsync_NullId_ArgumentExeption()
        {
            var result = _baseStore.SearchWithActorAsync(null, 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void SearchWithActorAsync_OffsetGreaterThanResults_ArgumentExeption()
        {

            var result = _baseStore.SearchWithActorAsync(new ProcessFilter(), -12, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void SearchWithActorAsync_LimitIsZero_ArgumentExeption()
        {

            var result = _baseStore.SearchWithActorAsync(new ProcessFilter(), 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void SearchWithUserAsync_NullId_ArgumentExeption()
        {
            var result = _baseStore.SearchWithUserAsync(null, 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void SearchWithUserAsync_OffsetGreaterThanResults_ArgumentExeption()
        {

            var result = _baseStore.SearchWithUserAsync(new ProcessFilter(), -12, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void SearchWithUserAsync_LimitIsZero_ArgumentExeption()
        {

            var result = _baseStore.SearchWithUserAsync(new ProcessFilter(), 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }



        [TestMethod]
        public void SearchWithOriginatorAsync_NullId_ArgumentExeption()
        {
            var result = _baseStore.SearchWithOriginatorAsync(null, 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void SearchWithOriginatorAsync_OffsetGreaterThanResults_ArgumentExeption()
        {

            var result = _baseStore.SearchWithOriginatorAsync(new ProcessFilter(), -12, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void SearchWithOriginatorAsync_LimitIsZero_ArgumentExeption()
        {

            var result = _baseStore.SearchWithOriginatorAsync(new ProcessFilter(), 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void TerminateAsync_NegativeProcessId_ArgumentExeption()
        {

            var result = _baseStore.TerminateAsync(-1, It.IsAny<string>(), It.IsAny<string>());
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void TerminateAsync_NullUserId_ArgumentNullExeption()
        {

            var result = _baseStore.TerminateAsync(1, null, "string");
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void TerminateAsync_EmptyUserId_ArgumentNullExeption()
        {

            var result = _baseStore.TerminateAsync(1, string.Empty, It.IsAny<string>());
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void TerminateAsync_NullStatus_ArgumentNullExeption()
        {

            var result = _baseStore.TerminateAsync(1, It.IsAny<string>(), null);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void TerminateAsync_EmptyStatus_ArgumentNullExeption()
        {

            var result = _baseStore.TerminateAsync(1, It.IsAny<string>(), string.Empty);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }


    }
}