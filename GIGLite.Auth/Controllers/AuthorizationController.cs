using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using GIGLite.Auth.Helpers;
using GIGLite.Auth.Models;
using GIGLite.Auth.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server;
using static AspNet.Security.OAuth.Validation.OAuthValidationConstants;
using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;
using Errors = AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants.Errors;

namespace GIGLite.Auth.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly OpenIddictScopeManager<OpenIddictScope> _scopeManager;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly GigLiteDbContext _GigLiteDbContext;

        public RoleManager<IdentityRole> _roleManager { get; }

        public AuthorizationController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, OpenIddictScopeManager<OpenIddictScope> scopeManager, GigLiteDbContext GigLiteDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _GigLiteDbContext = GigLiteDbContext;
            _scopeManager = scopeManager;
        }
        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
        {
            if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(request.Username);

                if (user == null)
                {
                    return Forbid(properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "user not found."
                    }), OpenIddictServerDefaults.AuthenticationScheme);
                }
                #region stash2

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return Forbid(properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "invalid username or password."
                    }), OpenIddictServerDefaults.AuthenticationScheme);
                }
                var getUserRole = await _userManager.GetRolesAsync(user);
                // Create a new ClaimsIdentity holding the user identity.
                var identity = new ClaimsIdentity(
                    OpenIddictServerDefaults.AuthenticationScheme,
                    OpenIdConnectConstants.Claims.Name,
                    OpenIdConnectConstants.Claims.Role);

                identity.AddClaim(OpenIdConnectConstants.Claims.Subject, user.Id, OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Name, user.UserName, OpenIdConnectConstants.Destinations.AccessToken);
                foreach (var userRole in getUserRole)
                {
                    identity.AddClaim(OpenIdConnectConstants.Claims.Role, userRole, OpenIdConnectConstants.Destinations.AccessToken);
                }
                var principal = new ClaimsPrincipal(identity);
                return SignIn(principal, OpenIddictServerDefaults.AuthenticationScheme);

                #endregion

            }
            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        // [HttpPost]
        [AllowAnonymous]
        [HttpPost("~/register"), Produces("application/json")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            CreateRoles(_GigLiteDbContext);
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var isAddedToRole = await _userManager.AddToRoleAsync(user, model.Role);
                    if (isAddedToRole.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return Ok();
                    }

                }
                return BadRequest("Unable to register user");
                //AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("model state is invalid");
        }

        [HttpPut]
        [Route("update/{userId}")]
        public async Task<IActionResult> Update(string userId, RegisterViewModel registration)
        {

            var user = await _userManager.FindByIdAsync(userId);


            if (user == null)
            {
                return BadRequest("User not found");

            }

            if (!string.Equals(user.Email, registration.Email, StringComparison.CurrentCultureIgnoreCase))
            {
                var isFound = await _userManager.FindByEmailAsync(registration.Email.ToLower());
                if (isFound == null)
                {
                    return BadRequest("User email not found");

                }
            }
            var _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_GigLiteDbContext), null, null, null, null);

            var roleExist = await _roleManager.RoleExistsAsync(registration.Role);
            if (!roleExist)
            {
                return BadRequest("role does not exist");
            }

            user.Email = registration.FirstName;
            user.LastName = registration.LastName;
            user.PhoneNumber = registration.PhoneNumber;
            //user.UserType = registration.UserType;

            if (!await UpdateUserAsync(user, registration.Role))
            {
                return BadRequest("unable to update user information");
            }

            return Ok(user);
        }
        private async void CreateRoles(GigLiteDbContext context)
        {

            var _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context), null, null, null, null);

            var rolesArray = new string[] { "Admin", "Operation", "ICU" };
            foreach (var role in rolesArray)
            {
                var roleExist_ = _roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();
                if (!roleExist_)
                {
                    var role_ = new IdentityRole();
                    role_.Name = role;
                    _roleManager.CreateAsync(role_).GetAwaiter().GetResult();
                }
            }
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user, string role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());
            }

            await _userManager.AddToRoleAsync(user, role);

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }



    }
}