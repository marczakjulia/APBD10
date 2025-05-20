namespace WebApplication1.DTO;

public class DeviceDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DeviceDto(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
}