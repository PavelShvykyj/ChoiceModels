using System.Text.Json.Serialization;

namespace ChoiceModels.Order
{
    public sealed record OrderDto(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("costOfPack")] int CostOfPack,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("cutlery")] CutleryDto Cutlery,
    [property: JsonPropertyName("delivery")] DeliveryDto Delivery,
    [property: JsonPropertyName("discount")] int Discount,
    [property: JsonPropertyName("discountData")] DiscountDataDto DiscountData,
    [property: JsonPropertyName("guid")] string Guid,
    [property: JsonPropertyName("items")] IReadOnlyList<OrderItemDto> Items,
    [property: JsonPropertyName("lang")] string Language,
    [property: JsonPropertyName("location")] LocationDto Location,
    [property: JsonPropertyName("num")] int Number,
    [property: JsonPropertyName("orderLocalTime")] DateTimeOffset OrderLocalTime,
    [property: JsonPropertyName("payBy")] string PayBy,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("subTotal")] int SubTotal,
    [property: JsonPropertyName("timestamps")] TimestampDto Timestamps,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("tips")] int Tips,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("type")] string Type
);

    public sealed record CutleryDto(
        [property: JsonPropertyName("personsCount")] int PersonsCount,
        [property: JsonPropertyName("posID")] string? PosId,
        [property: JsonPropertyName("withCutlery")] bool WithCutlery
    );

    public sealed record DeliveryDto(
        [property: JsonPropertyName("comment")] string? Comment,
        [property: JsonPropertyName("cost")] int Cost,
        [property: JsonPropertyName("customer")] CustomerDto Customer,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("when")] string? When,
        [property: JsonPropertyName("whenUTC")] string? WhenUtc
    );

    public sealed record CustomerDto(
        [property: JsonPropertyName("address")] AddressDto Address,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("phone")] string Phone
    );

    public sealed record AddressDto(
        [property: JsonPropertyName("apartment")] string? Apartment,
        [property: JsonPropertyName("city")] string City,
        [property: JsonPropertyName("location")] GeoLocationDto Location,
        [property: JsonPropertyName("prediction")] string? Prediction,
        [property: JsonPropertyName("state")] string? State,
        [property: JsonPropertyName("streetName")] string? StreetName,
        [property: JsonPropertyName("streetNumber")] string? StreetNumber
    );

    public sealed record GeoLocationDto(
        [property: JsonPropertyName("coordinates")] IReadOnlyList<double> Coordinates,
        [property: JsonPropertyName("type")] string Type
    );

    public sealed record DiscountDataDto(
        [property: JsonPropertyName("areaDiscount")] int AreaDiscount,
        [property: JsonPropertyName("loyaltyDiscount")] int LoyaltyDiscount,
        [property: JsonPropertyName("promocodeDiscount")] int PromocodeDiscount
    );

    public sealed record OrderItemDto(
        [property: JsonPropertyName("_id")] string Id,
        [property: JsonPropertyName("alcohol")] bool Alcohol,
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("name")] LocalizedOrderName Name,
        [property: JsonPropertyName("pack")] PackDto? Pack,
        [property: JsonPropertyName("posID")] string? PosId,
        [property: JsonPropertyName("price")] int Price,
        [property: JsonPropertyName("recommendation")] bool Recommendation,
        [property: JsonPropertyName("total")] int Total,
        [property: JsonPropertyName("vat")] int Vat
    );

    public sealed record LocalizedOrderName(
        [property: JsonPropertyName("uk")] OrderNameLang? Uk,
        [property: JsonPropertyName("en")] OrderNameLang? En
    );

    public sealed record OrderNameLang(
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("name")] string Name
    );

    public sealed record PackDto(
        [property: JsonPropertyName("cost")] int Cost,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("posID")] string? PosId
    );

    public sealed record LocationDto(
        [property: JsonPropertyName("_id")] string Id,
        [property: JsonPropertyName("area")] string Area,
        [property: JsonPropertyName("i18n")] Dictionary<string, LocationLang> I18n,
        [property: JsonPropertyName("posID")] string? PosId,
        [property: JsonPropertyName("type")] string Type
    );

    public sealed record LocationLang(
        [property: JsonPropertyName("name")] string Name
    );

    public sealed record TimestampDto(
        [property: JsonPropertyName("created")] DateTimeOffset Created,
        [property: JsonPropertyName("payment")] DateTimeOffset Payment
    );
}
