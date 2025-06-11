using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using nge_eatweb.Models;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace nge_eatweb.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            User? user = null;
            string hashPassword = ComputeSha256Hash(Password); // hash password input user

            string connectionString = _config.GetConnectionString("DefaultConnection")!;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM users WHERE email = @Email AND password = @Password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Password", hashPassword);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = (int)reader["id"],
                                Email = reader["email"].ToString()!,
                                Created_At = Convert.ToDateTime(reader["created_at"]),
                                Updated_At = Convert.ToDateTime(reader["updated_at"])
                            };
                        }
                    }
                }
            }

            if (user != null)
            {
                // Buat claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("email", user.Email),
                    new Claim("id", user.Id.ToString())
                };

                // Buat identity dan principal
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Login (SignIn)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.Error = "Email atau Password salah!";
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }

        // Fungsi hashing SHA-256
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
