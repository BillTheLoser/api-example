using GraphQL.Types;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Approvals.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pnnl.Api.Approvals.Queries
{
    public class ApprovalsQuery : ObjectGraphType<object>
    {
        public ApprovalsQuery(IProcessFacade processFacade, IProcessNodeStore processNodeStore)
        {
            #region Processes Example Query
            /*
                -------------Example Query----------------
                query GetProcesses($actorIdList: [String], $originatorIdList: [String],
  				  			    $beneficiaryIdList: [String],	$activityStateList: [String], 
  								$processStateList: [String], $createDateStart: DateTime,
  								$createDateEnd: DateTime, $createDays: Int,
  								$lastDateStart: DateTime, $lastDateEnd: DateTime,
  								$lastDays: Int, $lastChangeDateRange: DateTime,
  								$documentTypeList: [String], $offset: Int, $limit:Int){
                      processes(actorIdList: $actorIdList, originatorIdList: $originatorIdList,
    				                    beneficiaryIdList: $beneficiaryIdList, activityStateList: $activityStateList, 
    				                    processStateList: $processStateList, createDateStart: $createDateStart,
    				                    createDateEnd: $createDateEnd, createDays: $createDays,
    				                    lastDateStart: $lastDateStart, lastDateEnd: $lastDateEnd,
    				                    lastDays: $lastDays, lastChangeDateRange: $lastChangeDateRange, 
    				                    documentTypeList: $documentTypeList, offset: $offset, limit: $limit){
                        processId
                        processState
                        processStatus
                        documentId
                        documentTypeName
                        lastChangeHanfordId {
                          employeeId
                          lastName
                          firstName
                        }
                        lastChangeDateTime
                        createDateTime
                        beneficiaryHanfordId {
                          employeeId
                          lastName
                          firstName
                        }        
                        processDefinitionId
                      }  
                    }

                ----------Query Variables----------
                {
                  "beneficiaryIdList":  ["2002702"],
                  "processStateList": ["PENDING"],
                  "offset" : 0,
                  "limit": 10
                }
            */
            #endregion

            FieldAsync<ListGraphType<ProcessGraph>>(
                 name: "processes",
                 description: "Retrieves all the filtered process",                 
                 arguments: new QueryArguments
                 {
                    new QueryArgument<ListGraphType<StringGraphType>>{ Name="actorIdList"},
                    new QueryArgument<ListGraphType<StringGraphType>>{ Name = "originatorIdList" },
                    new QueryArgument<ListGraphType<StringGraphType>>{ Name = "beneficiaryIdList" },
                    new QueryArgument<ListGraphType<StringGraphType>>{ Name = "activityStateList" },
                    new QueryArgument<ListGraphType<StringGraphType>>{ Name = "processStateList" },
                    new QueryArgument<DateTimeGraphType>{ Name="createDateStart" },
                    new QueryArgument<DateTimeGraphType>{ Name="createDateEnd" },
                    new QueryArgument<IntGraphType>{ Name="createDays" },
                    new QueryArgument<DateTimeGraphType>{ Name="lastDateStart" },
                    new QueryArgument<DateTimeGraphType>{ Name="lastDateEnd" },
                    new QueryArgument<IntGraphType>{ Name="lastDays" },
                    new QueryArgument<DateTimeGraphType>{ Name="lastChangeDateRange" },
                    new QueryArgument<ListGraphType<StringGraphType>>{ Name="documentTypeList" },

                    new QueryArgument<IntGraphType>{ Name="offset" },
                    new QueryArgument<IntGraphType>{ Name="limit" }
                 },
                 resolve: async (context) =>
                 {
                     var actorIdList = context.GetArgument<List<string>>("actorIdList");
                     var originatorIdList = context.GetArgument<List<string>>("originatorIdList");
                     var beneficiaryIdList = context.GetArgument<List<string>>("beneficiaryIdList");
                     var activityStateList = context.GetArgument<List<string>>("activityStateList");
                     var processStateList = context.GetArgument<List<string>>("processStateList");
                     var createDateStart = context.GetArgument<DateTime?>("createDateStart");
                     var createDateEnd = context.GetArgument<DateTime?>("createDateEnd");
                     var createDays = context.GetArgument<int?>("createDays");
                     var lastDateStart = context.GetArgument<DateTime?>("lastDateStart");
                     var lastDateEnd = context.GetArgument<DateTime?>("lastDateEnd");
                     var lastDays = context.GetArgument<int?>("lastDays");

                     var person = context.GetCurrentUser();
                     var offset = context.GetArgument<int?>("offset");
                     var limit = context.GetArgument<int?>("limit");
                     
                     var processFilter = processFacade.GenerateProcessFilter(offset, limit, actorIdList, originatorIdList, beneficiaryIdList, null, activityStateList, createDateStart, createDateEnd, createDays, lastDateStart, lastDateEnd, lastDays, null, processStateList);

                     return await processFacade.SearchAsync(processFilter, person, offset, limit, context.CancellationToken);
                 });

            #region Process Example Query
            /*
            query processById {
              processById(processId: 1480670) {
                processId
                processState
                processStatus
                beneficiaryHanfordId {
                  employeeId
                  lastName
                  firstName
                }
                lastChangeDateTime
                createDateTime
                documentId
                documentTypeName
                processDefinitionId
                activities {
                  actedActorId
                  actedHanfordId {
                    id
                  }
                  activityId
                  activityName
                  activityState
                  activityStatus
                  comment
                  lastChangeDateTime
                }
              }
            }
            */

            #endregion

            FieldAsync<ProcessGraph>(
                name: "processById",
                description: "Retrieves the process with the given process Id.",
                arguments: new QueryArguments
                {
                    new QueryArgument<IntGraphType>{ Name = "processId" },
                },
                resolve: async (context) =>
                {
                    var processId = context.GetArgument<int>("processId");

                    return (await processFacade.GetAsync(new List<int>() { processId }, null, null, context.CancellationToken)).FirstOrDefault();
                });


            FieldAsync<ListGraphType<ProcessGraph>>(
                name: "processesByIds",
                description: "Retrieves the processes with the given process Ids.",
                arguments: new QueryArguments
                {
                    new QueryArgument<ListGraphType<IntGraphType>>{ Name = "processIds" },
                    new QueryArgument<IntGraphType> { Name = "offset" },
                    new QueryArgument<IntGraphType> { Name = "limit" }
                },
                resolve: async (context) =>
                {
                    var processIds = context.GetArgument<List<int>>("processIds");
                    var offset = context.GetArgument<int?>("offset");
                    var limit = context.GetArgument<int?>("limit");

                    return await processFacade.GetAsync(processIds, offset, limit, context.CancellationToken);
                });

            #region Processes' Nodes Example Query
            /*
                -------------Example Query----------------
                query GetProcessNodes($processIds: [Int]!, $offset: Int, $limit: Int) {
                  processesNodes(processIds: $processIds, offset: $offset, limit: $limit){
                    nodes{
                      processId
                      nodeName
                      nodeLabel
                      nodeDataType
                      nodeValue
                    }
                  }
                }

                ----------Query Variables----------
                {
                  "processIds": [71953,71951,71950,71949,71947],
                  "offset": 4,
                  "limit": 3
                }
            */
            #endregion

            FieldAsync<PagedProcessNodeResultGraph>(
                name: "processesNodes",
                description: "Retrieves the process's nodes for each of the process in the given list of process Ids.",
                arguments: new QueryArguments
                {
                    new QueryArgument<ListGraphType<IntGraphType>>{ Name = "processIds" },
                    new QueryArgument<IntGraphType> { Name = "offset" },
                    new QueryArgument<IntGraphType> { Name = "limit" }
                },
                resolve: async (context) =>
                {
                    var processIds = context.GetArgument<List<int>>("processIds");
                    var offset = context.GetArgument<int?>("offset");
                    var limit = context.GetArgument<int?>("limit");

                    var result = await processNodeStore.GetByIdsAsync(processIds, offset, limit, context.CancellationToken);

                    return new Connection<ProcessNodeResult>
                    {
                        PageInfo = new PageInfo
                        {
                            Total = (int)result.Total,
                            Offset = result.Offset,
                            Limit = result.Limit
                        },
                        Nodes = result.Results
                    };
                });
        }
    }
}
