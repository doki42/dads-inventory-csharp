using System.Threading.Tasks;
using DadsInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DadsInventory.Controllers;

[Authorize]
public class Account : Controller
{
    private UserManager<User> _userManager;

    private SignInManager<User> _signInManager;

    public Account(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    [AllowAnonymous]
    public IActionResult Login(string returnUrl)
    {
        Login login = new Login();
        login.ReturnUrl = returnUrl;
        return View(login);
    }
 
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(Login login)
    {
        if (ModelState.IsValid)
        {
            User appUser = await _userManager.FindByEmailAsync(login.Email);
            if (appUser != null)
            {
                await _signInManager.SignOutAsync();
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, login.Password, false, false);
                if (result.Succeeded)
                    return Redirect(login.ReturnUrl ?? "/");
            }
            ModelState.AddModelError(nameof(login.Email), "Login Failed: Invalid Email or password");
        }
        return View(login);
    }
    
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    
}