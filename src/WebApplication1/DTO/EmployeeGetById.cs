namespace WebApplication1.DTO;

public class EmployeeGetById
{
    public PersonDTO Person { get; set; } = null!;
    public decimal Salary { get; set; }
    public PositionDTO Position { get; set; } = null!;
    public DateTime HireDate { get; set; }
}