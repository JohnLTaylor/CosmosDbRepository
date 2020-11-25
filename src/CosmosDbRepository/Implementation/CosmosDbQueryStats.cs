using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CosmosDbRepository.Implementation
{
    internal class CosmosDbQueryStats<T>
        : CosmosDbQueryStats
    {
        public CosmosDbQueryStats(FeedResponse<T> feed, string operation)
            : base(feed.IsRUPerMinuteUsed,
                   feed.RequestCharge,
                   feed.SessionToken,
                   feed.ActivityId,
                   feed.RequestDiagnosticsString,
                   feed.QueryMetrics,
                   operation)
        {
        }

        public CosmosDbQueryStats(StoredProcedureResponse<T> feed, string operation)
            : base(feed.IsRUPerMinuteUsed,
                   feed.RequestCharge,
                   feed.SessionToken,
                   feed.ActivityId,
                   feed.ScriptLog,
                   default,
                   operation)
        {
        }
    }

    internal class CosmosDbQueryStats
        : ICosmosDbQueryStats
    {
        public CosmosDbQueryStats(ResourceResponseBase feed, string operation)
            : this(feed.IsRUPerMinuteUsed,
                   feed.RequestCharge,
                   feed.SessionToken,
                   feed.ActivityId,
                   feed.RequestDiagnosticsString,
                   default,
                   operation)
        {
        }

        public CosmosDbQueryStats(IList<CosmosDbQueryStats> stats)
        {
            IsRUPerMinuteUsed = stats.Any(feed => feed.IsRUPerMinuteUsed);
            RequestCharge = stats.Sum(feed => feed.RequestCharge);
            SessionToken = stats[0].SessionToken;
            ActivityId = string.Join(",", stats.Select(feed => feed.ActivityId));
            RequestDiagnosticsString = string.Join(",", stats.Select(feed => feed.RequestDiagnosticsString));
            QueryMetrics = stats.Where(feed => feed.QueryMetrics != default).SelectMany(feed => feed.QueryMetrics).GroupBy(qm => qm.Key, qm => qm.Value).ToDictionary(item => item.Key, item => item.Aggregate((curr, acc) => curr + acc));
            Operation = stats[0].Operation;
        }

        protected CosmosDbQueryStats(bool isRUPerMinuteUsed,
                   double requestCharge,
                   string sessionToken,
                   string activityId,
                   string requestDiagnosticsString,
                   IReadOnlyDictionary<string, QueryMetrics> queryMetrics,
                   string operation)
        {
            IsRUPerMinuteUsed = isRUPerMinuteUsed;
            RequestCharge = requestCharge;
            SessionToken = sessionToken;
            ActivityId = activityId;
            RequestDiagnosticsString = requestDiagnosticsString;
            QueryMetrics = queryMetrics;
            Operation = operation;
        }

        //
        // Summary:
        //     Gets the name of the operation associated with the request to the Azure Cosmos DB service.
        //
        // Value:
        //     Name of the operation
        public string Operation { get; }

        //
        // Summary:
        //     Gets the flag associated with the response from the Azure Cosmos DB service whether
        //     this request is served from Request Units(RUs)/minute capacity or not.
        //
        // Value:
        //     True if this request is served from RUs/minute capacity. Otherwise, false.
        public bool IsRUPerMinuteUsed { get; }

        //
        // Summary:
        //     Gets the request charge for this request from the Azure Cosmos DB service.
        //
        // Value:
        //     The request charge measured in request units.
        public double RequestCharge { get; }

        //
        // Summary:
        //     Gets the session token for use in session consistency reads from the Azure Cosmos
        //     DB service.
        //
        // Value:
        //     The session token for use in session consistency.

        public string SessionToken { get; }
        //
        // Summary:
        //     Gets the activity ID for the request from the Azure Cosmos DB service.
        //
        // Value:
        //     The activity ID for the request.

        public string ActivityId { get; }

        //
        // Summary:
        //     Gets the diagnostics information for the current request to Azure Cosmos DB service.
        //
        // Remarks:
        //     This field is only valid when the request uses direct connectivity.
        public string RequestDiagnosticsString { get; }

        //
        // Summary:
        //     Get Microsoft.Azure.Documents.QueryMetrics for each individual partition in the
        //     Azure Cosmos DB service
        public IReadOnlyDictionary<string, QueryMetrics> QueryMetrics { get; }

        public override string ToString()
        {
            return
$@"Operation: {Operation}
IsRUPerMinuteUsed: {IsRUPerMinuteUsed}
RequestCharge: {RequestCharge}
SessionToken: {SessionToken}
ActivityId: {ActivityId}
RequestDiagnosticsString: {RequestDiagnosticsString}
QueryMetrics:
{QueryMetricsToString()}";
        }

        public string QueryMetricsToString()
        {
            if (QueryMetrics == default)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            foreach (var kv in QueryMetrics)
            {
                result.AppendLine($"  {kv.Key}");
                result.AppendLine(
$@"    TotalTime: {kv.Value.TotalTime}
    RetrievedDocumentCount: {kv.Value.RetrievedDocumentCount}
    RetrievedDocumentSize: {kv.Value.RetrievedDocumentSize}
    OutputDocumentCount: {kv.Value.OutputDocumentCount}
    QueryPreparationTimes:
      CompileTime: {kv.Value.QueryPreparationTimes.CompileTime}
      LogicalPlanBuildTime: {kv.Value.QueryPreparationTimes.LogicalPlanBuildTime}
      PhysicalPlanBuildTime: {kv.Value.QueryPreparationTimes.PhysicalPlanBuildTime}
      QueryOptimizationTime: {kv.Value.QueryPreparationTimes.QueryOptimizationTime}
    QueryEngineTimes:
      IndexLookupTime: {kv.Value.QueryEngineTimes.IndexLookupTime}
      DocumentLoadTime: {kv.Value.QueryEngineTimes.DocumentLoadTime}
      WriteOutputTime: {kv.Value.QueryEngineTimes.WriteOutputTime}
      RuntimeExecutionTimes:
        SystemFunctionExecutionTime: {kv.Value.QueryEngineTimes.RuntimeExecutionTimes.SystemFunctionExecutionTime}
        UserDefinedFunctionExecutionTime: {kv.Value.QueryEngineTimes.RuntimeExecutionTimes.UserDefinedFunctionExecutionTime}
        TotalTime: {kv.Value.QueryEngineTimes.RuntimeExecutionTimes.TotalTime}
    Retries: {kv.Value.Retries}
    IndexHitRatio: {kv.Value.IndexHitRatio}");
            }

            return result.ToString();
        }
    }
}