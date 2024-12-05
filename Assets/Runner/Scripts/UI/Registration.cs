using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Registration : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _login;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _confirmPassword;
    [SerializeField] private TMP_InputField _name;
    [SerializeField] private Button _register;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Toggle _authType;
    [SerializeField] private TextMeshProUGUI _errorText;
    private bool _status = false;
    [SerializeField] private FirebaseAuthManager _auth;
    [SerializeField] private UIManager _canvas;

    private void Awake()
    {
        _auth.Logout();
        _register.gameObject.SetActive(true);
        _loginButton.gameObject.SetActive(false); 
    }

    private void Start()
    {
        _auth.AuthError += OnAuthError;
        _auth.AuthSuccess += OnAuthSuccess;
    }

    private void OnAuthError(string error)
    {
       Debug.Log("error");
        _errorText.text = error;
        if (_status)
        {
            _login.text = "";
            _password.text = "";
        }
        else
        {
            _login.text = "";
            _password.text = "";
            _email.text = "";
            _confirmPassword.text = "";
            _name.text = "";
        }
    }

    private void OnAuthSuccess(string success)
    {
        Debug.Log("success");
        _canvas.AuthComplite();
    }

    private void Update()
    {
        if(_authType.isOn != _status)
        {
            ChangeStatus(_authType.isOn);
        }
    }

    public void ChangeStatus(bool value)
    {
        if (value)
        {
            _email.gameObject.SetActive(false);
            _loginButton.gameObject.SetActive(true);
            _register.gameObject.SetActive(false);
            _confirmPassword.gameObject.SetActive(false);
            _name.gameObject.SetActive(false);
        }
        else
        {
            _email.gameObject.SetActive(true);
            _loginButton.gameObject.SetActive(false);
            _register.gameObject.SetActive(true);
            _confirmPassword.gameObject.SetActive(true);
            _name.gameObject.SetActive(true);
        }

        _status = value;
    }

    public void RegisterButtonTouch()
    {
        _auth.RegisterUser(_email.text, _password.text, _confirmPassword.text, _login.text, _name.text);
    }

    public void LoginButtonTouch()
    {
        _auth.LoginUserWithUsername(_login.text, _password.text);
    }

    private void OnDisable()
    {
        _auth.AuthError -= OnAuthError;
        _auth.AuthSuccess -= OnAuthSuccess;
    }

}
