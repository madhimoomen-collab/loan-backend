using Microsoft.EntityFrameworkCore;
using Data.Context;
using Domain.Interface;
using Data.Repositories;
using Domain.Handlers;
using Domain.Models;
using Domain.Queries;
using Domain.Commands;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Workflow Management API",
        Version = "v1",
        Description = "Generic Workflow Management System - PFE Project"
    });
});

// 3. Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Data")
    ));

// 4. Register Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 5. Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Book).Assembly);
});

// 6. Register Generic Handlers Explicitly (THE FIX!)
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<Book>, IEnumerable<Book>>, GetListGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<Book>, Book>, GetGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<Book>, Book>, AddGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<Book>, Book>, UpdateGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<Book>, bool>, DeleteGenericHandler<Book>>();

// 7. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 8. Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// 9. Configure Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("🚀 Workflow Management API Started!");

app.Run();