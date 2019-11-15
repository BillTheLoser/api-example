using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using approvals.routing;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using System.Security.Cryptography;
using ops = Pnnl.Api.Operations;
using Pnnl.Api.Approvals.Data.Interfaces;
using System.Text.Json;

namespace Pnnl.Api.Approvals.Data.Http
{
    /// <summary>
    /// The Http data store to access the <see cref="ApprovalsLegacyStoreBase"/> information
    /// </summary>
    public class HttpApprovalsLegacyStore : ApprovalsLegacyStoreBase
    {
        /// <summary>
        /// Gets the logger used to write diagnostic information.
        /// </summary>
        /// <value>The <see cref="ILogger"/> used to write diagnostic information.</value>
        protected ILogger<HttpApprovalsLegacyStore> _logger { get; }

        /// <summary>
        /// Gets the options used to configure this service.
        /// </summary>
        /// <value>The <see cref="IOptions{TOptions}"/> used to configure this service.</value>
        protected IOptions<HttpRouteItemStoreOptions> _options { get; }

        /// <summary>
        /// Gets the http client used to make service requests.
        /// </summary>
        private HttpClient _client { get; set; }

        private IPersonIdentificationStore _personIdentificationStore { get; }

        /// <summary>
        /// Constructor with basic parameters for dependency injection.
        /// </summary>
        /// <param name="logger">The logger for capturing logs.</param>
        /// <param name="options">Options for the store</param>
        /// <param name="clients">The client factory used to generate connections for the applciation.</param>
        /// <param name="personIdentificationStore">The data store we can use to map ids.</param>
        public HttpApprovalsLegacyStore(IPersonIdentificationStore personIdentificationStore, ILogger<HttpApprovalsLegacyStore> logger, IHttpClientFactory clients, IOptions<HttpRouteItemStoreOptions> options)
        {
            _personIdentificationStore = personIdentificationStore ?? throw new ArgumentNullException(nameof(personIdentificationStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _client = clients.CreateClient(_options.Value.Client);
        }

        /// <summary>
        /// Asynchronously routes an item for approvals
        /// </summary>
        /// <param name="routingItem">The information necessary to instantiate a new routing.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        protected override async Task<int?> OnCreateRoutingAsync(RoutingItem routingItem, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            // TODO: Fix this, errors in here need to be aggregated and passed back, right now it's a bit obtuse, but we need to validate the hanford ids
            PersonIdentification submitterIds = await _personIdentificationStore.GetByHanfordIdAsync(routingItem.SubmitUserHanfordId, cancellationToken, context);
            PersonIdentification originatorIds = await _personIdentificationStore.GetByHanfordIdAsync(routingItem.OriginatorHanfordId, cancellationToken, context);
            PersonIdentification beneficiaryIds = await _personIdentificationStore.GetByHanfordIdAsync(routingItem.BeneficiaryHanfordId, cancellationToken, context);

            try
            {
                Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                ChannelFactory<RoutingSoap> factory = new ChannelFactory<RoutingSoap>(binding, new EndpointAddress(_client.BaseAddress));
                RoutingSoap serviceProxy = factory.CreateChannel();

                InstantiateProcessRequest process = CreateInstProcessRequest(routingItem, submitterIds, originatorIds, beneficiaryIds);

                var result = await serviceProxy.InstantiateProcessAsync(process);


                if (result.InstantiateProcessResult.InstProcessId == 0)
                {
                    throw new ArgumentException(result.InstantiateProcessResult.Status);
                }

                int? approvalResponse = result.InstantiateProcessResult.InstProcessId;
                return approvalResponse;
            }
            catch (ArgumentException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                _logger.LogError("A web service error occurred while submitting the item for routing. Reason: {@Exception}", exception);
                _logger.LogDebug(JsonSerializer.Serialize(routingItem));
                throw;
            }
        }

        /// <summary>
        /// Asynchronously applies an actor action to an activity
        /// </summary>
        /// <param name="actorAction">The information necessary to instantiate a new routing.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        protected override async Task<int?> OnApplyActorActionAsync(ActorAction actorAction, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            // TODO: Fix this, errors in here need to be aggregated and passed back, right now it's a bit obtuse, but we need to validate the hanford ids
            PersonIdentification actingUser = await _personIdentificationStore.GetByHanfordIdAsync(actorAction.ActorHanfordId, cancellationToken, context);
            //PersonIdentification originatorIds = await _personIdentificationStore.GetByHanfordIdAsync(routingItem.SubmitUserHanfordId, cancellationToken, context);
            //PersonIdentification beneficiaryIds = await _personIdentificationStore.GetByHanfordIdAsync(routingItem.SubmitUserHanfordId, cancellationToken, context);

            try
            {
                ChannelFactory<RoutingSoap> factory = null;
                RoutingSoap serviceProxy = null;
                Binding binding = null;

                binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                factory = new ChannelFactory<RoutingSoap>(binding, new EndpointAddress(_client.BaseAddress));
                serviceProxy = factory.CreateChannel();

                sendActionRequest sendActionRequest = CreateSendActionRequest(actorAction, actingUser);
                var result = await serviceProxy.sendActionAsync(sendActionRequest);

                if (result.sendActionResult.StatusCode != SendActionStatus.Success)
                {
                    throw new ArgumentException(result.sendActionResult.StatusDescription);
                }

                int? approvalResponse = actorAction.ActivityId;
                return approvalResponse;
            }
            catch (ArgumentException exception)
            {
                _logger.LogError("A web service error occurred while submitting release  for review. Reason: {@Exception}", exception);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError("A web service error occurred while submitting release  for review. Reason: {@Exception}", exception);
                throw;
            }
        }

        private sendActionRequest CreateSendActionRequest(ActorAction actorAction, PersonIdentification actionPerson)
        {
            //	<action>
            //	<processId>string</processId>
            //	<activityId>string</activityId>
            //	<comments>string</comments>
            //	<actionTaken>string</actionTaken>
            //	<websignRedirect>string</websignRedirect>
            //	<documentDescription>string</documentDescription>
            //	<actionUser>
            //		<domain>string</domain>
            //		<networkId>string</networkId>
            //	</actionUser>
            //	<document>
            //		<fileExtension>string</fileExtension>
            //		<mimeType>string</mimeType>
            //		<content>base64Binary</content>
            //		<asciiContent>string</asciiContent>
            //		<xslStylesheet>string</xslStylesheet>
            //	</document>
            //</action>
            //

            byte[] bPlain;
            byte[] bSigned;

            XElement action = new XElement("action");

            XElement process = new XElement("processId")
            {
                Value = actorAction.ProcessId.ToString()
            };
            action.Add(process);

            XElement activity = new XElement("activityId");
            activity.Value = actorAction.ActivityId.ToString();
            action.Add(activity);

            XElement comment = new XElement("comments");
            comment.Value = actorAction.Comment;
            action.Add(comment);

            XElement actionTaken = new XElement("actionTaken")
            {
                Value = actorAction.ActionTaken.ToString()
            };
            action.Add(actionTaken);

            XElement websignRedirect = new XElement("websignRedirect");
            action.Add(websignRedirect);

            XElement actionUser = new XElement("actionUser");

            XElement domain = new XElement("domain")
            {
                Value = actionPerson.Domain
            };
            actionUser.Add(domain);

            XElement netId = new XElement("networkId")
            {
                Value = actionPerson.NetworkId
            };
            actionUser.Add(netId);
            action.Add(actionUser);

            XElement document = new XElement("document");
            action.Add(document);

            try
            {

                bPlain = System.Text.Encoding.ASCII.GetBytes(action.ToString());

                RSACryptoServiceProvider RSA;
                RSACryptoServiceProvider.UseMachineKeyStore = true;
                CspParameters csp = new CspParameters
                {
                    Flags = CspProviderFlags.UseExistingKey | CspProviderFlags.UseMachineKeyStore,
                    KeyContainerName = "approvalskey"
                };

                using (RSA = new RSACryptoServiceProvider(csp))
                {
                    bSigned = RSA.SignData(bPlain, "MD5");
                }
            }
            catch (Exception ex)
            {
                //Internal.Cryptography.CryptoThrowHelper + WindowsCryptographicException
                string exs = ex.GetType().ToString();

                if (exs == "Internal.Cryptography.CryptoThrowHelper+WindowsCryptographicException")
                    throw new InvalidOperationException("Crytography keyset does not exist!", ex);
                else
                    throw;
            }

            sendActionRequest sendActionRequest = new sendActionRequest()
            {
                processId = actorAction.ProcessId,
                actionPayload = action.ToString(),
                SignedString = bSigned
            };

            return sendActionRequest;
        }

        private static InstantiateProcessRequest CreateInstProcessRequest(RoutingItem routingItem, PersonIdentification submitterIds, PersonIdentification originatorIds, PersonIdentification beneficiaryIds)
        {
            NetworkIdentification1 instantiateUser = new NetworkIdentification1
            {
                Domain = submitterIds.Domain,
                NetworkId = submitterIds.NetworkId
            };

            InstantiateProcessRequest process = new InstantiateProcessRequest
            {
                InstantiateUser = instantiateUser
            };

            RoutingPayload pay = new RoutingPayload
            {
                MetaData = new DocMetaData()
            };

            pay.MetaData.ApplicationIRI = (int)routingItem.ApplicationItemId;
            pay.MetaData.DocumentTypeName = routingItem.DocumentTypeName;
            pay.MetaData.DocumentAtAGlance = routingItem.DocumentTitle;

            pay.MetaData.DocumentId = routingItem.DocumentId;

            pay.MetaData.DocumentOriginator = new NetworkIdentification
            {
                Domain = originatorIds.Domain,
                NetworkId = originatorIds.NetworkId
            };

            pay.MetaData.DocumentBeneficiary = new NetworkIdentification
            {
                Domain = beneficiaryIds.Domain,
                NetworkId = beneficiaryIds.NetworkId
            };

            // Example
            //<MetaData>
            //<ApprovalTechEmplId/>
            //<ApprovalUserEmplId/>
            //<AuthorEmplId/>
            //<ChangeSw/>
            //<OtherApproverList>
            //<OtherApproverList_items Type="USERIDLIST -or- ROLENAME -or- LISTVALUES">
            //<ListItem Key="" Value=""/>
            //</OtherApproverList_items>
            //</OtherApproverList>
            //<QualityEngineerEmplId/>
            //<RadiologicalControlEmplId/>
            //<RespManagerEmplId/>
            //<SafetyAndHealth/>
            //<SMEApproverList>
            //<SMEApproverList_items Type="USERIDLIST -or- ROLENAME -or- LISTVALUES">
            //<ListItem Key="" Value=""/>
            //</SMEApproverList_items>
            //</SMEApproverList>
            //<USQDSESNo/>
            //<USQTNumber/>
            //</MetaData>

            XElement metaData = new XElement("MetaData");

            if(routingItem.IntFields != null && routingItem.IntFields.Count > 0)
            {
                foreach (var item in routingItem.IntFields)
                {
                    XElement element = new XElement(item.Key)
                    {
                        Value = item.Value.ToString()
                    };
                    metaData.Add(element);
                }
            }

            if (routingItem.StringFields != null && routingItem.StringFields.Count > 0)
            {
                foreach (var item in routingItem.StringFields)
                {
                    XElement element = new XElement(item.Key)
                    {
                        Value = item.Value
                    };
                    metaData.Add(element);
                }
            }

            if (routingItem.ListFields != null && routingItem.ListFields.Count > 0)
            {
                foreach (var item in routingItem.ListFields)
                {
                    XElement listElement = new XElement(item.Key);
                    var list = item.Value;
                    XAttribute attribute = new XAttribute("Type", list.ListType);
                    listElement.Add(attribute);

                    XElement items = new XElement("LIST_ITEMS");
                    listElement.Add(item);
                    foreach (var value in list.Values)
                    {
                        XElement element = new XElement("LIST_ITEM")
                        {
                            Value = value
                        };
                        metaData.Add(element);
                    }
                }
            }

            pay.MetaData.DocumentRoutingData = metaData;
            process.RoutingPayload = pay;

            pay.Document = new Document
            {
                FileExtension = routingItem.Document.FileExtension,
                MimeType = routingItem.Document.MimeType,
                AsciiContent = routingItem.Document.AsciiContent,
                XslStylesheet = routingItem.Document.XslStyleSheet,
                Content = routingItem.Document.Content
            };

            return process;
        }

        protected override async Task<TerminateProcessResponse> OnTerminateProcessAsync(int processId, ops.Person terminatingUser, CancellationToken cancellationToken, IDictionary<object,object> context)
        {
            try
            {
                NetworkIdentification1 terminateUser = new NetworkIdentification1
                {
                    Domain = terminatingUser.Network.Domain,
                    NetworkId = terminatingUser.Network.Username
                };

                Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                {
                    SendTimeout = new TimeSpan(0, 5, 0),
                    ReceiveTimeout = new TimeSpan(0, 5, 0)
                };

                ChannelFactory<RoutingSoap> factory = new ChannelFactory<RoutingSoap>(binding, new EndpointAddress(_client.BaseAddress));
                RoutingSoap serviceProxy = factory.CreateChannel();

                TerminationResponse result = await serviceProxy.TerminateByProcessAsync(processId, terminateUser);
                factory.Close();

                TerminateProcessResponse terminateProcessResponse = new TerminateProcessResponse()
                {
                    ProcessId = processId,
                    ResultId = result.ResultId,
                    Status = result.Status.ToUpper()
                };

                return terminateProcessResponse;
            }
            catch (Exception exception)
            {
                _logger.LogError($"A web service error occurred while terminating the process with id: {processId}. Reason: {exception}");
                throw;
            }
        }

        protected override async Task<TerminateProcessResponse> OnTerminateByProcessNoStatusingAsync(int processId, ops.Person terminatingUser, CancellationToken cancellationToken, IDictionary<object, object> context)
        {
            try
            {
                NetworkIdentification1 terminateUser = new NetworkIdentification1
                {
                    Domain = terminatingUser.Network.Domain,
                    NetworkId = terminatingUser.Network.Username
                };

                Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                {
                    SendTimeout = new TimeSpan(0, 5, 0),
                    ReceiveTimeout = new TimeSpan(0, 5, 0)
                };

                ChannelFactory<RoutingSoap> factory = new ChannelFactory<RoutingSoap>(binding, new EndpointAddress(_client.BaseAddress));
                RoutingSoap serviceProxy = factory.CreateChannel();

                TerminationResponse result = await serviceProxy.TerminateByProcessNoStatusingAsync(processId, terminateUser);
                factory.Close();

                TerminateProcessResponse terminateProcessResponse = new TerminateProcessResponse()
                {
                    ProcessId = processId,
                    ResultId = result.ResultId,
                    Status = result.Status.ToUpper()
                };

                return terminateProcessResponse;
            }
            catch (Exception exception)
            {
                _logger.LogError($"A web service error occurred while terminating the process with id: {processId}. Reason: {exception}");
                throw;
            }
        }
    }
}
