using System.Xml.XPath;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private readonly IUserRepository repository;
    private readonly IMapper mapper;
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        repository = userRepository;
        this.mapper = mapper;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var userEntity = repository.FindById(userId);
        if (userEntity == null)
            return NotFound();
        var userDto = mapper.Map<UserDto>(userEntity);
        return Ok(userDto);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] PostUserDto userDto)
    {
        if (userDto == null)
            return BadRequest();
        if (!ModelState.IsValid || !userDto.Login.All(char.IsLetterOrDigit))
        {
            ModelState.AddModelError("Login", "Invalid login");
            return UnprocessableEntity(ModelState);
        }
        var user = repository.Insert(mapper.Map<UserEntity>(userDto));
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId  = user.Id },
            user.Id);
    }
    
    [HttpPut("{userId}")]
    [Produces("application/json", "application/xml")]
    public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] PutUserDto userDto)
    {
        if (userDto == null || userId == Guid.Empty)
            return BadRequest();
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        var userEntity = repository.FindById(userId) ?? new UserEntity(userId);
        var user = mapper.Map(userDto, userEntity);
        repository.UpdateOrInsert(user, out var isInserted);
        if (isInserted)
            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId = user.Id },
                user.Id);
        return NoContent();
    }
    
    [HttpPatch("{userId}")]
    [Produces("application/json", "application/xml")]
    public IActionResult PartiallyUpdateUser([FromRoute] Guid userId, [FromBody] JsonPatchDocument<PutUserDto> patchDoc)
    {
        var userEntity = repository.FindById(userId);
        if (patchDoc == null)
            return BadRequest();
        if (userId == Guid.Empty || userEntity == null)
            return NotFound();
        var userUpdate = new PutUserDto();
        patchDoc.ApplyTo(userUpdate, ModelState);
        if (!TryValidateModel(userUpdate))
            return UnprocessableEntity(ModelState);
        var user = mapper.Map(userUpdate, userEntity);
        repository.Update(user);
        return NoContent();
    }
}