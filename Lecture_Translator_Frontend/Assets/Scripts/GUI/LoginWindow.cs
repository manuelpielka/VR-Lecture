using UnityEngine;
using TMPro;

public class LoginWindow : Window
{
    [SerializeField] private TextMeshProUGUI errorTextBox;

    public async void OnTokenEntered(string token)
    {
        bool validToken = await Login.SetToken(token);

        if (validToken)
        {
            errorTextBox.gameObject.SetActive(false);
            Close();
        }
        else
        {
            errorTextBox.gameObject.SetActive(true);
        }
    }
}
