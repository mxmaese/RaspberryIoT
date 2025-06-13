using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Databases;
using Services.GeneralFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using static Services.Users.Users;
using Services.Web.Cookies;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Web.Cookies;
using Services.Traductions;


namespace Services.Users;

public class Users : IUsers
{
    private readonly ILogger<Users> _logger;
    private readonly IDatabasesActions _databasesActions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthCookiesManager _authCookiesManager;
    private readonly ITraductionManager _traductionManager;

    public Users(ILogger<Users> logger, IDatabasesActions databasesActions, IHttpContextAccessor httpContextAccessor, IAuthCookiesManager authCookiesManager, ITraductionManager traductionManager)
    {
        _logger = logger;
        _databasesActions = databasesActions;
        _httpContextAccessor = httpContextAccessor;
        _authCookiesManager = authCookiesManager;
        _traductionManager = traductionManager;
    }


    public async Task<List<(RegisterErrorMessages Error, string message)>> CreateUser(IUsers.RegisterFormModel NewUser)
    {
        var checkUser = CheckRegisterUser(NewUser);
        if (checkUser.Count != 0)
        {
            //implementar el sistema de errores
            List<(RegisterErrorMessages error, string message)> output = new();
            foreach (var item in checkUser)
            {
                var message = _traductionManager.GetTraduction(RegisterErrorToTraductionReference[item]);
                output.Add((item, message));
            }
            return output;
        }
        var user = _databasesActions.CreateUser(new Entities.User()
        {
            UserName = NewUser.RegisteUsername,
            Email = NewUser.RegisterEmail,
            Password = NewUser.RegisterPassword,
            ApiToken = _databasesActions.GenerateUniqueUserApiToken(),
            CreatedAt = DateTime.Now
        });

        await _authCookiesManager.SetLoginCookie(user.UserName, user.UserId, true);
        return default;
    }

    public List<RegisterErrorMessages> CheckRegisterUser(IUsers.RegisterFormModel RegisterUser)
    {
        var output = new List<RegisterErrorMessages>();
        if (RegisterUser.RegisteUsername.IsNullOrEmpty()) output.Add(RegisterErrorMessages.NotInsertedUserName);
        if (RegisterUser.RegisterEmail.IsNullOrEmpty()) output.Add(RegisterErrorMessages.NotInsertedEmail);
        if (RegisterUser.RegisterPassword.IsNullOrEmpty()) output.Add(RegisterErrorMessages.NotInsertedPassword);
        if (RegisterUser.RegisterPasswordConfirm.IsNullOrEmpty()) output.Add(RegisterErrorMessages.NotInsertedPasswordConfirm);
        if (RegisterUser.RegisterPassword != RegisterUser.RegisterPasswordConfirm) output.Add(RegisterErrorMessages.PasswordsDoNotMatch);

        bool EmailAvailable = _databasesActions.GetUser(new User() { Email = RegisterUser.RegisterEmail }) == null;
        bool UserNameAvailable = _databasesActions.GetUser(new User() { UserName = RegisterUser.RegisteUsername }) == null;
        if (!EmailAvailable) output.Add(RegisterErrorMessages.EmailAlreadyExists);
        if (!UserNameAvailable) output.Add(RegisterErrorMessages.UserAlreadyExists);

        return output;
    }

    public enum RegisterErrorMessages
    {
        NotInsertedUserName,
        UserAlreadyExists,

        NotInsertedEmail,
        EmailAlreadyExists,

        NotInsertedPassword,
        NotInsertedPasswordConfirm,
        PasswordsDoNotMatch
    }
    private Dictionary<RegisterErrorMessages, string> RegisterErrorToTraductionReference = new Dictionary<RegisterErrorMessages, string>() {
        {RegisterErrorMessages.NotInsertedUserName, "web.auth.register.notinsertedusername" },// "NotInsertedUserName"},
        {RegisterErrorMessages.UserAlreadyExists, "web.auth.register.useralreadyexists"},//"UserAlreadyExists" },
        {RegisterErrorMessages.NotInsertedEmail, "web.auth.register.notinsertedemail" },//"NotInsertedEmail" },
        {RegisterErrorMessages.EmailAlreadyExists, "web.auth.register.emailalredyexists" },//"EmailAlreadyExists" },
        {RegisterErrorMessages.NotInsertedPassword, "web.auth.register.notinsertedpassword" },//"NotInsertedPassword" },
        {RegisterErrorMessages.NotInsertedPasswordConfirm, "web.auth.register.notinsertedpasswordconfirm" },//"NotInsertedPasswordConfirm" },
        {RegisterErrorMessages.PasswordsDoNotMatch, "web.auth.register.passwordsdonotmatch" },//"PasswordsDoNotMatch" } 
    };

    public async Task<List<(LoginErrorMessages Error, string message)>> LoginUser(IUsers.LoginFormModel LoginForm)
    {
        var Response = CheckLoginUser(LoginForm);
        if (Response != default)
        {
            var output = new List<(LoginErrorMessages Error, string message)>();
            foreach (var item in Response)
            {
                var message = _traductionManager.GetTraduction(LoginErrorToTraductionReference[item]);
                output.Add((item, message));
            }
            return output;
        }
        var user = _databasesActions.GetUser(new User() { UserName = LoginForm.LoginUsername });
        await _authCookiesManager.SetLoginCookie(user.UserName, user.UserId, true);
        return default;
    }

    public List<LoginErrorMessages> CheckLoginUser(IUsers.LoginFormModel LoginForm)
    {
        var output = new List<LoginErrorMessages>();
        if (LoginForm.LoginUsername.IsNullOrEmpty()) output.Add(LoginErrorMessages.NotInsertedUserName);
        if (LoginForm.LoginPassword.IsNullOrEmpty()) output.Add(LoginErrorMessages.NotInsertedPassword);

        var user = _databasesActions.GetUser(new User() { UserName = LoginForm.LoginUsername });
        if (user == null && !LoginForm.LoginUsername.IsNullOrEmpty()) output.Add(LoginErrorMessages.UserDoesNotExist);
        if (user?.Password != LoginForm.LoginPassword && !LoginForm.LoginPassword.IsNullOrEmpty() && user != null) output.Add(LoginErrorMessages.PasswordIsIncorrect);

        return output.IsNullOrEmpty() ? default : output;
    }

    public enum LoginErrorMessages
    {
        NotInsertedUserName,
        UserDoesNotExist,

        NotInsertedPassword,
        PasswordIsIncorrect
    }
    private Dictionary<LoginErrorMessages, string> LoginErrorToTraductionReference = new Dictionary<LoginErrorMessages, string>() {
        {LoginErrorMessages.NotInsertedUserName, "web.auth.login.notinsertedusername" },// "NotInsertedUserName"},
        {LoginErrorMessages.UserDoesNotExist, "web.auth.login.userdoesnotexist"},//"UserDoesNotExist" },

        {LoginErrorMessages.NotInsertedPassword, "web.auth.login.notinsertedpassword" },//"NotInsertedPassword" },
        {LoginErrorMessages.PasswordIsIncorrect, "web.auth.login.passwordisincorrect" },//"PasswordIsIncorrect" }
    };

    public User? GetUserByApiToken(string apiToken)
    {
        return _databasesActions.GetUserByApiToken(apiToken);
    }

    public User? ValidateUserCredentials(string username, string password)
    {
        var user = _databasesActions.GetUser(new User() { UserName = username });
        if (user == null) return null;
        if (user.Password != password) return null;
        return user;
    }
}
public interface IUsers
{
    public Task<List<(RegisterErrorMessages Error, string message)>> CreateUser(IUsers.RegisterFormModel NewUser);
    List<RegisterErrorMessages> CheckRegisterUser(IUsers.RegisterFormModel RegisterUser);

    public Task<List<(LoginErrorMessages Error, string message)>> LoginUser(IUsers.LoginFormModel LoginForm);
    List<LoginErrorMessages> CheckLoginUser(IUsers.LoginFormModel LoginForm);
    User? GetUserByApiToken(string apiToken);
    User? ValidateUserCredentials(string username, string password);

    public class RegisterFormModel
    {
        public string RegisteUsername { get; set; }
        public string RegisterEmail { get; set; }
        public string RegisterPassword { get; set; }
        public string RegisterPasswordConfirm { get; set; }
    }
    public class LoginFormModel
    {
        public string LoginUsername { get; set; }
        public string LoginPassword { get; set; }
    }
}
