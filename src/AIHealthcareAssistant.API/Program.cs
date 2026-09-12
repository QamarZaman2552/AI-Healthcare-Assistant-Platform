using AIHealthcareAssistant.API;
using AIHealthcareAssistant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi();

var app = builder.Build();


app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();
app.UseApi();
app.MapControllers();

app.Run();