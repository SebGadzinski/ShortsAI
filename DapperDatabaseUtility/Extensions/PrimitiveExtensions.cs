using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDatabaseUtility.Extensions
{
    public static class PrimitiveExtensions
    {
        public static string ApplyIsActive<T>(this string target, bool isActive)
        {
            var isActiveQueryAddition = "";
            var members = typeof(T).GetMembers();
            if (members.Any(x => x.Name.Equals("status_type_id")))
            {
                var hasWhere = target.ToUpper().Contains("WHERE");
                isActiveQueryAddition = $" {(hasWhere ? "" : "Where ")} {(isActive ? $" {(hasWhere ? " and " : "")} status_type_id != 0" : "")}";
            }
            return target + isActiveQueryAddition;
        }
    }
}
