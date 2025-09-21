using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ChoiceModels.Menu {

// -------------------------
// Root
// -------------------------
public sealed record ChoiceQrFullMenu(
    [property: JsonPropertyName("sections")] IReadOnlyList<ChoiceQrSection> Sections,
    [property: JsonPropertyName("categories")] IReadOnlyList<ChoiceQrCategory> Categories,
    [property: JsonPropertyName("menu")] IReadOnlyList<ChoiceQrDish> Menu);

// -------------------------
// Section
// -------------------------
public sealed record ChoiceQrSection(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("mode")] ChoiceQrSectionMode Mode,
    [property: JsonPropertyName("schedule")] IReadOnlyList<ChoiceQrSectionSchedule>? Schedule,
    [property: JsonPropertyName("active")] bool? Active,
    [property: JsonPropertyName("position")] int? Position,
    [property: JsonPropertyName("isDirectLink")] bool? IsDirectLink,
    [property: JsonPropertyName("showOutsideSchedule")] bool? ShowOutsideSchedule);

public sealed record ChoiceQrSectionMode(
    [property: JsonPropertyName("type")] ChoiceQrSectionModeType Type,
    [property: JsonPropertyName("link")] string? Link,
    [property: JsonPropertyName("staticDoc")] string? StaticDoc);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChoiceQrSectionModeType
{
    [EnumMember(Value = "static")] Static,
    [EnumMember(Value = "interactive")] Interactive,
    [EnumMember(Value = "link")] Link,
    [EnumMember(Value = "category")] Category
}

public sealed record ChoiceQrSectionSchedule(
    [property: JsonPropertyName("dayOfWeek")] int DayOfWeek,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("till")] string Till);

// -------------------------
// Category
// -------------------------
public sealed record ChoiceQrCategory(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("section")] string Section,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("posID")] string? PosId,
    [property: JsonPropertyName("position")] int? Position,
    [property: JsonPropertyName("active")] bool? Active);

// -------------------------
// Dish
// -------------------------
public sealed record ChoiceQrDish(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("active")] bool? Active,
    [property: JsonPropertyName("position")] int? Position,
    [property: JsonPropertyName("price")] int Price,
    [property: JsonPropertyName("weight")] string? Weight,
    [property: JsonPropertyName("weightType")] ChoiceQrWeightType WeightType,
    [property: JsonPropertyName("posID")] string? PosId,
    [property: JsonPropertyName("preparationTime")] int? PreparationTime,
    [property: JsonPropertyName("alcohol")] decimal? Alcohol,
    [property: JsonPropertyName("attributes")] IReadOnlyList<ChoiceQrDishAttribute>? Attributes,
    [property: JsonPropertyName("kcal")] int? Kcal,
    [property: JsonPropertyName("allergens")] IReadOnlyList<string>? Allergens,
    [property: JsonPropertyName("externalMedia")] IReadOnlyList<ChoiceQrExternalMedia>? ExternalMedia,
    [property: JsonPropertyName("pack")] ChoiceQrPack? Pack,
    [property: JsonPropertyName("media")] IReadOnlyList<ChoiceQrDishMedia>? Media,
    [property: JsonPropertyName("menuOptions")] IReadOnlyList<ChoiceQrMenuOption> MenuOptions,
    [property: JsonPropertyName("menuLabels")] IReadOnlyList<ChoiceQrMenuLabel> MenuLabels);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChoiceQrWeightType
{
    [EnumMember(Value = "g")] G,
    [EnumMember(Value = "kg")] Kg,
    [EnumMember(Value = "mm")] Mm,
    [EnumMember(Value = "m")] M,
    [EnumMember(Value = "ml")] Ml,
    [EnumMember(Value = "l")] L,
    [EnumMember(Value = "oz")] Oz
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChoiceQrDishAttribute
{
    [EnumMember(Value = "SOLD_OUT")] SoldOut
}

// External media
public sealed record ChoiceQrExternalMedia(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("type")] ChoiceQrExternalMediaType Type);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChoiceQrExternalMediaType
{
    [EnumMember(Value = "youtube")] Youtube
}

// Pack
public sealed record ChoiceQrPack(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("cost")] int Cost);

// Dish media
public sealed record ChoiceQrDishMedia(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("thumbnail")] string Thumbnail,
    [property: JsonPropertyName("medium")] string? Medium,
    [property: JsonPropertyName("big")] string? Big);

// -------------------------
// Menu options
// -------------------------
public sealed record ChoiceQrMenuOption(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("required")] bool? Required,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("countable")] bool? Countable,
    [property: JsonPropertyName("posID")] string? PosId,
    [property: JsonPropertyName("menuMaxCount")] int? MenuMaxCount,
    [property: JsonPropertyName("menuMinCount")] int? MenuMinCount,
    [property: JsonPropertyName("defaultIndex")] int? DefaultIndex,
    [property: JsonPropertyName("type")] ChoiceQrMenuOptionType Type,
    [property: JsonPropertyName("list")] IReadOnlyList<ChoiceQrOptionListItem> List);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChoiceQrMenuOptionType
{
    [EnumMember(Value = "single")] Single,
    [EnumMember(Value = "multiple")] Multiple
}

public sealed record ChoiceQrOptionListItem(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] int Price,
    [property: JsonPropertyName("default")] bool IsDefault,
    [property: JsonPropertyName("posID")] string? PosId);

// -------------------------
// Menu labels
// -------------------------
public sealed record ChoiceQrMenuLabel(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("type")] ChoiceQrMenuLabelType Type,
    [property: JsonPropertyName("custom")] bool? Custom,
    [property: JsonPropertyName("name")] string? Name);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChoiceQrMenuLabelType
{
    [EnumMember(Value = "vegetarian")] Vegetarian,
    [EnumMember(Value = "gluten")] Gluten,
    [EnumMember(Value = "spicy")] Spicy,
    [EnumMember(Value = "middle-spicy")] MiddleSpicy,
    [EnumMember(Value = "recommended")] Recommended,
    [EnumMember(Value = "vegan")] Vegan,
    [EnumMember(Value = "new")] New,
    [EnumMember(Value = "custom")] Custom
}

}