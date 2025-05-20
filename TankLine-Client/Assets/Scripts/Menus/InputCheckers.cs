using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class InputCheckers : MonoBehaviour 
{
    public static bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    public static bool IsValidPassword(string password)
    {
        if (password.Length < 8)
        {
            return false;
        }
        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        return Regex.IsMatch(password, pattern);
    }

    public static bool IsValidDate(string day, string month, string year)
    {
        int dayInt, monthInt, yearInt;
        if (int.TryParse(day, out dayInt) && int.TryParse(month, out monthInt) && int.TryParse(year, out yearInt))
        {
            try
            {
                DateTime date = new DateTime(yearInt, monthInt, dayInt);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public static bool IsValidAge(string day, string month, string year)
    {
        int dayInt, monthInt, yearInt;
        if (int.TryParse(day, out dayInt) && int.TryParse(month, out monthInt) && int.TryParse(year, out yearInt))
        {
            try
            {
                DateTime birthDate = new DateTime(yearInt, monthInt, dayInt);
                DateTime today = DateTime.Today;
                int age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;
                return age >= 13;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }
}