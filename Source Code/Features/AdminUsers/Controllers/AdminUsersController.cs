using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.AdminUsers.Services;
namespace SpotifyClone.Features.AdminUsers.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UsersService _userService;
        public AdminUsersController(UsersService userService)
        {
            _userService = userService;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result.Success)
                return Ok(new { message = result.Message });
            else
                return BadRequest(new { message = result.Message });
        }
    }
}