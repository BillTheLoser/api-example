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
using System.Linq;

namespace Pnnl.Api.Approvals.Http.Test
{
    [TestClass]
    public class ProcessNodeControllerTest
    {
        private Mock<ILogger<ProcessNodeController>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IProcessNodeStore> _mockProcessNodeStore;
        private Mock<IContextPersonStore> _mockContextPersonStore;
        private Fixture fixture;

        public ProcessNodeControllerTest()
        {
            fixture = new Fixture();

            SetUpMockLogger();
            SetUpMockHttpContext();
            SetUpMockContextPersonStoreBase();
            SetUpMockIProcessFacade();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsProcessNodes_GivenValidList()
        {
            // Arrange
            var processIds = GetProcessIds(2);

            var result = GetProcessNodes(processIds, 10);
            
            _mockProcessNodeStore
                .Setup(store => store.GetByIdsAsync(It.Is<IList<int>>(s => s == processIds), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            using (var controller = InstantiateProcessNodeController(_mockProcessNodeStore))
            {
                // Act
                var actual = await controller.GetByIdsAsync(processIds);

                // Assert
                CommonAsserts(actual, StatusCodes.Status200OK); 

                var objectResult = actual as ObjectResult;

                Assert.IsNotNull(objectResult?.Value);
            }
        }

        [TestMethod]
        public async Task GetAsync_ReturnsPagedProcessNodesWithValidCount_GivenValidList()
        {
            // Arrange
            var processIds = GetProcessIds(2);

            int offset = 1, limit = 3;

            var processNodes = GetProcessNodes(processIds, 10);
            var pagedProcessNodes = processNodes
                            .Results
                            .Skip(offset * limit)
                            .Take(limit)
                            .ToList();

            var result = new PagedResult<ProcessNodeResult>(offset, limit, processNodes.Count(), pagedProcessNodes);

            _mockProcessNodeStore
                .Setup(store => store.GetByIdsAsync(It.Is<IList<int>>(s => s == processIds), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            using (var controller = InstantiateProcessNodeController(_mockProcessNodeStore))
            {
                // Act
                var actual = await controller.GetByIdsAsync(processIds);

                // Assert
                CommonAsserts(actual, StatusCodes.Status200OK);

                var objectResult = actual as ObjectResult;

                var value = objectResult?.Value as PagedResult<ProcessNodeResult>;

                Assert.IsNotNull(value);

                var actualResults = value.Results as IList<ProcessNodeResult>;
                Assert.IsNotNull(actualResults);
                Assert.AreEqual(actualResults.Count, 3);

                var expectedResults = result.Results as IList<ProcessNodeResult>;
                Assert.IsNotNull(actualResults[0].Nodes);
                Assert.AreNotEqual(actualResults[0].Nodes.Count, 0);
            }
        }

        private IList<int> GetProcessIds(int count)
        {
            return fixture
                    .CreateMany<int>(count)
                    .ToList();
        }

        private PagedResult<ProcessNodeResult> GetProcessNodes(IList<int> processIds, int count)
        {
            var processesNodes = fixture
                                .CreateMany<ProcessNodeResult>(count)
                                .ToList();

            for (int i = 0; i < processesNodes.Count; i++)
                processesNodes[i].ProcessId = i <= 6 ? processIds[0] : processIds[1];

            PagedResult<ProcessNodeResult> result = new PagedResult<ProcessNodeResult>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), processesNodes);

            return result;
        }

        [TestMethod]        
        public async Task GetByIdAsync_ThrowsException_GivenNullInput()
        {
            using (var controller = InstantiateProcessNodeController(_mockProcessNodeStore))
            {
                try
                {
                    var actual = await controller.GetByIdsAsync(null, 0, 10); // act
                    Assert.Fail();  // If this line is hit, then the test case failed.
                }
                catch (Exception ex) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ThrowsException_GivenEmptyListInput()
        {
            using (var controller = InstantiateProcessNodeController(_mockProcessNodeStore))
            {
                try
                {
                    var actual = await controller.GetByIdsAsync(new List<int>(), 0, 10); // act
                    Assert.Fail();  // If this line is hit, then the test case failed.
                }
                catch (Exception ex) { } // this is the correct behavior for this test
            }
        }

        private dynamic InstantiateProcessNodeController(Mock<IProcessNodeStore> mockProcessNodeStore)
        {
            var controller = new ProcessNodeController(_mockLogger.Object, mockProcessNodeStore.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };

            return controller;
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
            _mockLogger = new Mock<ILogger<ProcessNodeController>>();
            _mockLogger
                .As<ILogger<ProcessNodeController>>()
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

        private void SetUpMockIProcessFacade()
        {
            _mockProcessNodeStore = new Mock<IProcessNodeStore>();
            _mockProcessNodeStore.As<IDisposable>()
                .Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ProcessNode data store.");

            _mockProcessNodeStore = _mockProcessNodeStore.As<IProcessNodeStore>();
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
