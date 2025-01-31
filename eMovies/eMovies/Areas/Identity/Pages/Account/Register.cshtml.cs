﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using eMovies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace eMovies.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and digits.")]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Full name is required"), MaxLength(50, ErrorMessage = "Full name cannot exceed 50 symbols"), MinLength(3, ErrorMessage = "Full name cannot be less than 3 symbols")]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name can only contain letters.")]
            public string FullName { get; set; }

            [MaxLength(10, ErrorMessage = "PhoneNumber cannot exceed 10 digits"), MinLength(10, ErrorMessage = "Phone number cannot be less than 10 digits")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Country is required"), MaxLength(50, ErrorMessage = "Country cannot exceed 50 symbols"), MinLength(3, ErrorMessage = "Country cannot be less than 3 symbols")]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Country can only contain letters.")]
            public string Country { get; set; }

            [Required(ErrorMessage = "City is required"), MaxLength(50, ErrorMessage = "City cannot exceed 50 symbols"), MinLength(3, ErrorMessage = "City cannot be less than 3 symbols")]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "City can only contain letters.")]
            public string City { get; set; }

            [Required(ErrorMessage = "State is required"), MaxLength(50, ErrorMessage = "State cannot exceed 50 symbols"), MinLength(3, ErrorMessage = "State cannot be less than 3 symbols")]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "State can only contain letters.")]
            public string State { get; set; }

            public string Address { get; set; }

            [RegularExpression(@"^\d{4,9}$", ErrorMessage = "Zip code must be between 4 and 9 digits."), MinLength(4, ErrorMessage = "Zip Code cannot be less than 4 digits"), MaxLength(9, ErrorMessage = "Zip Code cannot exceed 10 digits")]
            public string Zip { get; set; }

            [Required(ErrorMessage = "Date of birth is required")]
            public DateTime DateOfBirth { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.FullName = Input.FullName;
                user.PhoneNumber = Input.PhoneNumber;
                user.DateOfBirth = Input.DateOfBirth;
                user.Country = Input.Country;
                user.City = Input.City;
                user.State = Input.State;
                user.Address = Input.Address;
                user.Zip = Input.Zip;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var roleExist = await _roleManager.RoleExistsAsync("User");
                    if (!roleExist)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }

                    await _userManager.AddToRoleAsync(user, "User");

                    TempData["SuccessMessage"] = "You registered successfully. Please log in.";
                    return RedirectToPage("Login");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
