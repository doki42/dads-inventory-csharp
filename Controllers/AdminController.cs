using System.Threading.Tasks;
using DadsInventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DadsInventory.Controllers;

public class AdminController : Controller
{
    private UserManager<User> _userManager;
    private IPasswordHasher<User> passwordHasher;

    public AdminController(UserManager<User> userManager, IPasswordHasher<User> passwordHasher)
    {
        _userManager = userManager;
        this.passwordHasher = passwordHasher;
    }

    // GET
    public IActionResult Index()
    {
        return View(_userManager.Users);
    }
    
    public ViewResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(AppUSer user)
    {
        if (ModelState.IsValid)
        {
            User appUser = new User
            {
                UserName = user.Name,
                Email = user.Email
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);

            if (result.Succeeded)
                return RedirectToAction("Index");
            else
            {
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
        }
        return View(user);
    }
    
    public async Task<IActionResult> Update(string id)
    {
        User user = await _userManager.FindByIdAsync(id);
        if (user != null)
            return View(user);
        else
            return RedirectToAction("Index");
    }
 
    [HttpPost]
    public async Task<IActionResult> Update(string id, string email, string password)
    {
        User user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(email))
                user.Email = email;
            else
                ModelState.AddModelError("", "Email cannot be empty");
 
            if (!string.IsNullOrEmpty(password))
                user.PasswordHash = passwordHasher.HashPassword(user, password);
            else
                ModelState.AddModelError("", "Password cannot be empty");
 
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
        }
        else
            ModelState.AddModelError("", "User Not Found");
        return View(user);
    }
 
    private void Errors(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
            ModelState.AddModelError("", error.Description);
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        User user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            IdentityResult result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return RedirectToAction("Index");
            else
                Errors(result);
        }
        else
            ModelState.AddModelError("", "User Not Found");
        return View("Index", _userManager.Users);
    }
}