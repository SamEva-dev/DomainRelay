using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Sample.AspNetCoreApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDomainRelayMapping(options =>
{
    options.AddProfile<UserProfile>();
});

var app = builder.Build();

app.MapGet("/map", (IObjectMapper mapper) =>
{
    var dto = mapper.Map<User, UserDto>(new User
    {
        Id = Guid.NewGuid(),
        FirstName = "Sam",
        LastName = "Fokam"
    });

    return Results.Ok(dto);
});

app.Run();