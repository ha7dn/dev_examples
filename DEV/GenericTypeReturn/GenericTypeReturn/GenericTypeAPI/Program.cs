using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using GenericTypeReturn;
namespace GenericTypeAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var assembly = Assembly.Load("GenericTypeReturn");


            builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                            .AddApplicationPart(typeof(GenericType<>).Assembly);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();


            app.Run();
        }
    }
}