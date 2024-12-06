using UnityEngine;

public class PrefManager : MonoBehaviour
{

    public class PlayerPrefsManager
    {
        private const string LoginKey = "UserLogin";
        private const string PasswordKey = "UserPassword";

        // ��������� ����� � ������
        public static void SaveCredentials(string login, string password)
        {
            PlayerPrefs.SetString(LoginKey, login);
            PlayerPrefs.SetString(PasswordKey, password);
            PlayerPrefs.Save();
        }

        // ��������� �����
        public static string GetLogin()
        {
            return PlayerPrefs.GetString(LoginKey);
        }

        // ��������� ������
        public static string GetPassword()
        {
            return PlayerPrefs.GetString(PasswordKey);
        }

        // ������� ����������� ������
        public static void ClearCredentials()
        {
            PlayerPrefs.DeleteKey(LoginKey);
            PlayerPrefs.DeleteKey(PasswordKey);
            PlayerPrefs.Save();
        }
    }

}
