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

namespace Pnnl.Api.Approvals.Data.Tests
{
    [TestClass]
    public class ActivityFacadeTest
    {
        private Mock<ILogger<ActivityFacade>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IActivityStore> _mockActivityStore;
        private Mock<IProcessStore> _mockProcessStore;
        private Mock<IPersonIdentificationStore> _mockPersonIdentificationStore;
        private Mock<IApprovalsLegacyStore> _mockApprovalsLegacyStore;
        private Mock<ISecurityStore> _mockSecurityStore;

        public ActivityFacadeTest()
        {
            _mockActivityStore = MockIActivityStoreBase();
            _mockProcessStore = MockIProcessStoreBase();
            _mockApprovalsLegacyStore = MockIApprovalsLegacyStoreBase();
            _mockPersonIdentificationStore = MockIPersonIdentificationStoreBase();
            SetUpMockLogger();
            SetUpMockHttpContext();
            _mockSecurityStore = MockISecurityStoreBase();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsActivity_GivenValidList()
        {
            IList<int> validList = new List<int>() { 1 };
            List<Activity> returnList = new List<Activity>();
            returnList.Add(new Activity() { ActivityId = 1 });
            PagedResult<Activity> result =  new PagedResult<Activity>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockActivityStore // arrange
                //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == validList), It.IsAny<int>(),It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            //.ReturnsAsync( new PagedResult<Activity>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),It.IsAny<IList<Activity>>()));

            var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
            var actual = await facade.GetAsync(validList, 0, 10); // act
            var objectResult = actual as ObjectResult;
            Assert.AreEqual(result, actual);

        }
        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_GivenInvalidList()
        {
            IList<int> invalidList = new List<int>() { 1 };
            PagedResult< Activity> returnList = null;

            _mockActivityStore // arrange
                //.Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == moqList),0,0, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == invalidList), It.IsAny<int>(),It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(returnList);

            var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
            var actual = await facade.GetAsync(invalidList, 0, 10); // act
            var objectResult = actual as ObjectResult;
            Assert.IsNull(objectResult);

        }
        [TestMethod]
        public async Task GetByIdsAsync_ThrowsArgumentNull_GivenNullId()
        {
            _mockActivityStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s == null), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .Throws(new ArgumentNullException());
            // no need to configure the store, controller itself handles this case

            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.GetAsync(null, 0, 10); // act
                Assert.Fail();
            }
            catch (Exception ) { } // this is the correct behavior for this test
        }

        [TestMethod]
        public async Task ApplyActorActionAsync_ThrowsInvalidOperationException_ProcessTerminated()
        {
            IList<int> invalidList = new List<int>() { -1 };

            List<Process> returnList = new List<Process>();
            returnList.Add(new Process() { ProcessId = 1, ProcessState = "TERMINATED" });
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == -1), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            // no need to configure the store, controller itself handles this case

            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.ApplyActorActionAsync(new ActorAction() { ProcessId = -1 }, It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()); // act
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // correct behavior
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task ApplyActorActionAsync_ThrowsInvalidOperationException_ProcessApproved()
        {
            IList<int> invalidList = new List<int>() { -2 };

            List<Process> returnList = new List<Process>();
            returnList.Add(new Process() { ProcessId = -2, ProcessState = "APPROVED" });
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == -2), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            // no need to configure the store, controller itself handles this case

            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.ApplyActorActionAsync(new ActorAction() { ProcessId = -2 }, It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()); // act
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // correct behavior
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task ApplyActorActionAsync_ThrowsInvalidOperationException_ActivityComplete()
        {
            IList<int> activityComplete = new List<int>() { -3 };

            List<Process> returnList = new List<Process>();
            returnList.Add(new Process()
            {
                ProcessId = -3,
                ProcessState = "PENDING",
                Activities = new Dictionary<int, Activity>() {
                    { 1,
                        new Activity(){
                        ActivityId = 1,
                        ActivityState = "COMPLETE"
                    }
                    }
                }
            });
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            ActorAction actorAction = new ActorAction() { ProcessId = -3, ActivityId = 1 };

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == -3), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            // no need to configure the store, controller itself handles this case

            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.ApplyActorActionAsync(actorAction, It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()); // act
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // correct behavior
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task ApplyActorActionAsync_ThrowsInvalidOperationException_ActivityNull()
        {
            int testId = -4;

            IList<int> activityComplete = new List<int>() { testId };

            List<Process> returnList = new List<Process>();
            returnList.Add(new Process()
            {
                ProcessId = testId,
                ProcessState = "PENDING",
                Activities = new Dictionary<int, Activity>() {
                    { 1,
                        new Activity(){
                        ActivityId = 1,
                        ActivityState = "NULL"
                    }
                    }
                }
            });
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            ActorAction actorAction = new ActorAction() { ProcessId = testId, ActivityId = 1 };

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() {"Valid Id" });

            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.ApplyActorActionAsync(actorAction, It.IsAny<Person>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()); // act
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // correct behavior
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task ApplyActorActionAsync_ThrowsInvalidOperationException_InvalidChangeAccount()
        {
            int testId = -5;

            IList<int> activityComplete = new List<int>() { testId };

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
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            ActorAction actorAction = new ActorAction() {
                ProcessId = testId,
                ActivityId = 1,
                ActorHanfordId = "Valid Id",
            };

            Person person = new Person()
            {
                Id = "Invalid",
                Network = new NetworkIdentifier()
                {
                    Username = "Invalid",
                    Id = "Invalid"
                }
            };

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() {"Valid Id" });

            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.ApplyActorActionAsync(actorAction, person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()); // act
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // correct behavior
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task ApplyActorActionAsync_ThrowsInvalidOperationException_InvalidActor()
        {
            int testId = -6;

            IList<int> activityComplete = new List<int>() { testId };

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
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            ActorAction actorAction = new ActorAction() {
                ProcessId = testId,
                ActivityId = 1,
                ActorHanfordId = "Invalid",
            };

            Person person = new Person()
            {
                Id = "Valid Id",
                Network = new NetworkIdentifier()
                {
                    Username = "Valid Id",
                    Id = "Valid Id"
                }
            };

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);

            _mockSecurityStore // arrange
                .Setup(store => store.GetAuthorizedAccountsAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(new List<string>() {"Valid Id" });

            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.ApplyActorActionAsync(actorAction, person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()); // act
                Assert.Fail();
            }
            catch (Exception exception)
            {
                Assert.IsInstanceOfType(exception, typeof(InvalidOperationException));
            }
        }

        [TestMethod]
        public async Task ApplyActorActionAsync_ReturnsProcess()
        {
            int testId = -6;

            IList<int> activityComplete = new List<int>() { testId };

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
            PagedResult<Process> result =  new PagedResult<Process>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), returnList);

            ActorAction actorAction = new ActorAction() {
                ProcessId = testId,
                ActivityId = 1,
                ActorHanfordId = "Valid Id",
            };

            Person person = new Person()
            {
                Id = "Valid Id",
                Network = new NetworkIdentifier()
                {
                    Username = "Valid Id",
                    Id = "Valid Id"
                }
            };

            _mockProcessStore // arrange
                .Setup(store => store.GetAsync(It.Is<IList<int>>(s => s[0] == testId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()))
                .ReturnsAsync(result);
            // no need to configure the store, controller itself handles this case
            try
            {
                var facade = InstanceActivityFacade(_mockActivityStore, _mockProcessStore, _mockPersonIdentificationStore, _mockApprovalsLegacyStore, _mockSecurityStore);
                var actual = await facade.ApplyActorActionAsync(actorAction, person, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<object, object>>()); // act
                var objectResult = actual as ObjectResult;
                Assert.AreEqual(returnList[0], actual);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private dynamic InstanceActivityFacade(Mock<IActivityStore> mockActivityStore, Mock<IProcessStore> mockProcessStore, Mock<IPersonIdentificationStore> mockPersonIdentificationStore, Mock<IApprovalsLegacyStore> mockApprovalsLegacyStore, Mock<ISecurityStore> mockSecurityStore)
        {
            var facade = new ActivityFacade( _mockLogger.Object, mockActivityStore.Object, mockProcessStore.Object, mockApprovalsLegacyStore.Object, mockPersonIdentificationStore.Object, mockSecurityStore.Object);
            return facade;
        }

        private Mock<IActivityStore> MockIActivityStoreBase()
        {
            var mockIActivityStore = new Mock<IActivityStore>();
            //mockIActivityStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ActivityStore data store.");
            mockIActivityStore = mockIActivityStore.As<IActivityStore>();
            return mockIActivityStore;
        }

        private Mock<IProcessStore> MockIProcessStoreBase()
        {
            var mockIProcessStore = new Mock<IProcessStore>();
            //mockIProcessStore.As<IDisposable>().Setup(store => store.Dispose()).Verifiable("Controller did not properly dispose ProcessStore data store.");
            mockIProcessStore = mockIProcessStore.As<IProcessStore>();
            return mockIProcessStore;
        }

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
            _mockLogger = new Mock<ILogger<ActivityFacade>>();
            _mockLogger
                .As<ILogger<ActivityController>>()
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
