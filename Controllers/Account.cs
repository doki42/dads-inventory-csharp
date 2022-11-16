using System.Security.Claims;
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
    
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        string redirectUrl = Url.Action("GoogleResponse", "Account");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return new ChallengeResult("Google", properties);
    }
 
    [AllowAnonymous]
    public async Task<IActionResult> GoogleResponse()
    {
        ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return RedirectToAction(nameof(Login));
 
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };
        if (result.Succeeded)
            return View(userInfo);
        else
        {
            User user = new User
            {
                Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                UserName = info.Principal.FindFirst(ClaimTypes.Email).Value
            };
 
            IdentityResult identResult = await _userManager.CreateAsync(user);
            if (identResult.Succeeded)
            {
                identResult = await _userManager.AddLoginAsync(user, info);
                if (identResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return View(userInfo);
                }
            }
            return AccessDenied();
        }
    }
    
    public IActionResult AccessDenied()
    {
        return View();
    }
}