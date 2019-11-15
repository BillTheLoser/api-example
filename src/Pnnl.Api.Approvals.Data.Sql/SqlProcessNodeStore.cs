using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pnnl.Data.Paging;
using Pnnl.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Sql
{
    public class SqlProcessNodeStore : ProcessNodeStoreBase
    {
        private readonly IOptions<SqlProcessNodeStoreOptions> _options;
        private readonly ILogger<SqlProcessNodeStore> _logger;
        private readonly ISqlConnectionFactory _connectionFactory;

        public SqlProcessNodeStore(ILogger<SqlProcessNodeStore> logger, IOptions<SqlProcessNodeStoreOptions> options, ISqlConnectionFactory connectionFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override async Task<PagedResult<ProcessNode>> OnGetByIdsAsync(IList<int> processIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            int count = 0;

            string query = @"SELECT PROCESS_ID AS ProcessId
                                , NODE_NAME AS NodeName
                                , NODE_LABEL AS NodeLabel
                                , NODE_DATATYPE AS NodeDataType
                                , NODE_VALUE AS NodeValue
                             FROM XML_NODE
                             WHERE PROCESS_ID IN @Ids";

            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.AddDynamicParams(new { Ids = processIds });

                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                      IEnumerable<ProcessNode> processNodes;
                      IDictionary<int, ProcessNodeResult> processNodeResults = new Dictionary<int, ProcessNodeResult>();

                      using (SqlMapper.GridReader multi = await cn.QueryMultipleAsync(query, parameters, commandType: CommandType.Text))
                      {
                          processNodes = await multi.ReadAsync<ProcessNode>();
                      }

                      if (processNodes != null)
                      {
                          count = processNodes.Count();

                          // Group the nodes together by processId.
                          foreach (var processNode in processNodes)
                          {
                              int processId = processNode.ProcessId;

                              if(!processNodeResults.ContainsKey(processId))
                              {
                                  processNodeResults.Add(processId, new ProcessNodeResult()
                                  {
                                      ProcessId = processId,
                                      Nodes = new List<Node>()
                                  });
                              }

                              processNodeResults[processId].Nodes.Add(new Node()
                              {
                                   NodeName = processNode.NodeName,
                                   NodeLabel = processNode.NodeLabel,
                                   NodeValue = processNode.NodeValue,
                                   NodeDataType = processNode.NodeDataType
                              });
                          }
                      }

                      var result = processNodeResults
                                      .Values
                                      .ToList()
                                      .Skip(offset * limit)
                                      .Take(limit);

                      return new PagedResult<ProcessNodeResult>(offset, limit, count, result);
                  }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve process nodes list. Reason: {ex}");
                throw ex;
            }
        }
    }
}
