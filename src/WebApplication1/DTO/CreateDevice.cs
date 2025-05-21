using System.ComponentModel.DataAnnotations;
using System.Text.Json;


namespace WebApplication1.DTO;

public class CreateDevice
{
    //used for creation hence the required 
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string DeviceTypeName { get; set; }  = null!;
    
    [Required]
    public bool IsEnabled {get; set;}
    
    [Required] 
    public JsonElement AdditionalProperties { get; set; } 
}