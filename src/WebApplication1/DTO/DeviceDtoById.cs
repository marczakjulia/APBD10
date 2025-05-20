using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.DTO;

public class DeviceDtoById
{
    public string Name { get; set; } = null!;
    public bool IsEnabled { get; set; }

    public JsonElement AdditionalProperties { get; set; }

    public string DeviceTypeName { get; set; }
    
    public CurrentUserDTO? CurrentUser { get; set; }
}