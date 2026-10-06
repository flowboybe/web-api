using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<UserEntity, UserDto>()
        .ForMember(
            dest => dest.FullName,
            opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName}"));

    cfg.CreateMap<PostUserDto, UserEntity>();
}, typeof(Program).Assembly);


builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddSingleton<WebApi.MinimalApi.Domain.IUserRepository, WebApi.MinimalApi.Domain.InMemoryUserRepository>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<WebApi.MinimalApi.Domain.UserEntity, WebApi.MinimalApi.Models.UserDto>()
        .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName}"));
    cfg.CreateMap<WebApi.MinimalApi.Models.PostUserDto, WebApi.MinimalApi.Domain.UserEntity>();
}, typeof(Program).Assembly);

builder.Services.AddControllers(options =>
    {
        options.ReturnHttpNotAcceptable = true;
        options.RespectBrowserAcceptHeader = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
        options.SuppressMapClientErrors = true;
    })
    .AddXmlSerializerFormatters()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver =
            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

        options.SerializerSettings.DefaultValueHandling =
            Newtonsoft.Json.DefaultValueHandling.Populate;
    });

var app = builder.Build();

app.MapControllers();

app.Run();

