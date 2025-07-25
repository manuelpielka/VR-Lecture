using UnityEngine;
using TMPro;

/// <summary>
/// This window handles logging in via a token.
/// </summary>
public class LoginWindow : Window
{
    /// <summary>
    /// The text box of the error text.
    /// </summary>
    [SerializeField] private TextMeshProUGUI errorTextBox;

    /// <summary>
    /// The input field where the user inputs the token.
    /// </summary>
    [SerializeField] private TMP_InputField inputField;

    /// <summary>
    /// Handler for the input field.
    /// </summary>
    /// <param name="token"> The token the user entered. </param>
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

    /// <summary>
    /// Pastes the clipboard into the inputfield.
    /// </summary>
    public void PasteFromClipboard()
    {
        inputField.text = GUIUtility.systemCopyBuffer;
    }
}
