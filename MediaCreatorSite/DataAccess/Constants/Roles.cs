using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.DataAccess.Constants
{
    public static class Roles
    {
        public const string SUPERADMIN = "superadmin";
        public const string ADMIN = "admin";
        public const string SHOPPER = "shopper";

        private static Dictionary<string, string> AUTHORIZATION_ROLES = new Dictionary<string, string>()
        {

            {"SUPERADMIN", SUPERADMIN},
            {"ADMIN", ADMIN},
            {"SHOPPER", SHOPPER},
        };

        public const string AuthorizeSuperAdmin = $"{SUPERADMIN}";
        public const string AuthorizeAdmin = $"{ADMIN},{SUPERADMIN}";
        public const string AuthorizeShopper = $"{SHOPPER},{ADMIN},{SUPERADMIN}";

        public static readonly HashSet<string> AuthorizeSuperAdminRoles = new HashSet<string>() { SUPERADMIN };
        public static readonly HashSet<string> AuthorizeAdminRoles = new HashSet<string>() { ADMIN, SUPERADMIN };
        public static readonly HashSet<string> AuthorizeShopperRoles = new HashSet<string>() { SHOPPER, ADMIN, SUPERADMIN };

        public static Dictionary<string, string> GetAuthroizationRoles() => AUTHORIZATION_ROLES;
    }
}
