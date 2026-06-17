using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        public AccountController(IAuthService authService) => _authService = authService;

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _authService.LoginAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);

            TempData["Success"] = $"Welcome back, {user.FullName}!";

            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            var success = await _authService.RegisterAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", "An account with this email already exists.");
                return View(model);
            }

            TempData["Success"] = "Account created! Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var user = await _authService.GetUserByIdAsync(userId.Value);
            return View(user);
        }

        // ── Google OAuth ───────────────────────────────────────────
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Account")
            };
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded)
            {
                TempData["Error"] = "Google login failed. Please try again.";
                return RedirectToAction("Register");
            }

            // Fix: null-safe access for principal and its claims
            var principal = result.Principal;
            if (principal == null)
            {
                TempData["Error"] = "Google login failed. No user principal returned.";
                return RedirectToAction("Register");
            }

            var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Could not retrieve email from Google.";
                return RedirectToAction("Register");
            }

            // Use email as fallback display name if Google didn't return one
            var displayName = string.IsNullOrEmpty(name) ? email : name;

            // Auto-register user if they don't exist
            var model = new RegisterViewModel
            {
                Email = email,
                FullName = displayName,
                Password = Guid.NewGuid().ToString(), // random — they sign in via Google
                ConfirmPassword = string.Empty
            };
            await _authService.RegisterGoogleUserAsync(model);

            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "Login failed. Please try again.";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);

            TempData["Success"] = $"Welcome, {user.FullName}!";
            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }
    }
}