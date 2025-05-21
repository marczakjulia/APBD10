using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DTO;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                       throw new InvalidOperationException("No connection string found");

builder.Services.AddDbContext<MasterContext>(options => options.UseSqlServer(connectionString));


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//get all devices 
app.MapGet("/api/devices", async (MasterContext db, CancellationToken cancellationToken) =>
{
    try
    {
        var devices = await db.Devices
            .Select(d => new DeviceDto(d.Id, d.Name)) //what i am getting, so new devicedto 
            .ToListAsync(cancellationToken);
        return Results.Ok(devices);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

//get by id
app.MapGet("/api/devices/{id}", async (int id, MasterContext db, CancellationToken cancellationToken) =>
{
    try
    {
        //eagerly load device types, and colection of deviceempl,  then for each deviceemp loads employee, and for each emplyee person. kinda like a lot of left joins
        var device = await db.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceEmployees)
            .ThenInclude(de => de.Employee)
            .ThenInclude(e => e.Person)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (device is null)
            return Results.NotFound($"Device {id} not found.");
        //i am not boxing as object, in this solution. what i am doing is i know im working with json element, and the API will return JSON object.
        //if this is not the correct approach, i was absent during the classes hence i am happy to correct it for it to work properly
        var additionalProps = JsonDocument.Parse(device.AdditionalProperties ?? "{}").RootElement;
        
        var currentAssignment = device.DeviceEmployees
            .FirstOrDefault(de => de.ReturnDate == null);

        CurrentUserDTO? currentUser = null;
        if (currentAssignment != null)
        {
            var person = currentAssignment.Employee.Person!;
            currentUser = new CurrentUserDTO
            {
                Id   = currentAssignment.EmployeeId,
                Name = person.FirstName + " " + person.LastName,
            };
        }
        var dto = new DeviceDtoById
        {
            Name = device.Name,
            DeviceTypeName = device.DeviceType!.Name,
            IsEnabled = device.IsEnabled,
            AdditionalProperties= additionalProps,
            CurrentUser = currentUser
        };
        return Results.Ok(dto);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/devices", async (CreateDevice dev, MasterContext db, CancellationToken cancellationToken) =>
{
    try
    {
        //i also added this, so that there is en error if someone inputs null for the additionalProperties 
        if (dev.AdditionalProperties.ValueKind == JsonValueKind.Null)
        {
            return Results.BadRequest("AdditionalProperties cannot be null.");
        }

        var type = await db.DeviceTypes
            .SingleOrDefaultAsync(t => t.Name == dev.DeviceTypeName, cancellationToken);

        if (type is null)
            return Results.BadRequest($"Unknown device type '{dev.DeviceTypeName}'.");
        var device = new Device
        {
            Name = dev.Name,
            DeviceTypeId = type.Id,
            IsEnabled = dev.IsEnabled,
            AdditionalProperties = dev.AdditionalProperties.GetRawText()
        };
        db.Devices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        var updatedDto = new
        {
            id = device.Id,
            name = device.Name,
            deviceTypeName  = type.Name,
            isEnabled = device.IsEnabled,
            additionalProperties = JsonDocument.Parse(device.AdditionalProperties ?? "{}").RootElement
        };
        return Results.Created($"/api/devices/{device.Id}", updatedDto);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});



//update
app.MapPut("/api/devices/{id}", async (int id, CreateDevice dev, MasterContext db, CancellationToken cancellationToken) =>
{

    try
    {
        var device = await db.Devices.FindAsync(id, cancellationToken);

        if (device is null)
            return Results.NotFound($"Device {id} not found.");

        var type = await db.DeviceTypes
            .SingleOrDefaultAsync(t => t.Name == dev.DeviceTypeName, cancellationToken);
        if (type is null)
            return Results.BadRequest($"Unknown device type '{dev.DeviceTypeName}'.");
        device.Name = dev.Name;
        device.DeviceTypeId = type.Id;
        device.IsEnabled = dev.IsEnabled;
        device.AdditionalProperties = dev.AdditionalProperties.GetRawText();

        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

//delete
//since it wasnt specifies in the file, what i am doing is checking if a device belongs to any employee
//if yes i do not delete it
//another way would be to delete the device from the connection and then safetly delete it but im taking the easy way out :)
app.MapDelete("/api/devices/{id}", async (int id, MasterContext db, CancellationToken cancellationToken) =>
{
    try
    {
        var device = await db.Devices.FindAsync(id, cancellationToken);
        if (device is null)
            return Results.NotFound($"Device {id} not found");

        var isAssigned = await db.DeviceEmployees
            .AnyAsync(de => de.DeviceId == id, cancellationToken);
        if (isAssigned)
            return Results.BadRequest($"Cannot delete device {id} because it is associated with an employee."
            );

        db.Devices.Remove(device);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();

    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message);
    }
});

//get all employees
app.MapGet("/api/employees", async (MasterContext db, CancellationToken cancellationToken) =>
{
    try
    {
        var employees = await db.Employees
            .Include(e => e.Person) 
            .Select(e => new EmployeeDTO
            {
                Id = e.Id,
                Name = e.Person.FirstName +" " + e.Person.MiddleName +" " + e.Person.LastName
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(employees);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/employees/{id}", async (
    int id,
    MasterContext db,
    CancellationToken cancellationToken) =>
{
    try
    {
        var emp = await db.Employees
            .Where(e => e.Id == id)
            .Include(e => e.Person)
            .Include(e => e.Position)
            .Select(e => new EmployeeGetById
            {
                Person = new PersonDTO
                {
                    PassportNumber = e.Person.PassportNumber,
                    FirstName = e.Person.FirstName,
                    MiddleName = e.Person.MiddleName,
                    LastName = e.Person.LastName,
                    PhoneNumber = e.Person.PhoneNumber,
                    Email = e.Person.Email
                },
                Salary   = e.Salary,
                Position = new PositionDTO()
                {
                    Id = e.Position.Id,
                    Name = e.Position.Name
                },
                HireDate = e.HireDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (emp is null)
            return Results.NotFound($"Employee {id} not found");

        return Results.Ok(emp);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message);
    }
});

app.Run();