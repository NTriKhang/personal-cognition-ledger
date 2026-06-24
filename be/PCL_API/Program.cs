using Common.Application;
using Common.Infrastructure;
using Common.Infrastructure.Configuration;
using Common.Presentation.Endpoints;
using PCL.Modules.Evidence.Infrastructure;
using PCL.Modules.Session.Infrastructure;
using PCL.Modules.TaskPlanning.Infrastructure;
using PCL_API.Extensions;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

Assembly[] moduleApplicationAssemblies = [
    PCL.Modules.Evidence.Application.AssemblyReference.Assembly,
    PCL.Modules.Session.Application.AssemblyReference.Assembly,
    PCL.Modules.TaskPlanning.Application.AssemblyReference.Assembly];

builder.Services.AddApplication(moduleApplicationAssemblies);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");

builder.Services.AddInfrastructure(
    [
        TaskPlanningModule.ConfigureConsumers,
    ],
    databaseConnectionString);

builder.Services.AddLSessionModule(builder.Configuration);
builder.Services.AddTaskPlanningModule(builder.Configuration);
builder.Services.AddEvidenceModule(builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.MapEndpoints();
//app.UseAuthorization();

app.Run();

public partial class Program;
