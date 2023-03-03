using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LesegaisParcer
{
    public class SearchReportWoodDealQuery
    {
        [JsonPropertyName("query")]
        public string Query { get; } = 
            @"query SearchReportWoodDeal($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {
                searchReportWoodDeal(filter: $filter, pageable: {number: $number, size: $size}, orders: $orders) {
                content {
                  sellerName
                  sellerInn
                  buyerName
                  buyerInn
                  woodVolumeBuyer
                  woodVolumeSeller
                  dealDate
                  dealNumber
                  __typename
                }
                __typename
              }
            }";

        [JsonPropertyName("variables")]
        public SearchReportWoodDealQueryVariables Variables { get; } = new SearchReportWoodDealQueryVariables();

        public void SetVariables(int size, int number, string filter = null, string orders = null)
        {
            Variables.Size = size;
            Variables.Number = number;
            Variables.Filter = filter;
            Variables.Orders = orders;
        }
    }

    public class SearchReportWoodDealQueryVariables
    {
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("number")]
        public int Number { get; set; }
        [JsonPropertyName("filter")]
        public string Filter { get; set; } = null;
        [JsonPropertyName("orders")]
        public string Orders { get; set; } = null;
    }

    public class SearchReportWoodDealQueryResponse
    {
        [JsonPropertyName("data")]
        public SearchReportWoodDealQueryResponseData Data { get; set; }
    }

    public class SearchReportWoodDealQueryResponseData
    {
        [JsonPropertyName("searchReportWoodDeal")]
        public SearchReportWoodDealQueryResponseContent SearchReportWoodDeal { get; set; }
    }

    public class SearchReportWoodDealQueryResponseContent
    {
        [JsonPropertyName("content")]
        public IEnumerable<Deal> Content { get; set; }
    }
}
