using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pnnl.Api.Operations;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Approvals.Http.Controllers;

namespace Pnnl.Api.Approvals.Http.Tests
{
    [TestClass]
    public class ProcessControllerTest
    {
        private Mock<ILogger<ProcessController>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IProcessFacade> _mockProcessFacade;
        private Mock<IContextPersonStore> _mockContextPersonStore;
        private Fixture fixture;

        public ProcessControllerTest()
        {
            SetUpMockLogger();
            SetUpMockHttpContext();
            SetUpMockContextPersonStoreBase();
            SetUpMockIProcessFacade();

            fixture = new Fixture();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsProcess_GivenValidList()
        {
            IList<int> validList = new List<int>() { 1 };
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process() { ProcessId = 1 });
            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessFacade // arrange
                               //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            //.ReturnsAsync( new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<Process>>()));
            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                var actual = await controller.GetByIdsAsync(validList); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PagedResult<Process>);
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_GivenInvalidList()
        {
            IList<int> invalidList = new List<int>() { 1 };
            PagedResult<Process> returnList = null;

            // arrange
            _mockProcessFacade
                //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(returnList);

            // act
            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                // assert
                var actual = await controller.GetByIdsAsync(invalidList); 
                var objectResult = actual as ObjectResult;

                Assert.AreEqual(StatusCodes.Status404NotFound, objectResult?.StatusCode);
            }
        }

        [TestMethod]
        public async Task GetByIdsAsync_ReturnsBadRequest_GivenNullId()
        {
            _mockProcessFacade // arrange
                .Setup(facade => facade.GetAsync(It.Is<IList<int>>(s => s == null), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentNullException());

            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                var actual = await controller.GetByIdsAsync(null); // act

                CommonAsserts(actual, StatusCodes.Status400BadRequest); // assert
            }
        }

        [TestMethod]
        public async Task GetByIdsAsync_ReturnsBadRequest_GivenOffsetOverCount()
        {
            IList<int> invalidList = new List<int>() { 1 };
            _mockProcessFacade // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.Is<int?>(s => s > invalidList.Count), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentException());
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                var actual = await controller.GetByIdsAsync(invalidList, 2); // act
                CommonAsserts(actual, StatusCodes.Status400BadRequest); // assert
            }
        }

        [TestMethod]
        public async Task GetByIdsAsync_ReturnsBadRequest_GivenZeroLimit()
        {
            IList<int> list = new List<int>();
            _mockProcessFacade // arrange
                .Setup(store => store.GetAsync(It.IsAny<IList<int>>(), It.IsAny<int?>(), It.Is<int?>(s => s == 0), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentException());
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                var actual = await controller.GetByIdsAsync(list, 2, 0); // act
                CommonAsserts(actual, StatusCodes.Status400BadRequest); // assert
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ThrowsException_GivenEmptyString()
        {
            // no need to configure the store, controller itself handles this case
            using (var controller = InstanceProcessController(_mockProcessFacade))
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
            using (var controller = InstanceProcessController(_mockProcessFacade))
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
            using (var controller = InstanceProcessController(_mockProcessFacade))
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
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process() { ProcessId = 1 });
            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessFacade // arrange
                               //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            //.ReturnsAsync( new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<Process>>()));
            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                var actual = await controller.GetByIdsAsync(validList, 99, It.IsAny<int>()); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PagedResult<Process>);
            }
        }

        [TestMethod]
        public async Task TerminateProcessAsync_ReturnsOkResponse_GivenValidProcessIdAndRequestedFromAuthorizedUser()
        {
            // arrange
            var expected = fixture.Create<TerminateProcessResponse>();

            int processId = expected.ProcessId;

            _mockProcessFacade 
                .Setup(facade => facade.TerminateProcessAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(expected);

            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                // act
                var actual = await controller.TerminateProcess(processId, false); 
                
                // assert
                CommonAsserts(actual, StatusCodes.Status200OK); 

                var result = actual as ObjectResult;

                var value = result?.Value as TerminateProcessResponse;

                Assert.IsNotNull(value);
                Assert.AreEqual(value.ProcessId, expected.ProcessId);
            }
        }

        [TestMethod]
        public async Task TerminateProcessAsync_ReturnsUnAuthorizedResponse_GivenValidProcessIdButRequestedFromUnAuthorizedUser()
        {
            // arrange
            var input = fixture.Create<TerminateProcessResponse>();

            int processId = input.ProcessId;

            _mockProcessFacade
                .Setup(facade => facade.TerminateProcessAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new UnauthorizedAccessException());

            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                // act
                var actual = await controller.TerminateProcess(processId, false);

                // assert
                Assert.IsNotNull(actual);

                var value = actual as StatusCodeResult;
                Assert.IsNotNull(value);

                Assert.AreEqual(value.StatusCode, StatusCodes.Status401Unauthorized);          
            }
        }

        [TestMethod]
        public async Task TerminateProcessAsync_ReturnsBadRequest_GivenInvalidInput()
        {
            // arrange
            var input = fixture.Create<TerminateProcessResponse>();

            int processId = input.ProcessId;

            _mockProcessFacade
                .Setup(facade => facade.TerminateProcessAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentException());

            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                // act
                var actual = await controller.TerminateProcess(processId, false);

                // assert
                CommonAsserts(actual, StatusCodes.Status400BadRequest);
            }
        }
        [TestMethod]
        public async Task TerminateProcessAsync_ReturnsForbidden_GivenCompleProcess()
        {
            // arrange
            var input = fixture.Create<TerminateProcessResponse>();

            int processId = input.ProcessId;

            _mockProcessFacade
                .Setup(facade => facade.TerminateProcessAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new InvalidOperationException());

            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                // act
                var actual = await controller.TerminateProcess(processId, false);

                // assert
                //CommonAsserts(actual, StatusCodes.Status403Forbidden);
            }
        }
        
        [TestMethod]
        public async Task SearchAsync_ReturnsValidRequest_GivenValidLimit()
        {
            ProcessFilter validFilter = new ProcessFilter();
            int validLimit = 1;
            int validOffset = 0;

            List<Process> returnList = new List<Process>
            {
                new Process() { ProcessId = 1 }
            };

            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessFacade // arrange
                .Setup(store => store.SearchAsync(It.IsAny<ProcessFilter>(), It.IsAny<Person>(), It.Is<int?>(s => s == validOffset), It.Is<int?>(s => s == validLimit), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            using (var controller = InstanceProcessController(_mockProcessFacade))
            {
                var actual = await controller.FilterAsync(It.IsAny<List<string>>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<ActorActionTaken>>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<string>>(),
                    validOffset, validLimit); // act

                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PagedResult<Process>);
            }
        }

        private dynamic InstanceProcessController(Mock<IProcessFacade> mockProcessStore)
        {
            var controller = new ProcessController(mockProcessStore.Object, _mockLogger.Object, _mockContextPersonStore.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };

            return controller;
        }

        private void SetUpMockIProcessFacade()
        {
            _mockProcessFacade = new Mock<IProcessFacade>();
            _mockProcessFacade.As<IDisposable>()
                .Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ProcessStore data store.");

            _mockProcessFacade = _mockProcessFacade.As<IProcessFacade>();
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
