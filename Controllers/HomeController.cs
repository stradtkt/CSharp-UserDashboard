using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserDashboard.Models;

namespace UserDashboard.Controllers
{
    public class HomeController : Controller
    {
  private UserContext _uContext;

        public HomeController(UserContext context)
        {
            _uContext = context;    
        }
        private User ActiveUser 
        {
            get 
            {
                return _uContext.users.Where(u => u.user_id == HttpContext.Session.GetInt32("user_id")).FirstOrDefault();
            }
        }
        [HttpGet("")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("registeruser")]
        public IActionResult RegisterUser(RegisterUser newuser)
        {
            User CheckEmail = _uContext.users
                .Where(u => u.email == newuser.email)
                .SingleOrDefault();

            if(CheckEmail != null)
            {
                ViewBag.errors = "That email already exists";
                return RedirectToAction("Register");
            }
            if(ModelState.IsValid)
            {
                PasswordHasher<RegisterUser> Hasher = new PasswordHasher<RegisterUser>();
                User newUser = new User
                {
                    user_id = newuser.user_id,
                    first_name = newuser.first_name,
                    last_name = newuser.last_name,
                    email = newuser.email,
                    password = Hasher.HashPassword(newuser, newuser.password)
                  };
                _uContext.Add(newUser);
                _uContext.SaveChanges();
                ViewBag.success = "Successfully registered";
                return RedirectToAction("Login");
            }
            else
            {
                return View("Register");
            }
        }

        [HttpPost("loginuser")]
        public IActionResult LoginUser(LoginUser loginUser) 
        {
            User CheckEmail = _uContext.users
                .SingleOrDefault(u => u.email == loginUser.email);
            if(CheckEmail != null)
            {
                var Hasher = new PasswordHasher<User>();
                if(0 != Hasher.VerifyHashedPassword(CheckEmail, CheckEmail.password, loginUser.password))
                {
                    HttpContext.Session.SetInt32("user_id", CheckEmail.user_id);
                    HttpContext.Session.SetString("first_name", CheckEmail.first_name);
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ViewBag.errors = "Incorrect Password";
                    return View("Register");
                }
            }
            else
            {
                ViewBag.errors = "Email not registered";
                return View("Register");
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                List<User> users = _uContext.users
                    .Include(m => m.Messages)
                    .ThenInclude(m => m.Users)
                    .Include(c => c.Comments)
                    .OrderByDescending(d => d.created_at).ToList();
                ViewBag.users = users;
                ViewBag.user = ActiveUser;
            }
            return View();
        }
        [HttpGet("Profile/{user_id}")]
        public IActionResult Profile(int user_id)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                User user = _uContext.users
                    .Include(m => m.Messages)
                    .ThenInclude(m => m.Users)
                    .Include(c => c.Comments)
                    .Where(u => u.user_id == user_id)
                    .SingleOrDefault();
                List<Message> messages = _uContext.messages
                    .Include(u => u.Users)
                    .ThenInclude(u => u.Messages)
                    .ThenInclude(u => u.Comments)
                    .Include(c => c.Comments)
                    .ThenInclude(c => c.Users)
                    .ThenInclude(c => c.Messages)
                    .ToList();
                List<Comment> comments = _uContext.comments
                    .Include(u => u.Users)
                    .Include(m => m.Messages)
                    .ThenInclude(m => m.Users)
                    .ToList();
                ViewBag.active = ActiveUser;
                ViewBag.comments = comments;
                ViewBag.messages = messages;
                ViewBag.user = user;
            }
            return View();
        }

        [HttpPost("process_message")]
        public IActionResult ProcessMessage(Message msg)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                if(ModelState.IsValid)
                {
                    Message mess = new Message
                    {
                        user_id = ActiveUser.user_id,
                        message = msg.message
                    };
                    _uContext.messages.Add(mess);
                    _uContext.SaveChanges();
                    return RedirectToAction("Dashboard");
                }
            }
                List<Message> messages = _uContext.messages
                    .Include(u => u.Users)
                    .Include(c => c.Comments)
                    .ThenInclude(c => c.Users)
                    .ToList();
                ViewBag.messages = messages;
            return RedirectToAction("Dashboard");
        }
        [HttpPost("ProcessComment")]
        public IActionResult ProcessComment(string comm, int message_id)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Comment com = new Comment
                {
                    comment = comm,
                    message_id = message_id,
                    user_id = ActiveUser.user_id
                };
                _uContext.Add(com);
                _uContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
        }
        [HttpGet("AddUser")]
        public IActionResult AddUser()
        {
           return View();
        }
          [HttpPost("ProcessUser")]
        public IActionResult ProcessUser(AddUser user)
        {
            User CheckEmail = _uContext.users
                .Where(u => u.email == user.email)
                .SingleOrDefault();

            if(CheckEmail != null)
            {
                ViewBag.errors = "That email already exists";
                return RedirectToAction("Register");
            }
            if(ModelState.IsValid)
            {
                PasswordHasher<AddUser> Hasher = new PasswordHasher<AddUser>();
                User newUser = new User
                {
                    user_id = user.user_id,
                    first_name = user.first_name,
                    last_name = user.last_name,
                    email = user.email,
                    password = Hasher.HashPassword(user, user.password)
                  };
                _uContext.Add(newUser);
                _uContext.SaveChanges();
                ViewBag.success = "Successfully Added";
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Dashboard");
            }
        }


        [HttpGet("DeleteUser/{user_id}")]
        public IActionResult DeleteUser(int user_id)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                User toDelete = _uContext.users.Where(u => u.user_id == user_id).SingleOrDefault();
                _uContext.users.Remove(toDelete);
                _uContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
        }
        [HttpGet("comments/{message_id}")]
        public IActionResult Comments(int message_id)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Message message = _uContext.messages
                    .Include(u => u.Users)
                    .Include(c => c.Comments)
                    .ThenInclude(c => c.Users)
                    .Where(m => m.message_id == message_id)
                    .SingleOrDefault();
                List<Comment> comments = _uContext.comments
                    .Include(u => u.Users)
                    .Include(m => m.Messages)
                    .ThenInclude(c => c.Comments)
                    .ToList();
                ViewBag.user = ActiveUser;
                ViewBag.comments = comments;
                ViewBag.message = message;
                return View();
            }
        }
        [HttpPost("delete_message/{message_id}")]
        public IActionResult DeleteMessage(int message_id)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Message toDelete = _uContext.messages.Where(m => m.message_id == message_id).SingleOrDefault();
                List<Comment> comments = _uContext.comments.Include(m => m.Messages).Where(m => m.message_id == message_id).ToList();
                _uContext.comments.RemoveRange(comments);
                _uContext.messages.Remove(toDelete);
                _uContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
        }
        [HttpPost("delete_comment/{comment_id}")]
        public IActionResult DeleteComment(int comment_id)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Comment toDelete = _uContext.comments.Where(c => c.comment_id == comment_id).SingleOrDefault();
                _uContext.comments.Remove(toDelete);
                _uContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet("EditUser/{user_id}")]
        public IActionResult EditUser(int user_id)
        {
            if(ActiveUser == null) 
            {
                return RedirectToAction("Login");
            }
            User user = _uContext.users.Where(u => u.user_id == user_id).SingleOrDefault();
            ViewBag.success = TempData["success"];
            ViewBag.user = user;
            return View();
        }
        [Route("{user_id}/ProcessEditUser")]
        public IActionResult ProcessEditUser(int user_id, string first_name, string last_name, string email)
        {
            if(ActiveUser == null)
            {
                return RedirectToAction("Login");
            }
            User user = _uContext.users.Where(u => u.user_id == user_id).SingleOrDefault();
            user.first_name = first_name;
            user.last_name = last_name;
            user.email = email;
            _uContext.SaveChanges();
            TempData["success"] = "User successfully edited";
            return Redirect("/EditUser/"+ user_id);
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
