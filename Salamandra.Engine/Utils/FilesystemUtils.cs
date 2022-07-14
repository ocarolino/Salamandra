using Salamandra.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Utils
{
    public static class FilesystemUtils
    {
        // Based on: https://stackoverflow.com/questions/60703387/check-file-permission-in-c-sharp-net-core (2022-07-14)
        public static bool CheckWritePermissions(string directory)
        {
            try
            {
                var dInfo = new DirectoryInfo(directory.EnsureHasDirectorySeparatorChar());

                DirectorySecurity dSecurity = dInfo.GetAccessControl();

                SecurityIdentifier usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                FileSystemRights fileRights = FileSystemRights.FullControl | FileSystemRights.Write | FileSystemRights.Modify;

                var rules = dSecurity.GetAccessRules(true, true, usersSid.GetType()).OfType<FileSystemAccessRule>().ToList();
                var hasRights = rules.Where(r => r.FileSystemRights == fileRights).Any();

                return hasRights;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
