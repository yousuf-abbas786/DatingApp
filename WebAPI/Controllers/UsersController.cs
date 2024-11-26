using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.Runtime.Serialization;
using System.Security.Claims;

using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Extensions;
using WebAPI.Helpers;
using WebAPI.Interfaces;

namespace WebAPI.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;   
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IPhotoService photoService, IMapper mapper)
        {
            _userRepository = userRepository;
            _photoService = photoService;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            userParams.CurrentUsername = User.GetUsername();
            var users = await _userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users);

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _userRepository.GetMemberAsync(username);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPut]

        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUdateDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("Could not find user");

            _mapper.Map(memberUdateDto, user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("Cannot update user");

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
                return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem adding photo");


        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("Could not find user");

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("User not found");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");
        }

    }
}
