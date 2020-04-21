using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
//using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using GIGLite.Auth.Helpers;
using GIGLite.Auth.Models;
using GIGLite.Auth.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server;
//using static AspNet.Security.OAuth.Validation.OAuthValidationConstants;
//using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;
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
        private readonly OpenIddictTokenManager<OpenIddictToken> tokenManager;

        public RoleManager<IdentityRole> _roleManager { get; }
        public IOptions<DefaultAdmin> _defaultAdmin { get; }

        public AuthorizationController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, OpenIddictScopeManager<OpenIddictScope> scopeManager, GigLiteDbContext GigLiteDbContext, OpenIddictTokenManager<OpenIddictToken> tokenManager, IOptions<DefaultAdmin> defaultAdmin)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _GigLiteDbContext = GigLiteDbContext;
            this.tokenManager = tokenManager;
            _defaultAdmin = defaultAdmin;
            _scopeManager = scopeManager;
        }
        //[Route("/connect/token")]
        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
        {
            if (request.IsPasswordGrantType())
            {

                var user = await _userManager.FindByNameAsync(request.Username);
                var isDefaultAdmin = await DefaultAdminCheck();
                if (user == null && !isDefaultAdmin)
                {
                    return Forbid(properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "this user was not found."
                    }), OpenIddictServerDefaults.AuthenticationScheme);
                }
                #region stash2
                ApplicationUser defaultUser = new ApplicationUser();
                 var defaultCheck = false;
                if (request.Username == _defaultAdmin.Value.Username)
                {
                    defaultCheck = true;
                    defaultUser = await _userManager.FindByEmailAsync(_defaultAdmin.Value.Username);
                }
                var result = await _signInManager.CheckPasswordSignInAsync(defaultCheck ? defaultUser :  user,request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return Forbid(properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "invalid username or password."
                    }), OpenIddictServerDefaults.AuthenticationScheme);
                }
                var getUserRole = await _userManager.GetRolesAsync(defaultCheck? defaultUser : user);
                // Create a new ClaimsIdentity holding the user identity.
                var identity = new ClaimsIdentity(
                  OpenIddictServerDefaults.AuthenticationScheme,
                  OpenIdConnectConstants.Claims.Name,
                  OpenIdConnectConstants.Claims.Role);

                var employeeDetail = _GigLiteDbContext.Employees.FirstOrDefault(a => a.ApplicationUserId == user.Id);
               

                try
                {
                    identity.AddClaim(OpenIdConnectConstants.Claims.Subject, user.Id, OpenIdConnectConstants.Destinations.AccessToken);
                    identity.AddClaim(OpenIdConnectConstants.Claims.Username, user.UserName, OpenIdConnectConstants.Destinations.AccessToken);
                    identity.AddClaim(OpenIdConnectConstants.Claims.Email, user.Email, OpenIdConnectConstants.Destinations.AccessToken);
                    if (!isDefaultAdmin)
                    {
                        identity.AddClaim(OpenIdConnectConstants.Claims.PhoneNumber, user.PhoneNumber, OpenIdConnectConstants.Destinations.AccessToken);
                    }
                    identity.AddClaim("firstname", user.FirstName, OpenIdConnectConstants.Destinations.AccessToken);
                    identity.AddClaim("lastname", user.LastName, OpenIdConnectConstants.Destinations.AccessToken);
                   

                    if (employeeDetail != null)
                    {
                        identity.AddClaim("position", employeeDetail.PositionName, OpenIdConnectConstants.Destinations.AccessToken);
                        identity.AddClaim("department", employeeDetail.DepartmentName, OpenIdConnectConstants.Destinations.AccessToken);
                        identity.AddClaim("terminal", employeeDetail.TerminalName, OpenIdConnectConstants.Destinations.AccessToken);
                    }

                    foreach (var userRole in getUserRole)
                    {
                        identity.AddClaim(OpenIdConnectConstants.Claims.Role, userRole, OpenIdConnectConstants.Destinations.AccessToken);
                    }
                }
                catch (Exception e)
                {
                    return BadRequest($"an error occured,user claim missing..");
                }

                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, OpenIdConnectServerDefaults.AuthenticationScheme);
                //ticket.SetScopes(OpenIdConnectConstants.Scopes.OfflineAccess);

                //return SignIn(ticket.Principal,ticket.Properties, OpenIddictServerDefaults.AuthenticationScheme);
                return SignIn(principal, OpenIddictServerDefaults.AuthenticationScheme);

                #endregion

            }
            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        #region OldLogout
        //[HttpGet("~/logout")]
        //public IActionResult Logout(OpenIdConnectRequest request)
        //{
        //    // Flow the request_id to allow OpenIddict to restore
        //    // the original logout request from the distributed cache.
        //    return Ok();
        //}
        //[Route("/logout")]
        //[HttpPost("~/connect/logout"),Produces("application/json")]
        //public async Task<IActionResult> LogoutPost(SignoutModel model)
        //{
        //    // Ask ASP.NET Core Identity to delete the local and external cookies created
        //    // when the user agent is redirected from the external identity provider
        //    // after a successful authentication flow (e.g Google or Facebook).
        //    await _signInManager.SignOutAsync();

        //    // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        //    // to the post_logout_redirect_uri specified by the client application or to
        //    // the RedirectUri specified in the authentication properties if none was set.
        //    return SignOut(
        //        authenticationSchemes: OpenIddictServerDefaults.AuthenticationScheme,
        //        properties: new AuthenticationProperties
        //        {
        //            RedirectUri = "/home/index"
        //        });
        //    //SignOut(OpenIddictServerDefaults.AuthenticationScheme);
        //    //return Ok();

        //}

        #endregion


        [HttpPost("~/connect/logout")]
        public async Task<IActionResult> Signout(SignoutModel model)
        {
            //var tokenManager = new OpenIddictTokenManager<OpenIddictToken>();
            var tokens = await tokenManager.FindBySubjectAsync(model.UserId);
            //var tokens_ = await tokenManager.(model.UserId);
            if (tokens.Count() > 0)
            {
                foreach (var token in tokens)
                {
                    if (await tokenManager.IsValidAsync(token))
                    {
                        await tokenManager.RevokeAsync(token);
                    }
                }
                return Ok();
            }
            return BadRequest("user is currently not signed in");
        }

        // [HttpPost]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("~/register"), Produces("application/json")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            CreateRoles(_GigLiteDbContext);

            if (ModelState.IsValid)
            {
                //var user = new ApplicationUser { UserName = model.Email, Email = model.Email,FirstName = model.FirstName,LastName = model.LastName,PhoneNumber = model.PhoneNumber,DateJoined = model.DateJoined, Department = model.Department, Position = model.Position, Terminal = model.Terminal,}; 
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, PhoneNumber = model.PhoneNumber,UserType = model.UserType };
                var employee = new Employee
                {
                    
                    EmployeeCode = model.EmployeeCode,
                    PartnerName = model.PartnerName,
                    DepartmentName = model.DepartmentName,
                    PositionName = model.PositionName,
                    TerminalName = model.TerminalName,
                    DateJoined = model.DateJoined,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    Gender = model.Gender,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    NextOfKin = model.NextOfKin,
                    NextOfKinPhone = model.NextOfKinPhone
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    foreach (var role in model.Roles)
                    {
                        var isAddedToRole = await _userManager.AddToRoleAsync(user, role);
                        if (isAddedToRole.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                          
                        }
                    }
                    var getUser = await _userManager.FindByEmailAsync(model.Email);
                    var userId = "";
                    if (getUser != null)
                    {
                       
                        employee.ApplicationUserId = userId = getUser.Id;
                    }

                    _GigLiteDbContext.Employees.Add(employee);
                    _GigLiteDbContext.SaveChanges();
                    return Ok();
                }
                return BadRequest("Unable to register user,please try using a different email.");
                //AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("model state is invalid");
        }

        [Authorize]
        [HttpPut("~/update/{userId}")]
        //[Route("update/{userId}")]
        public async Task<IActionResult> Update(string userId, RegisterViewModel registration)
        {

            var user = await _userManager.FindByIdAsync(userId);


            if (user == null)
            {
                return BadRequest("User not found");

            }
           

            if (!string.Equals(user.Email, registration.Email, StringComparison.CurrentCultureIgnoreCase))
            {
                var isFound = await _userManager.FindByNameAsync(user.UserName.ToLower());
                if (isFound == null)
                {
                    return BadRequest("User email not found");

                }
                user.Email = registration.Email;
                user.FirstName = registration.FirstName;
                user.LastName = registration.LastName;
                user.PhoneNumber = registration.PhoneNumber;
                user.UserName = registration.Email;

                await _userManager.UpdateAsync(user);
            }
            else
            {
                user.Email = registration.Email;
                user.FirstName = registration.FirstName;
                user.LastName = registration.LastName;
                user.PhoneNumber = registration.PhoneNumber;
                user.UserName = registration.Email;

                await _userManager.UpdateAsync(user);
            }
            var _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_GigLiteDbContext), null, null, null, null);

            foreach (var role in registration.Roles)
            {
                var roleExist = await _roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    return BadRequest("role does not exist");
                }
              
                //user.UserType = registration.UserType;

                if (!await UpdateUserRoleAsync(user, role))
                {
                    return BadRequest("unable to update user role.");
                }
            }
          

           

            return Ok();
        }
        [NonAction]
        private void CreateRoles(GigLiteDbContext context)
        {

            var _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context), null, null, null, null);

            //var rolesArray = new string[] { "Admin", "Operation", "ICU" };

            var rolesList = "Administrator Auditor AuditSupervisor CustomerRepresentative CustomerRepSupervisor FleetManager GroupAccountant HumanResource IntelligentControlUnit InventoryAdmin InventoryOfficer InventorySupervisor OnlineBookingRepresentative Partner PatrolTeamMember ReportViewer TerminalAccountant OperationsManager TerminalOfficer Workshop WorkshopSupervisor WorkshopAdministrator Ticketer OnlineBookingManager RegionalManager EnterpriseBusinessManager Marketing_&_Innovation GLA IT_Support ThirdPartyManager LineManager OperationHead Captain_Relation_Officer Captain_Relation_Manager PartnerRelationsManager";

            var rolesArray = rolesList.Split(" ");
            foreach (var role in rolesArray)
            {
                var roleExist_ =  _roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();
                if (!roleExist_)
                {
                    var role_ = new IdentityRole();
                    role_.Name = role;
                    _roleManager.CreateAsync(role_).GetAwaiter().GetResult();
                }
            }
        }
        [NonAction]
        public async Task<bool> UpdateUserRoleAsync(ApplicationUser user, string role)
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

        [NonAction]
        public async Task<bool> DefaultAdminCheck()
        {
            var _email = _defaultAdmin.Value.Username;
            var _password = _defaultAdmin.Value.Password;
            var role = _defaultAdmin.Value.Role;

            var user = await  _userManager.FindByEmailAsync(_email);
            if (user == null)
            {
                var defaultUser = new ApplicationUser { Email = _email,UserName = _email, FirstName = "default",LastName = "admin" };
                var result = await _userManager.CreateAsync(defaultUser, _password);
                if (result.Succeeded)
                {
                    CreateRoles(_GigLiteDbContext);
                    var isAddedToRole = await _userManager.AddToRoleAsync(defaultUser, role);
                    if (isAddedToRole.Succeeded)
                    {
                        await _signInManager.SignInAsync(defaultUser, isPersistent: false);
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return true;
        }
       




    }
}