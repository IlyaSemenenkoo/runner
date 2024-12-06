using UnityEngine;

public class PrefManager : MonoBehaviour
{

    public class PlayerPrefsManager
    {
        private const string LoginKey = "UserLogin";
        private const string PasswordKey = "UserPassword";

        // Сохранить логин и пароль
        public static void SaveCredentials(string login, string password)
        {
            PlayerPrefs.SetString(LoginKey, login);
            PlayerPrefs.SetString(PasswordKey, password);
            PlayerPrefs.Save();
        }

        // Загрузить логин
        public static string GetLogin()
        {
            return PlayerPrefs.GetString(LoginKey);
        }

        // Загрузить пароль
        public static string GetPassword()
        {
            return PlayerPrefs.GetString(PasswordKey);
        }

        // Удалить сохраненные данные
        public static void ClearCredentials()
        {
            PlayerPrefs.DeleteKey(LoginKey);
            PlayerPrefs.DeleteKey(PasswordKey);
            PlayerPrefs.Save();
        }
    }

}
