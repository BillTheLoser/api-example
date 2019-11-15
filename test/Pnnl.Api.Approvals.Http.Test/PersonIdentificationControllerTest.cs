using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Approvals.Http.Controllers;
using Pnnl.Api.Operations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Http.Tests
{
    [TestClass]
    public class PersonIdentificationControllerTest
    {
        private Mock<ILogger<PersonIdentificationController>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IPersonIdentificationStore> _mockPersonIdentificationStore;
        private Mock<IContextPersonStore> _mockContextPersonStore;

        public PersonIdentificationControllerTest()
        {
            _mockPersonIdentificationStore = MockIPersonIdentificationStoreBase(); ;
            SetUpMockLogger();
            SetUpMockHttpContext();
            SetUpMockContextPersonStoreBase();
        }

        [TestMethod]
        public async Task GetByEmployeeIdAsync_ReturnsPersonIdentification_GivenValid()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByEmployeeIdAsync(It.Is<string>(s => s == "Valid"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(GetPersonIdentification());
            //.ReturnsAsync( new PagedResult<PersonIdentification>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<PersonIdentification>>()));
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByEmployeeIdAsync("Valid"); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PersonIdentification);
            }
        }

        [TestMethod]
        public async Task GetByHanfordIdAsync_ReturnsPersonIdentification_GivenValid()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByHanfordIdAsync(It.Is<string>(s => s == "Valid"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(GetPersonIdentification());
            //.ReturnsAsync( new PagedResult<PersonIdentification>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<PersonIdentification>>()));
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByHanfordIdAsync("Valid"); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PersonIdentification);
            }
        }

        [TestMethod]
        public async Task GetByNetworkIdAsync_ReturnsPersonIdentification_GivenValid()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByNetworkIdAsync(It.Is<string>(s => s == "Valid"), It.Is<string>(s => s == "Valid"),It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(GetPersonIdentification());
            //.ReturnsAsync( new PagedResult<PersonIdentification>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<PersonIdentification>>()));
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByNetworkIdAsync("Valid", "Valid"); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PersonIdentification);
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsPersonIdentification_GivenValid()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByIdAsync(It.Is<string>(s => s == "Valid"),It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(GetPersonIdentification());
            //.ReturnsAsync( new PagedResult<PersonIdentification>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<PersonIdentification>>()));
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByIdAsync("Valid"); // act
                CommonAsserts(actual, StatusCodes.Status200OK); // assert
                var objectResult = actual as ObjectResult;
                Assert.IsNotNull(objectResult?.Value as PersonIdentification);
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsBadRequest_GivenInvalidId()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByIdAsync(It.Is<String>(s => s == "InValid"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(()=> null);
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByIdAsync("InValid"); // act
                CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
            }
        }

        [TestMethod]
        public async Task GetByHanfordIdAsync_ReturnsBadRequest_GivenInvalidId()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByHanfordIdAsync(It.Is<String>(s => s == "InValid"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(()=> null);
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByIdAsync( "InValid"); // act
                CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
            }
        }

        [TestMethod]
        public async Task GetByEmployeeIdAsync_ReturnsBadRequest_GivenInvalidId()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByEmployeeIdAsync(It.Is<String>(s => s == "InValid"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(()=> null);
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByIdAsync( "InValid"); // act
                CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
            }
        }

        [TestMethod]
        public async Task GetByNetworkIdAsync_ReturnsBadRequest_GivenInvalidId()
        {
            _mockPersonIdentificationStore // arrange
                .Setup(store => store.GetByNetworkIdAsync(It.Is<string>(s => s == "Invalid"), It.Is<string>(s => s == "Invalid"),It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(()=> null);
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                var actual = await controller.GetByNetworkIdAsync( "InValid", "InValid"); // act
                CommonAsserts(actual, StatusCodes.Status404NotFound); // assert
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ThrowsException_GivenEmptyString()
        {
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                try
                {
                    var actual = await controller.GetByIdAsync(String.Empty); // act
                    Assert.Fail();
                }
                catch (Exception ) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByEmployeeIdAsync_ThrowsException_GivenEmptyString()
        {
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                try
                {
                    var actual = await controller.GetByEmployeeIdAsync(String.Empty); // act
                    Assert.Fail();
                }
                catch (Exception ) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByHanfordIdAsync_ThrowsException_GivenEmptyString()
        {
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                try
                {
                    var actual = await controller.GetByHanfordIdAsync(String.Empty); // act
                    Assert.Fail();
                }
                catch (Exception ) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByNetworkIdAsync_ThrowsException_GivenEmptyString()
        {
            // no need to configure the store, controller itself handles this case
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                try
                {
                    var actual = await controller.GetByNetworkIdAsync(String.Empty); // act
                    Assert.Fail();
                }
                catch (Exception ) { } // this is the correct behavior for this test
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsException()
        {
            using (var controller = InstancePersonIdentificationController(_mockPersonIdentificationStore))
            {
                try
                {
                    await controller.GetByIdAsync("TriggersException");
                    Assert.Fail();
                }
                catch (Exception ) { } // this is the correct behavior for this test
            }
        }


        private PersonIdentification GetPersonIdentification()
        {
            return new PersonIdentification()
            {
                EmployeeId = "99999"
                        ,
                HanfordId = "7777777"
                        ,
                Domain = "PNL"
                        ,
                NetworkId = "LLLLDDDD"
            };
        }

        private dynamic InstancePersonIdentificationController(Mock<IPersonIdentificationStore> mockPersonIdentificationStore)
        {
            var controller = new PersonIdentificationController(mockPersonIdentificationStore.Object, _mockLogger.Object)
            { ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object } };
            return controller;
        }

        private Mock<IPersonIdentificationStore> MockIPersonIdentificationStoreBase()
        {
            var mockIPersonIdentificationStore = new Mock<IPersonIdentificationStore>();
            mockIPersonIdentificationStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose PersonIdentificationStore data store.");
            mockIPersonIdentificationStore = mockIPersonIdentificationStore.As<IPersonIdentificationStore>();
            return mockIPersonIdentificationStore;
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
            _mockLogger = new Mock<ILogger<PersonIdentificationController>>();
            _mockLogger
                .As<ILogger<PersonIdentificationController>>()
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
