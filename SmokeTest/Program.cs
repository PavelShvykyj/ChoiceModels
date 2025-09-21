using System.Text.Json;
using ChoiceModels.Menu;
using ChoiceModels.Order;
using ChoiceModels.Events;

Console.WriteLine("Smoke Test for JSON Deserialization (.NET 8)");
Console.WriteLine("Usage: SmokeTest <file-path> <type>");
Console.WriteLine("type: 'menu', 'order', or 'event'");
Console.WriteLine();

if (args.Length < 2)
{
    Console.WriteLine("Error: Missing arguments");
    return;
}


var filePath = args[0];
if (!File.Exists(filePath))
{
    Console.WriteLine($"Error: File not found at {filePath}");
    return;
}

var json = File.ReadAllText(filePath);


var modelType = args[1].ToLower();

if (string.IsNullOrWhiteSpace(json))
{
    Console.WriteLine("Error: JSON string is empty");
    return;
}

try
{
    switch (modelType)
    {
        case "menu":
            TestMenu(json);
            break;

        case "order":
            TestOrder(json);
            break;
        case "event":
            TestEvent(json);
            break;
        default:
            Console.WriteLine("Error: Unknown type. Use 'menu' or 'order'");
            break;
    }
}
catch (JsonException ex)
{
    Console.WriteLine($"JSON parse error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex}");
}

// --------------------------
// Local functions
// --------------------------
void TestMenu(string json)
{
    Console.WriteLine("Deserializing as ChoiceQrFullMenu...");
    var menu = JsonSerializer.Deserialize<ChoiceQrFullMenu>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    });

    if (menu == null)
    {
        Console.WriteLine("Failed to deserialize menu.");
        return;
    }

    Console.WriteLine("=== MENU SUMMARY ===");
    Console.WriteLine($"Sections: {menu.Sections.Count}");
    Console.WriteLine($"Categories: {menu.Categories.Count}");
    Console.WriteLine($"Dishes: {menu.Menu.Count}");

    if (menu.Categories.Count > 0)
        Console.WriteLine($"First Category: {menu.Categories[0].Name}");

    if (menu.Menu.Count > 0)
        Console.WriteLine($"First Dish: {menu.Menu[0].Name}");
}

void TestOrder(string json)
{
    Console.WriteLine("Deserializing as OrderDto...");
    var order = JsonSerializer.Deserialize<OrderDto>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    });

    if (order == null)
    {
        Console.WriteLine("Failed to deserialize order.");
        return;
    }

    Console.WriteLine("=== ORDER SUMMARY ===");
    Console.WriteLine($"Order Id: {order.Id}");
    Console.WriteLine($"Status: {order.Status}");
    Console.WriteLine($"Total: {order.Total} {order.Currency}");
    Console.WriteLine($"Items count: {order.Items.Count}");

    if (order.Items.Count > 0)
    {
        var firstItem = order.Items[0];
        Console.WriteLine($"First item: {firstItem.Name.Uk?.Name}");
    }
}

void TestEvent(string json)
{
    var evt = JsonSerializer.Deserialize<EventEnvelope>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    });

    if (evt == null)
    {
        Console.WriteLine("Failed to deserialize event.");
        return;
    }

    Console.WriteLine($"Event ID: {evt.Id}");
    Console.WriteLine($"Type: {evt.Type}");
    Console.WriteLine($"LangCode: {evt.LangCode}");
    Console.WriteLine($"Timestamp: {evt.Timestamp}");
    Console.WriteLine($"IsOrderCreated: {evt.IsOrderCreated}");

    if (evt.IsOrderCreated)
    {
        var order = evt.TryGetOrder();
        Console.WriteLine(order != null
            ? $"Order ID: {order.Id}, Total: {order.Total} {order.Currency}, Status: {order.Status}"
            : "Failed to parse order data.");
    }
}