using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Interfaces;

namespace WebAPI.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper) 
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetUsersAsync();

            var usersToRetrn = _mapper.Map<IEnumerable<MemberDto>>(users);

            return Ok(usersToRetrn);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUsers(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null)
                return NotFound();

            return _mapper.Map<MemberDto>(user);
        }
    }
}
