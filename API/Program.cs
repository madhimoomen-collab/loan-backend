using AutoMapper;
using Data.Context;
using Data.Repositories;
using Domain.Commands;
using Domain.DTOs;
using Domain.Handlers;
using Domain.Interface;
using Domain.Mappings;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

// 6. Register Generic Handlers Explicitly
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<Book>, IEnumerable<Book>>, GetListGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<Book>, Book?>, GetGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<Book>, Book>, AddGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<Book>, Book>, UpdateGenericHandler<Book>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<Book>, bool>, DeleteGenericHandler<Book>>();

// 6.1 Register Generic Handlers for Client
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<Client>, IEnumerable<Client>>, GetListGenericHandler<Client>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<Client>, Client?>, GetGenericHandler<Client>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<Client>, Client>, AddGenericHandler<Client>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<Client>, Client>, UpdateGenericHandler<Client>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<Client>, bool>, DeleteGenericHandler<Client>>();

// 6.2 Register GetBooksByClientHandler
builder.Services.AddScoped<IRequestHandler<GetBooksByClientQuery, IEnumerable<BookDto>>, GetBooksByClientHandler>();

// 7. Register AutoMapper manually (AutoMapper 13+)
builder.Services.AddAutoMapper(typeof(BookMappingProfile));

// 8. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 9. Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// 10. Configure Middleware
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