using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sample.EFCore.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet("/login")]
        public async Task<IActionResult> Login()
        {
            var scope = new[]
            {
                "+read:sample.efcore.entities.product",
                "+read:sample.efcore.entities.category",
                "-read:sample.efcore.entities.product#name",
                "+write:sample.efcore.entities.product",
                "-write:sample.efcore.entities.product#name",
            };
            var claims = new List<Claim>
            {
                new Claim("scope", string.Join(" ", scope))
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Redirect("/sample");
        }
    }
}
