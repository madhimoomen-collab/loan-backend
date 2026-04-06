using AutoMapper;
using Data.Context;
using Data.Repositories;
using Domain.Commands;
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

// 4. Register Generic Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 5. Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(LoanApplication).Assembly);
});

// 6. Register generic handlers for loan entity

// LoanApplication handlers
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<LoanApplication>, IEnumerable<LoanApplication>>, GetListGenericHandler<LoanApplication>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<LoanApplication>, LoanApplication?>, GetGenericHandler<LoanApplication>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<LoanApplication>, LoanApplication>, AddGenericHandler<LoanApplication>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<LoanApplication>, LoanApplication>, UpdateGenericHandler<LoanApplication>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<LoanApplication>, bool>, DeleteGenericHandler<LoanApplication>>();

// User handlers
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<User>, IEnumerable<User>>, GetListGenericHandler<User>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<User>, User?>, GetGenericHandler<User>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<User>, User>, AddGenericHandler<User>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<User>, User>, UpdateGenericHandler<User>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<User>, bool>, DeleteGenericHandler<User>>();

// Role handlers
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<Role>, IEnumerable<Role>>, GetListGenericHandler<Role>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<Role>, Role?>, GetGenericHandler<Role>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<Role>, Role>, AddGenericHandler<Role>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<Role>, Role>, UpdateGenericHandler<Role>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<Role>, bool>, DeleteGenericHandler<Role>>();

// UserRole handlers
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<UserRole>, IEnumerable<UserRole>>, GetListGenericHandler<UserRole>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<UserRole>, UserRole?>, GetGenericHandler<UserRole>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<UserRole>, UserRole>, AddGenericHandler<UserRole>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<UserRole>, UserRole>, UpdateGenericHandler<UserRole>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<UserRole>, bool>, DeleteGenericHandler<UserRole>>();

// UserLoan handlers
builder.Services.AddScoped<IRequestHandler<GetListGenericQuery<UserLoan>, IEnumerable<UserLoan>>, GetListGenericHandler<UserLoan>>();
builder.Services.AddScoped<IRequestHandler<GetGenericQuery<UserLoan>, UserLoan?>, GetGenericHandler<UserLoan>>();
builder.Services.AddScoped<IRequestHandler<AddGenericCommand<UserLoan>, UserLoan>, AddGenericHandler<UserLoan>>();
builder.Services.AddScoped<IRequestHandler<UpdateGenericCommand<UserLoan>, UserLoan>, UpdateGenericHandler<UserLoan>>();
builder.Services.AddScoped<IRequestHandler<DeleteGenericCommand<UserLoan>, bool>, DeleteGenericHandler<UserLoan>>();

// 7. Register AutoMapper
builder.Services.AddAutoMapper(typeof(LoanApplicationMappingProfile).Assembly);

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
app.Logger.LogInformation("📚 Endpoints: LoanApprovals");

app.Run();