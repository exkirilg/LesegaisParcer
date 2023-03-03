using System;
using System.Text.Json.Serialization;

namespace LesegaisParcer
{
    public class Deal
    {
        [JsonPropertyName("dealNumber")]
        public string DealNumber { get; set; }

        [JsonPropertyName("dealDate")]
        public DateTime? DealDate { get; set; }

        [JsonPropertyName("sellerName")]
        public string SellerName { get; set; }

        [JsonPropertyName("sellerInn")]
        public string SellerInn { get; set; }

        [JsonPropertyName("buyerName")]
        public string BuyerName { get; set; }

        [JsonPropertyName("buyerInn")]
        public string BuyerInn { get; set; }

        [JsonPropertyName("woodVolumeBuyer")]
        public float WoodVolumeBuyer { get; set; }

        [JsonPropertyName("woodVolumeSeller")]
        public float WoodVolumeSeller { get; set; }
    }
}
