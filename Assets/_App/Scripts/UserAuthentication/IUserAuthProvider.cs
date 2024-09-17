using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserAuthProvider
{
    public IEnumerator TryAuthenticateUser(string username, string password);
    public bool IsAuthenticated();
    public string TryGetIdToken();
    public string TryGetAccessToken();
    public string TryGetRefreshToken();
}