namespace WebAPI
{
    public class Settings
    {
        public const string AdminPolicyName       = "AdminUser"; 
        public const string UserPolicyName        = "User";
        public const string AdminOrUserPolicyName = "AdminOrUser";
        public const string KeycloakAdminRoleName = "adminRole"; // use "myAdminRole" for realm myTest
        public const string KeycloakUserRoleName  = "userRole";  // use "myUserRole" for realm myTest
    }
}