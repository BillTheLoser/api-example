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
using Pnnl.Api.Approvals.Data;
using Pnnl.Api.Approvals;

namespace Pnnl.Api.Approvals.Data.Tests
{
    [TestClass]
    public class ProcessFacadeTest
    {
        private Mock<ILogger<ProcessFacade>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IProcessStore> _mockProcessStore;
        //private Mock<IProcessStore> _mockProcessStore;
        private Mock<IPersonIdentificationStore> _mockPersonIdentificationStore;
        private Mock<IApprovalsLegacyStore> _mockApprovalsLegacyStore;
        private Mock<ISecurityStore> _mockSecurityStore;

        public ProcessFacadeTest()
        {
            _mockProcessStore = MockIProcessStoreBase();
            //_mockProcessStore = MockIProcessStoreBase();
            _mockApprovalsLegacyStore = MockIApprovalsLegacyStoreBase();
            _mockPersonIdentificationStore = MockIPersonIdentificationStoreBase();
            SetUpMockLogger();
            SetUpMockHttpContext();
            _mockSecurityStore = MockISecurityStoreBase();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsProcess_GivenValidList()
        {
            IList<int> validList = new List<int>() { 1 };
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process() { ProcessId = 1 });
            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            // arrange
            _mockProcessStore
                //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            //.ReturnsAsync( new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<Process>>()));

            var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
            var actual = await facade.GetAsync(validList, 0, 10); // act
            var objectResult = actual as ObjectResult;
            Assert.AreEqual(result, actual);

        }
        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_GivenInvalidList()
        {
            IList<int> invalidList = new List<int>() { 1 };
            PagedResult<Process> returnList = null;

            _mockProcessStore // arrange
                              //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(returnList);

            var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
            var actual = await facade.GetAsync(invalidList, 0, 10); // act
            var objectResult = actual as ObjectResult;
            Assert.IsNull(objectResult);

        }
        [TestMethod]
        public async Task GetByIdsAsync_ThrowsArgumentNull_GivenNullId()
        {
            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == null), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentNullException());
            // no need to configure the store, controller itself handles this case

            try
            {
                var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.GetAsync(null, 0, 10); // act
                Assert.Fail();
            }
            catch (Exception ) { } // this is the correct behavior for this test
        }

        //TerminateProcessAsync
        [TestMethod]
        public async Task TerminateProcessAsync_ThrowsArgumentNull_GivenNullPerson()
        {
            // The store needs to return an item that we can use against the logic
            IList<int> invalidList = new List<int>() { 1 };
            PagedResult<Process> returnList = null;

            _mockProcessStore // arrange
                              //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(returnList);

            IActionResult actual = null;   
            try
            {
                var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                actual = await facade.TerminateProcessAsync(It.IsAny<int>(), It.IsAny<bool>(), null, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>());
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
            } // this is the correct behavior for this test
        }

        [TestMethod]
        public async Task TerminateProcessAsync_ThrowsInvalidOperation_GivenApprovedProcess() 
        {
            int testId = 3;
            IList<int> testList = new List<int>() { testId,  };


            // The store needs to return an item that we can use against the logic
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process()
            {
                ProcessId = testId,
                ProcessState = "APPROVED",
                Activities = new Dictionary<int, Activity>() {
                    { 1,
                        new Activity()
                        {
                        ActivityId = 1,
                        ActivityState = "PENDING",
                        Actors = new Dictionary<int, Actor>()
                        {
                            {
                                1,
                                new Actor()
                                {
                                    ActorHanfordId = "Valid Id"
                                }
                            }
                        }

                        }
                    }
                }
            });
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() {"Valid Id" });

            _mockSecurityStore // arrange
                .Setup(store => store.IsSuperUserAsync(It.Is<int>(p => p == testId), It.Is<string>(u => u== "Valid Id"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(true);

            TerminateProcessResponse processResponse = new TerminateProcessResponse()
            {
                State = "Terminated",
                ProcessId = testId
            };

            _mockApprovalsLegacyStore // arrange
                .Setup(store => store.TerminateProcessAsync(It.Is<int>(s => s == testId), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(processResponse);

            Person person = new Person()
            {
                Id = "Valid Id",
                Network = new NetworkIdentifier()
                {
                    Username = "Valid Id",
                    Id = "Valid Id"
                }
            };

            IActionResult actual = null;
            try
            {
                var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                actual = await facade.TerminateProcessAsync(testId, It.IsAny<bool>(), person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>());
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
            } // this is the correct behavior for this test
        }

        [TestMethod]
        public async Task TerminateProcessAsync_ThrowsInvalidOperation_GivenTerminatedProcess()
       {
            int testId = 3;
            IList<int> testList = new List<int>() { testId, };


            // The store needs to return an item that we can use against the logic
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process()
            {
                ProcessId = testId,
                ProcessState = "TERMINATED",
                Activities = new Dictionary<int, Activity>() {
                    { 1,
                        new Activity()
                        {
                        ActivityId = 1,
                        ActivityState = "PENDING",
                        Actors = new Dictionary<int, Actor>()
                        {
                            {
                                1,
                                new Actor()
                                {
                                    ActorHanfordId = "Valid Id"
                                }
                            }
                        }

                        }
                    }
                }
            });
            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() { "Valid Id" });

            _mockSecurityStore // arrange
                .Setup(store => store.IsSuperUserAsync(It.Is<int>(p => p == testId), It.Is<string>(u => u == "Valid Id"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(true);

            TerminateProcessResponse processResponse = new TerminateProcessResponse()
            {
                State = "Terminated",
                ProcessId = testId
            };

            _mockApprovalsLegacyStore // arrange
                .Setup(store => store.TerminateProcessAsync(It.Is<int>(s => s == testId), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(processResponse);

            Person person = new Person()
            {
                Id = "Valid Id",
                Network = new NetworkIdentifier()
                {
                    Username = "Valid Id",
                    Id = "Valid Id"
                }
            };

            IActionResult actual = null;
            try
            {
                var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                actual = await facade.TerminateProcessAsync(testId, It.IsAny<bool>(), person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>());
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
            } // this is the correct behavior for this test
        }

        [TestMethod]
        public async Task TerminateProcessAsync_ThrowsInvalidOperation_GivenInvalidUserAccount()
        {
            int testId = 3;
            IList<int> testList = new List<int>() { testId, };


            // The store needs to return an item that we can use against the logic
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process()
            {
                ProcessId = testId,
                ProcessState = "PENDING",
                Activities = new Dictionary<int, Activity>() {
                    { 1,
                        new Activity()
                        {
                        ActivityId = 1,
                        ActivityState = "PENDING",
                        Actors = new Dictionary<int, Actor>()
                        {
                            {
                                1,
                                new Actor()
                                {
                                    ActorHanfordId = "Valid Id"
                                }
                            }
                        }

                        }
                    }
                }
            });
            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() { "Valid Id" });

            _mockSecurityStore // arrange
                .Setup(store => store.IsSuperUserAsync(It.Is<int>(p => p == testId), It.Is<string>(u => u == "Invalid Id"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(false);

            TerminateProcessResponse processResponse = new TerminateProcessResponse()
            {
                State = "Terminated",
                ProcessId = testId
            };

            _mockApprovalsLegacyStore // arrange
                .Setup(store => store.TerminateProcessAsync(It.Is<int>(s => s == testId), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(processResponse);

            Person person = new Person()
            {
                Id = "Invalid Id",
                Network = new NetworkIdentifier()
                {
                    Username = "Invalid Id",
                    Id = "Invalid Id"
                }
            };

            IActionResult actual = null;
            try
            {
                var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                actual = await facade.TerminateProcessAsync(testId, It.IsAny<bool>(), person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>());
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(UnauthorizedAccessException));
            } // this is the correct behavior for this test
        }
        
        [TestMethod]
        public async Task TerminateProcessAsync_ReturnsResponse_GivenSuperUser()
        {
            int testId = 4;
            IList<int> testList = new List<int>() { testId, };


            // The store needs to return an item that we can use against the logic
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process()
            {
                ProcessId = testId,
                ProcessState = "PENDING",
                Activities = new Dictionary<int, Activity>() {
                    { 1,
                        new Activity()
                        {
                        ActivityId = 1,
                        ActivityState = "PENDING",
                        Actors = new Dictionary<int, Actor>()
                        {
                            {
                                1,
                                new Actor()
                                {
                                    ActorHanfordId = "Valid Id"
                                }
                            }
                        }

                        }
                    }
                }
            });
            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() { "Valid Id" });

            _mockSecurityStore // arrange
                .Setup(store => store.IsSuperUserAsync(It.Is<int>(p => p == testId), It.Is<string>(u => u == "Valid Id"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(true);

            TerminateProcessResponse processResponse = new TerminateProcessResponse()
            {
                State = "Terminated",
                ProcessId = testId
            };

            _mockProcessStore // arrange
                .Setup(store => store.TerminateAsync(It.Is<int>(s => s == testId), It.Is<string>(u => u == "Valid Id"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new Process() { ProcessId = testId, ProcessState = ProcessState.Terminated.Name });

            _mockApprovalsLegacyStore // arrange
                .Setup(store => store.TerminateProcessAsync(It.Is<int>(s => s == testId), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(processResponse);

            Person person = new Person()
            {
                Id = "Valid Id",
                Network = new NetworkIdentifier()
                {
                    Username = "Valid Id",
                    Id = "Valid Id"
                }
            };

            try
            {
                var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.TerminateProcessAsync(testId, It.IsAny<bool>(), person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>());
                Assert.IsInstanceOfType(actual, typeof(TerminateProcessResponse));
            }
            catch (Exception )
            {
                Assert.Fail();
            } // this is the correct behavior for this test
        }
        
        [TestMethod]
        public async Task TerminateProcessAsync_ReturnsResponse_GivenChangeAccount()
        {
            int testId = 5;
            IList<int> testList = new List<int>() { testId, };


            // The store needs to return an item that we can use against the logic
            List<Process> returnList = new List<Process>();
            returnList.Add(new Process()
            {
                ProcessId = testId,
                ProcessDefinitionId = testId,
                ProcessState = "PENDING",
                Activities = new Dictionary<int, Activity>() {
                    { 1,
                        new Activity()
                        {
                        ActivityId = 1,
                        ActivityState = "PENDING",
                        Actors = new Dictionary<int, Actor>()
                        {
                            {
                                1,
                                new Actor()
                                {
                                    ActorHanfordId = "Valid Id"
                                }
                            }
                        }

                        }
                    }
                }
            });
            PagedResult<Process> result = new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() { "Auth Id" });

            _mockSecurityStore // arrange
                .Setup(store => store.IsSuperUserAsync(It.Is<int>(p => p == testId), It.Is<string>(u => u == "Auth Id"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(false);

            _mockSecurityStore // arrange
                .Setup(store => store.IsAuthorizedChangeAccountAsync(It.Is<int>(p => p == testId), It.Is<string>(u => u == "Auth Id"), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(true);

            TerminateProcessResponse processResponse = new TerminateProcessResponse()
            {
                State = "Terminated",
                ProcessId = testId
            };

            _mockProcessStore // arrange
                .Setup(store => store.TerminateAsync(It.Is<int>(s => s == testId), It.Is<string>(u => u == "Auth Id"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new Process() { ProcessId = testId, ProcessState = ProcessState.Terminated.Name });

            //_mockApprovalsLegacyStore // arrange
            //    .Setup(store => store.TerminateProcessAsync(It.Is<int>(s => s == testId), It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
            //    .ReturnsAsync(processResponse);

            Person person = new Person()
            {
                Id = "Auth Id",
                Network = new NetworkIdentifier()
                {
                    Username = "Auth Id",
                    Id = "Auth Id"
                }
            };

            //try
            //{
                var facade = InstanceProcessFacade(_mockProcessStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.TerminateProcessAsync(testId, It.IsAny<bool>(), person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>());
                Assert.IsInstanceOfType(actual, typeof(TerminateProcessResponse));
            //}
            //catch (Exception ex)
            //{
            //    Assert.Fail();
            //} // this is the correct behavior for this test
        }
        

        private dynamic InstanceProcessFacade(Mock<IProcessStore> mockProcessStore, Mock<IApprovalsLegacyStore> mockApprovalsLegacyStore, Mock<ISecurityStore> mockSecurityStore)
        {
            var facade = new ProcessFacade(_mockLogger.Object, mockProcessStore.Object, mockApprovalsLegacyStore.Object, mockSecurityStore.Object);
            return facade;
        }

        private Mock<IProcessStore> MockIProcessStoreBase()
        {
            var mockIProcessStore = new Mock<IProcessStore>();
            //mockIProcessStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ProcessStore data store.");
            mockIProcessStore = mockIProcessStore.As<IProcessStore>();
            return mockIProcessStore;
        }

        //private Mock<IProcessStore> MockIProcessStoreBase()
        //{
        //    var mockIProcessStore = new Mock<IProcessStore>();
        //    //mockIProcessStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ProcessStore data store.");
        //    mockIProcessStore = mockIProcessStore.As<IProcessStore>();
        //    return mockIProcessStore;
        //}

        private Mock<IPersonIdentificationStore> MockIPersonIdentificationStoreBase()
        {
            var mockIPersonIdentificationStore = new Mock<IPersonIdentificationStore>();
            //mockIPersonIdentificationStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose PersonIdentificationStore data store.");
            mockIPersonIdentificationStore = mockIPersonIdentificationStore.As<IPersonIdentificationStore>();
            return mockIPersonIdentificationStore;
        }

        private Mock<IApprovalsLegacyStore> MockIApprovalsLegacyStoreBase()
        {
            var mockIApprovalsLegacyStore = new Mock<IApprovalsLegacyStore>();
            //mockIApprovalsLegacyStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ApprovalsLegacyStore data store.");
            mockIApprovalsLegacyStore = mockIApprovalsLegacyStore.As<IApprovalsLegacyStore>();
            return mockIApprovalsLegacyStore;
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
            _mockLogger = new Mock<ILogger<ProcessFacade>>();
            _mockLogger
                .As<ILogger<ProcessController>>()
                .Setup(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }
        private Mock<ISecurityStore> MockISecurityStoreBase()
        {
            var mockISecurityStore = new Mock<ISecurityStore>();
            //mockISecurityStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ApprovalsLegacyStore data store.");
            mockISecurityStore = mockISecurityStore.As<ISecurityStore>();
            return mockISecurityStore;
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