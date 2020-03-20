using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GIGLite.Auth.Helpers
{
    public static class OpenIddictServerAspNetCoreConstants
    {
        public static class Cache
        {
            public const string AuthorizationRequest = "openiddict-authorization-request:";
            public const string LogoutRequest = "openiddict-logout-request:";
        }

        public static class JsonWebTokenTypes
        {
            public const string AuthorizationRequest = "oi_auth_req";
            public const string LogoutRequest = "oi_lgt_req";
        }

        public static class Properties
        {
            public const string Error = ".error";
            public const string ErrorDescription = ".error_description";
            public const string ErrorUri = ".error_uri";
            public const string Realm = ".realm";
            public const string Scope = ".scope";
        }

    }
}
