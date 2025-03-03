using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

[Serializable]
public class User
{
    public string firstName;
    public string lastName;
    public string nickname;
    public string email;
    public string password;
}

[Serializable]
public class UserDatabase
{
    public List<User> users = new List<User>();

    private static string filePath = Application.persistentDataPath + "/users.json";

    public static UserDatabase LoadUsers()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<UserDatabase>(json);
        }
        return new UserDatabase();
    }
    public void SaveUsers()
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(filePath, json);
    }
    public bool UserExists(string email)
    {
        return users.Exists(user => user.email == email);
    }
    public bool ValidateUser(string email, string password)
    {
        return users.Exists(user => user.email == email && user.password == password);
    }

    public void AddUser(User newUser)
    {
        users.Add(newUser);
        SaveUsers();
    }
}
