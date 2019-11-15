using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pnnl.Api.Operations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pnnl.Api.Approvals.Http.Controllers;
using Pnnl.Api.Approvals.Data.Interfaces;

namespace Pnnl.Api.Approvals.RouteItem.Test
{
    [TestClass]
    public class RouteItemControllerTest
    {
        private Mock<ILogger<ProcessController>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IProcessFacade> _mockRouteItemStore;
        private Mock<IContextPersonStore> _mockContextPersonStore;

        public RouteItemControllerTest()
        {
            _mockRouteItemStore = MockIRouteItemStoreBase(); ;
            SetUpMockLogger();
            SetUpMockHttpContext();
            SetUpMockContextPersonStoreBase();
        }

        //[TestMethod]
        //public async Task GetAsync_ReturnsRouteItem_GivenValidList()
        //{
        //    IList<int> validList = new List<int>() { 1 };
        //    List<RouteItem> returnList = new List<RouteItem>();
        //    returnList.Add(new RouteItem() { RouteItemId = 1 });
        //    PagedResult<RouteItem> result =  new PagedResult<RouteItem>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

        //    _mockRouteItemStore // arrange
        //        //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int>(),It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .ReturnsAsync(result);
        //        //.ReturnsAsync( new PagedResult<RouteItem>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<RouteItem>>()));
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        var actual = await controller.GetByIdsAsync(validList); // act
        //        CommonAsserts(actual, StatusCodes.Status200OK); // assert
        //        var objectResult = actual as ObjectResult;
        //        Assert.IsNotNull(objectResult?.Value as PagedResult<RouteItem>);
        //    }

        //}
        //[TestMethod]
        //public async Task GetByIdAsync_ReturnsNull_GivenInvalidList()
        //{
        //    IList<int> invalidList = new List<int>() { 1 };
        //    PagedResult< RouteItem> returnList = null;

        //    _mockRouteItemStore // arrange
        //        //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.IsAny<int>(),It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .ReturnsAsync(returnList);
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        var actual = await controller.GetByIdsAsync(invalidList); // act
        //        var objectResult = actual as ObjectResult;
        //        Assert.AreEqual(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        //    }
        //}
        //[TestMethod]
        //public async Task GetByIdsAsync_ReturnsBadRequest_GivenNullId()
        //{
        //    _mockRouteItemStore // arrange
        //        .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == null), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .Throws(new ArgumentNullException());
        //    // no need to configure the store, controller itself handles this case
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        var actual = await controller.GetByIdsAsync(null); // act
        //        CommonAsserts(actual, StatusCodes.Status400BadRequest); // assert
        //    }
        //}
        //[TestMethod]
        //public async Task GetByIdsAsync_ReturnsBadRequest_GivenOffsetOverCount()
        //{
        //    IList<int> invalidList = new List<int>() { 1 };
        //    _mockRouteItemStore // arrange
        //        .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.Is<int>(s => s > invalidList.Count), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .Throws(new ArgumentException());
        //    // no need to configure the store, controller itself handles this case
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        var actual = await controller.GetByIdsAsync(invalidList, 2); // act
        //        CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
        //    }
        //}
        //[TestMethod]
        //public async Task GetByIdsAsync_ReturnsBadRequest_GivenZeroLimit()
        //{
        //    IList<int> list = new List<int>();
        //    _mockRouteItemStore // arrange
        //        .Setup(store => store.GetAsync(It.IsAny<IList<int>>(), It.IsAny<int>(), It.Is<int>(s => s == 0), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .Throws(new ArgumentException());
        //    // no need to configure the store, controller itself handles this case
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        var actual = await controller.GetByIdsAsync(list, 2, 0); // act
        //        CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
        //    }
        //}
        //[TestMethod]
        //public async Task GetByIdAsync_ThrowsException_GivenEmptyString()
        //{
        //    // no need to configure the store, controller itself handles this case
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        try
        //        {
        //            var actual = await controller.GetByIdsAsync(String.Empty); // act
        //            Assert.Fail();
        //        }
        //        catch (Exception ex) { } // this is the correct behavior for this test
        //    }
        //}

        //[TestMethod]
        //public async Task GetByIdAsync_ReturnsException()
        //{
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        try
        //        {
        //            await controller.GetByIdsAsync("TriggersException");
        //            Assert.Fail();
        //        }
        //        catch (Exception ex) { } // this is the correct behavior for this test
        //    }
        //}

        //[TestMethod]
        //public async Task GetByIdAsync_MaxLimit_ReturnsException()
        //{
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        try
        //        {
        //            await controller.GetByIdsAsync(It.IsAny<int>(), 101);
        //            Assert.Fail();
        //        }
        //        catch (Exception ex) { } // this is the correct behavior for this test
        //    }
        //}

        //[TestMethod]
        //public async Task GetByIdsAsync_ReturnsValidRequest_GivenValidLimit()
        //{
        //    IList<int> validList = new List<int>() { 1 };
        //    List<RouteItem> returnList = new List<RouteItem>();
        //    returnList.Add(new RouteItem() { RouteItemId = 1 });
        //    PagedResult<RouteItem> result =  new PagedResult<RouteItem>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

        //    _mockRouteItemStore // arrange
        //        //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int>(),It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
        //        .ReturnsAsync(result);
        //        //.ReturnsAsync( new PagedResult<RouteItem>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<RouteItem>>()));
        //    using (var controller = InstanceRouteItemController(_mockRouteItemStore))
        //    {
        //        var actual = await controller.GetByIdsAsync(validList, 99, It.IsAny<int>()); // act
        //        CommonAsserts(actual, StatusCodes.Status200OK); // assert
        //        var objectResult = actual as ObjectResult;
        //        Assert.IsNotNull(objectResult?.Value as PagedResult<RouteItem>);
        //    }
        //}

        [TestMethod]
        public async Task CreateRouting_ReturnsNotFound_WithNullRouteItemId()
        {
            Process result = null;
            _mockRouteItemStore // arrange
                .Setup(store => store.CreateRoutingAsync(It.Is<RoutingItem>(s => s.DocumentTypeName == "Invalid"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceRouteItemController(_mockRouteItemStore))
            {
                var actual = await controller.CreateRouting(new RoutingItem() { DocumentTypeName = "Invalid" }); // act
                CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
            }
        }

        [TestMethod]
        public async Task CreateRouting_ReturnsBadRequest_ThrowsException()
        {
            _mockRouteItemStore // arrange
                .Setup(store => store.CreateRoutingAsync(It.Is<RoutingItem>(s => s.DocumentTypeName == "Invalid"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentNullException());
            // no need to configure the store, controller itself handles this case
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceRouteItemController(_mockRouteItemStore))
            {
                var actual = await controller.CreateRouting(new RoutingItem() { DocumentTypeName = "Invalid" }); // act
                CommonAsserts(actual, StatusCodes.Status400BadRequest); // assert
            }
        }

        [TestMethod]
        public async Task CreateRouting_ReturnsBadRequest_ThrowsAggregateException()
        {
            _mockRouteItemStore // arrange
                .Setup(store => store.CreateRoutingAsync(It.Is<RoutingItem>(s => s.BeneficiaryHanfordId == "11"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new AggregateException());
            // no need to configure the store, controller itself handles this case
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceRouteItemController(_mockRouteItemStore))
            {
                var actual = await controller.CreateRouting(new RoutingItem() { BeneficiaryHanfordId = "11" }); // act
                CommonAsserts(actual, StatusCodes.Status400BadRequest); // assert
            }
        }



        private dynamic InstanceRouteItemController(Mock<IProcessFacade> mockRouteItemStore)
        {
            var controller = new ProcessController(mockRouteItemStore.Object, _mockLogger.Object, _mockContextPersonStore.Object)
            { ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object } };
            return controller;
        }

        private Mock<IProcessFacade> MockIRouteItemStoreBase()
        {
            var mockIRouteItemStore = new Mock<IProcessFacade>();
            mockIRouteItemStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose RouteItemStore data store.");
            mockIRouteItemStore = mockIRouteItemStore.As<IProcessFacade>();
            return mockIRouteItemStore;
        }

        private void CommonAsserts(IActionResult actual, int statusCode)
        {
            Assert.IsNotNull(actual);
            var objectResult = actual as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(statusCode, objectResult?.StatusCode);
        }

        private void SetUpMockLogger()
        {
            _mockLogger = new Mock<ILogger<ProcessController>>();
            _mockLogger
                .As<ILogger<ProcessController>>()
                .Setup(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }

        private void SetUpMockHttpContext()
        {
            _mockHttpContext = new Mock<HttpContext>();

            //  Set up a unique identifier for this http request to represent in the trace log.
            string traceIdentifier = Guid.NewGuid().ToString("N");
            _mockHttpContext
                .SetupGet(context => context.TraceIdentifier)
                .Returns(traceIdentifier);

            // Set up the httpcontext to send a cancellation token when the connection is aborted.
            _mockHttpContext
                .SetupGet(context => context.RequestAborted)
                .Returns(CancellationToken.None);

            _mockHttpContext
                .SetupGet(context => context.Items)
                .Returns(new Dictionary<object, object>());
        }

        private void SetUpMockContextPersonStoreBase()
        {
            _mockContextPersonStore = new Mock<IContextPersonStore>();
            _mockContextPersonStore
                .As<IContextPersonStore>()
                .Setup(personStore => personStore.Get(It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Returns(new Person() { EmployeeId = "Fred0001" });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }
        }
    }
}
