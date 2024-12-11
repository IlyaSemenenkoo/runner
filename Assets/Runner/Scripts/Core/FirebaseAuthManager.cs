using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;
    private FirebaseUser User;
    private FirestoreManager firestoreManager;
    public static FirebaseAuthManager instance;
    private Regex _emailPattern = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", RegexOptions.IgnoreCase);
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        CreateInstance();
    }
    private void Start()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private void CreateInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(predicate:() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializationFirebase();
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoLogin());
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencis: " + dependencyStatus);
        }
    }

    private void InitializationFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        firestoreManager = GetComponent<FirestoreManager>();

        auth.StateChanged += AuthStateChange;
        AuthStateChange(this, null);
    }

    void AuthStateChange(object sender, System.EventArgs eventArgs)
    {
        if(auth.CurrentUser != User)
        {
            bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null;

            if(!signedIn && User != null)
            {
                Debug.Log("Signed out " + User.UserId);
                Registration.instance.Back();
            }

            User = auth.CurrentUser;

            if(signedIn)
            {
                Debug.Log("Signed in" + User.UserId);
            }
        }
    }

    private IEnumerator CheckForAutoLogin()
    {
        if(User != null)
        {
            var reloadUser = User.ReloadAsync();

            yield return new WaitUntil(() => reloadUser.IsCompleted);

            AutoLogin();
        }
        else
        {
            Registration.instance.Back();
        }
    }

    private void AutoLogin()
    {
        if(User != null)
        {
            //this.userName = User.DisplayName;
            Registration.instance.AuthComplite();
        }
        else
        {
            Registration.instance.Back();
        }
    }

    public void Logout()
    {
        if(auth != null && User != null)
        {
            auth.SignOut();
            SceneManager.LoadScene("LoginScene");
        }
    }

    public void LoginButton(string login, string password)
    {
        
        StartCoroutine(Login(login, password));
    }

    public void RegisterButton(string email, string password, string login, string passwordVerify)
    {
        StartCoroutine(Register(email, password, login, passwordVerify));
    }

    private IEnumerator Login(string login, string password)
    {
        var getEmailTask = firestoreManager.GetUserByLoginAsync(login);

        yield return new WaitUntil(predicate: () => getEmailTask.IsCompleted);
        
        string email = getEmailTask.Result;

        var LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        

        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if(LoginTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failde to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch(errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Login";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Login";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            Registration.instance.WarmingLogin(message);
        }
        else
        {
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            Registration.instance.WarmingLogin("");
            Registration.instance.ConfirmLogin("Logged In");
            Registration.instance.AuthComplite();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username, string _passwordVerify)
    {
        var getEmailTask = firestoreManager.GetUserByLoginAsync(_username);

        yield return new WaitUntil(predicate: () => getEmailTask.IsCompleted);

        if (_username == "")
        {
            Registration.instance.WarmingReg("Missing Username");
        }
        else if(_username.Length > 10)
        {
            Registration.instance.WarmingReg("Login too long");
        }
        else if (_password != _passwordVerify)
        {
            Registration.instance.WarmingReg("Password Does Not Match!");
        }
        else if (_email == "")
        {
            Registration.instance.WarmingReg("Missing Email");
        }
        else if(!_emailPattern.IsMatch(_email))
        {
            Registration.instance.WarmingReg("Invalid Email");
        }
        else if (getEmailTask.Result != null)
        {
            Registration.instance.WarmingReg("Login Already In Use");
        }
        else
        {
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failde to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                Registration.instance.WarmingReg(message);
            }
            else
            {
                User = RegisterTask.Result.User;

                if (User != null)
                {
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    var ProfileTask = User.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failde to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        Registration.instance.WarmingReg("Username Set Failde!");
                    }
                    else
                    {
                        Registration.instance.WarmingReg("");
                    }
                    Registration.instance.AuthComplite();
                    StartCoroutine(firestoreManager.SaveUserData(User.UserId, _email, _username, 0));
                }
            }
        }
    }
}