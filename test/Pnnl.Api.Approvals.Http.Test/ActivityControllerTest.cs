using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Approvals.Http.Controllers;
using Pnnl.Api.Operations;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Http.Tests
{
    [TestClass]
    public class ActivityControllerTest
    {
        private Mock<ILogger<ActivityController>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IActivityFacade> _mockActivityFacade;
        private Mock<IContextPersonStore> _mockContextPersonStore;

        public ActivityControllerTest()
        {
            _mockActivityFacade = MockIActivityStoreBase(); ;
            SetUpMockLogger();
            SetUpMockHttpContext();
            SetUpMockContextPersonStoreBase();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsActivity_GivenValidList()
        {
            IList<int> validList = new List<int>() { 1 };
            List<Activity> returnList = new List<Activity>();
            returnList.Add(new Activity() { ActivityId = 1 });
            PagedResult<Activity> result =  new PagedResult<Activity>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockActivityFacade // arrange
                //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int?>(),It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
                //.ReturnsAsync( new PagedResult<Activity>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<Activity>>()));
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                var actual = await controller.GetByIdsAsync(validList); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PagedResult<Activity>);
            }

        }
        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_GivenInvalidList()
        {
            IList<int> invalidList = new List<int>() { 1 };
            PagedResult< Activity> returnList = null;

            _mockActivityFacade // arrange
                //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.IsAny<int?>(),It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(returnList);
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                var actual = await controller.GetByIdsAsync(invalidList); // act
                var objectResult = actual as ObjectResult;
                Assert.AreEqual(StatusCodes.Status404NotFound, objectResult?.StatusCode);
            }
        }
        [TestMethod]
        public async Task GetByIdsAsync_ReturnsBadRequest_GivenNullId()
        {
            _mockActivityFacade // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == null), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentNullException());
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                var actual = await controller.GetByIdsAsync(null); // act
                CommonAsserts(actual, StatusCodes.Status400BadRequest); // assert
            }
        }
        [TestMethod]
        public async Task GetByIdsAsync_ReturnsBadRequest_GivenOffsetOverCount()
        {
            IList<int> invalidList = new List<int>() { 1 };
            _mockActivityFacade // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.Is<int>(s => s > invalidList.Count), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentException());
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                var actual = await controller.GetByIdsAsync(invalidList, 2); // act
                CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
            }
        }
        [TestMethod]
        public async Task GetByIdsAsync_ReturnsBadRequest_GivenZeroLimit()
        {
            IList<int> list = new List<int>();
            _mockActivityFacade // arrange
                .Setup(store => store.GetAsync(It.IsAny<IList<int>>(), It.IsAny<int?>(), It.Is<int?>(s => s == 0), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentException());
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                var actual = await controller.GetByIdsAsync(list, 2, 0); // act
                CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
            }
        }
        [TestMethod]
        public async Task GetByIdAsync_ThrowsException_GivenEmptyString()
        {
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                try
                {
                    var actual = await controller.GetByIdsAsync(String.Empty); // act
                    Assert.Fail();
                }
                catch (Exception ex) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsException()
        {
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                try
                {
                    await controller.GetByIdsAsync("TriggersException");
                    Assert.Fail();
                }
                catch (Exception ex) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_MaxLimit_ReturnsException()
        {
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                try
                {
                    await controller.GetByIdsAsync(It.IsAny<int>(), 101);
                    Assert.Fail();
                }
                catch (Exception ex) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByIdsAsync_ReturnsValidRequest_GivenValidLimit()
        {
            IList<int> validList = new List<int>() { 1 };
            List<Activity> returnList = new List<Activity>();
            returnList.Add(new Activity() { ActivityId = 1 });
            PagedResult<Activity> result =  new PagedResult<Activity>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockActivityFacade // arrange
                //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int?>(),It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
                //.ReturnsAsync( new PagedResult<Activity>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<Activity>>()));
            using (var controller = InstanceActivityController(_mockActivityFacade))
            {
                var actual = await controller.GetByIdsAsync(validList, 99, It.IsAny<int>()); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PagedResult<Activity>);
            }
        }


        private dynamic InstanceActivityController(Mock<IActivityFacade> mockActivityFacade)
        {
            var controller = new ActivityController(mockActivityFacade.Object, _mockLogger.Object, _mockContextPersonStore.Object)
            { ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object } };
            return controller;
        }

        private Mock<IActivityFacade> MockIActivityStoreBase()
        {
            var mockIActivityStore = new Mock<IActivityFacade>();
            mockIActivityStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ActivityStore data store.");
            mockIActivityStore = mockIActivityStore.As<IActivityFacade>();
            return mockIActivityStore;
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
            _mockLogger = new Mock<ILogger<ActivityController>>();
            _mockLogger
                .As<ILogger<ActivityController>>()
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
