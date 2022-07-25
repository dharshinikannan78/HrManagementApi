using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HrMangementApi.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly UserdbContext dataContext;
        public LoginController(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }

        [HttpPost("Login")]
        public IActionResult GetLogin([FromBody] Login loginData)
        {
            var user = dataContext.LoginModels.Where(q => q.UserName == loginData.UserName && q.Password == loginData.Password);
            return Ok(user);
        }

        [HttpPost("AddUser")]
        public IActionResult AddUserlogin([FromBody] Login loginData)
        {

            dataContext.LoginModels.Add(loginData);
            dataContext.SaveChanges();
            return Ok(loginData);
        }

        [HttpDelete("Delete")]
        public IActionResult DeletUser(int id)
        {
            var delete = dataContext.LoginModels.Find(id);
            if (delete == null)
            {
                return NotFound();
            }
            else
            {
                dataContext.LoginModels.Remove(delete);
                dataContext.SaveChanges();
                return Ok();
            }
        }

    }
}
