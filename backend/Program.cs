var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // ✅ registers controllers
builder.Services.AddOpenApi(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers(); // ✅ this is required for HelloController to work

app.Run();
