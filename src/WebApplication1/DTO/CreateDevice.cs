using System.ComponentModel.DataAnnotations;
using System.Text.Json;


namespace WebApplication1.DTO;

public class CreateDevice
{
    //used for creation hence the required 
    //in here keyword added so that the 400 is returned 
    [Required]
    public required string Name { get; set; } = null!;
    [Required]
    public required string DeviceTypeName { get; set; }  = null!;
    
    [Required]
    public required bool IsEnabled {get; set;}
    
    [Required] 
    public required JsonElement AdditionalProperties { get; set; } 
}