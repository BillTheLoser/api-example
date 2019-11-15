using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Pnnl.Api.Approvals.Data.Tests
{
    [TestClass]
    public class ProcessNodeStoreBaseTest
    {
        private readonly ProcessNodeStoreBase _baseStore;
        
        public ProcessNodeStoreBaseTest()
        {
            var mock = new Mock<ProcessNodeStoreBase> { CallBase = true };
            _baseStore = mock.Object;
        }

        [TestMethod]
        public void GetByIdAsync_NullId_ArgumentException()
        {
            var result = _baseStore.GetByIdsAsync(null, 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByIdAsync_EmptyList_ArgumentExeption()
        {
            var result = _baseStore.GetByIdsAsync(new List<int>(), 0, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void GetByIdAsync_OverLimit_ArgumentExeption()
        {
            var result = _baseStore.GetByIdsAsync(new List<int>(), 0, 101);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentException), result.Exception.InnerException.GetType());
        }
    }
}
