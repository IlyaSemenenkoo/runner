using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;


public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;
    private FirebaseUser User;
    private FirestoreManager firestoreManager;

    [Header("Login")]
    [SerializeField] private TMP_InputField loginLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;
    [SerializeField] private TMP_Text warmingLoginText;
    [SerializeField] private TMP_Text confirmLoginText;

    [Header("Register")]
    [SerializeField] private TMP_InputField loginRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterVerifyField;
    [SerializeField] private TMP_Text warningRegisterText;

    private void Start()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
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
            SceneManager.LoadScene("GameScene");
        }
    }

    public void LoginButton()
    {
        string email = firestoreManager.GetUserByLogin(loginLoginField.text).Result;
        StartCoroutine(Login(email, passwordLoginField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, loginRegisterField.text));
    }

    private IEnumerator Login(string email, string password)
    {
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
            warmingLoginText.text = message;
        }
        else
        {
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warmingLoginText.text = "";
            confirmLoginText.text = "Logged In";
            Registration.instance.AuthComplite();
            StartCoroutine(firestoreManager.LoadUserData(User.UserId));
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Password Does Not Match!";
        }
        else if (_email == "")
        {
            warningRegisterText.text = "Missing Email";
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
                warningRegisterText.text = message;
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
                        warningRegisterText.text = "Username Set Failde!";
                    }
                    else
                    {
                        warningRegisterText.text = "";
                    }
                    Registration.instance.AuthComplite();
                    StartCoroutine(firestoreManager.SaveUserData(User.UserId, _email, _username, 0));
                }
            }
        }
    }
}