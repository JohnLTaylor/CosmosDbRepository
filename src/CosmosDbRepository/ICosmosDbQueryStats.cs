using Microsoft.Azure.Documents;
using System.Collections.Generic;

namespace CosmosDbRepository
{
    public interface ICosmosDbQueryStats
    {
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
    }
}
