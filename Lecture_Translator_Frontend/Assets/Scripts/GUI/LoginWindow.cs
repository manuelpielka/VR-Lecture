using UnityEngine;
using TMPro;

public class LoginWindow : Window
{
    [SerializeField] private TextMeshProUGUI errorTextBox;
    [SerializeField] private TMP_InputField inputField;

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

    public void PasteFromClipboard()
    {
        inputField.text = GUIUtility.systemCopyBuffer;
    }
}
