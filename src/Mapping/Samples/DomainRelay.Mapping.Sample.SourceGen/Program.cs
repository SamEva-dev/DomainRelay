using DomainRelay.Mapping.Sample.SourceGen;
using DomainRelay.Mapping.Generated;

var source = new User
{
    Id = Guid.NewGuid(),
    FirstName = "Sam",
    LastName = "Fokam"
};

var dto = GeneratedMappings.Map_User_To_UserDto(source);

Console.WriteLine(dto.Id);
Console.WriteLine(dto.FirstName);
Console.WriteLine(dto.LastName);

var registry = new GeneratedMappingRegistry();
if (registry.TryGetGeneratedMapper(typeof(User), typeof(UserDto), out var mapper) && mapper is not null)
{
    var boxed = (UserDto)mapper(source);
    Console.WriteLine(boxed.FirstName);
}