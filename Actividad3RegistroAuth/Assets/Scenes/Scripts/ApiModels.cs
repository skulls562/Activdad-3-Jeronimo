using System;
using System.Collections.Generic;

[Serializable]
public class RegisterRequest
{
    public string username;
    public string password;
}

[Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[Serializable]
public class UpdateRequest
{
    public string username;
    public UserData data;
}

[Serializable]
public class UserData
{
    public int score;
}

[Serializable]
public class LoginResponse
{
    public UserResponse usuario;
    public string token;
}

[Serializable]
public class RegisterResponse
{
    public UserResponse usuario;
}

[Serializable]
public class ProfileResponse
{
    public List<UserResponse> usuarios;
}

[Serializable]
public class UsersListResponse
{
    public List<UserResponse> usuarios;
}

[Serializable]
public class UserResponse
{
    public string _id;
    public string username;
    public string password;
    public bool estado;
    public UserData data;
}