using MediaCreatorFunctions.DataAccess.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.DataAccess.Constants
{
    public static class Claims
    {
        public static List<AppClaim> GetAcceptableClaims() => new List<AppClaim>() {
            new AppClaim()
            {
                name = "FIRST_NAME",
                created_date = DateTime.UtcNow,
                modified_date = DateTime.UtcNow,
                modified_by = "Seeding"
            },
            new AppClaim()
            {
                name = "LAST_NAME",
                created_date = DateTime.UtcNow,
                modified_date = DateTime.UtcNow,
                modified_by = "Seeding"
            },
            new AppClaim()
            {
                name = "BILLING-ADDRESS",
                created_date = DateTime.UtcNow,
                modified_date = DateTime.UtcNow,
                modified_by = "Seeding"
            }
        };
    }
}
