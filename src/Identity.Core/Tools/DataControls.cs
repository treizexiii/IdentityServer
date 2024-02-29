using System.Text.RegularExpressions;
using Identity.Core.Configuration;

namespace Identity.Core.Tools;

public class DataControls(IAuthOptions authOptions)
{
    private readonly List<string> _problemsMessage = [];


    public string[]? Execute()
    {
        if (_problemsMessage.Count > 0)
        {
            return _problemsMessage.ToArray();
        }

        return null;
    }

    public DataControls ControlPassword(string password)
    {
        IsPasswordValid(password);
        return this;
    }

    public DataControls ControlEmail(string email)
    {
        IsEmailValid(email);
        return this;
    }

    public DataControls ControlAppName(string appName)
    {
        IsAppNameValid(appName);
        return this;
    }

    private void IsPasswordValid(string password)
    {
        var regex = new Regex(authOptions.PasswordRegex);
        var result = regex.IsMatch(password);
        if (!result) _problemsMessage.Add("Password is not valid");
    }

    private void IsEmailValid(string email)
    {
        var regex = new Regex(authOptions.EmailRegex);
        var result = regex.IsMatch(email);
        if (!result) _problemsMessage.Add("Email is not valid");
    }

    private void IsAppNameValid(string appName)
    {
        var regex = new Regex(authOptions.AppNameRegex);
        var result = regex.IsMatch(appName);
        if (!result) _problemsMessage.Add("App name is not valid");
    }
}
