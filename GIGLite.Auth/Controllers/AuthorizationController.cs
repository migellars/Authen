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
        // private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly GigLiteDbContext _GigLiteDbContext;
        private static bool _databaseChecked;

        public RoleManager<IdentityRole> _roleManager { get; }

        public AuthorizationController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, OpenIddictScopeManager<OpenIddictScope> scopeManager, GigLiteDbContext GigLiteDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            // _roleManager = roleManager;
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

                identity.AddClaim(OpenIdConnectConstants.Claims.Subject,user.Id,OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Name, user.UserName,OpenIdConnectConstants.Destinations.AccessToken);
                foreach (var userRole in getUserRole)
                {
                    identity.AddClaim(OpenIdConnectConstants.Claims.Role, userRole,OpenIdConnectConstants.Destinations.AccessToken);
                }
                // ... add other claims, if necessary.
                var principal = new ClaimsPrincipal(identity);
                // Ask OpenIddict to generate a new token and return an OAuth2 token response.
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
            //EnsureDatabaseCreated(_GigLiteDbContext);
            //ViewData["ReturnUrl"] = returnUrl;
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
            return BadRequest("model state is invald");
        }

        private async void CreateRoles(GigLiteDbContext context)
        {
            //var context = new GigLiteDbContext();

            var _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context), null, null, null, null);
            //var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context), null, null, null, null);

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




    }
}