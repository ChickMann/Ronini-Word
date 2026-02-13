namespace EF.Generic
{
    public class AuthResult
    {
        public bool isSuccessful;
        public string userId;

        public AuthResult(bool success, string userUserId)
        {
            isSuccessful = success;
            userId = userUserId;
        }
    }
}