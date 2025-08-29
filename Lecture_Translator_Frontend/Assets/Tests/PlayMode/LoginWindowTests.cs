using UnityEngine;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.TestTools;
using System.Threading.Tasks;

/// <summary>
/// This class is used for testing the login functionality.
/// </summary>
public class LoginWindowTests
{
    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Library Hall");
    }

    /// <summary>
    /// This test tests if the error panel shows up when entering an invalid token.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator WrongToken_Test()
    {
        var loginWindow = GameObject.Find("LoginWindow").GetComponent<LoginWindow>();

        var apiClient = new FakeBrowsingApiClient();

        Login.apiclient = apiClient;

        yield return null;

        var inputfield = loginWindow.transform.Find("Canvas/Panel/TokenInput").GetComponent<TMPro.TMP_InputField>();

        yield return null;

        inputfield.text = "invalidToken";

        yield return null;

        inputfield.onEndEdit.Invoke(inputfield.text);

        yield return null;

        var errorPanel = loginWindow.transform.Find("Canvas/ErrorPanel");

        Assert.IsTrue(errorPanel.gameObject.activeSelf);
    }

    /// <summary>
    /// This test tests if the login window closes when entering the right token.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator CorrectToken_Test()
    {
        var loginWindow = GameObject.Find("LoginWindow").GetComponent<LoginWindow>();

        var apiClient = new FakeBrowsingApiClient();

        Login.apiclient = apiClient;

        yield return null;

        var inputfield = loginWindow.transform.Find("Canvas/Panel/TokenInput").GetComponent<TMPro.TMP_InputField>();

        yield return null;

        inputfield.text = "Correct|token|username";

        yield return null;

        inputfield.onEndEdit.Invoke(inputfield.text);

        yield return null;

        Assert.IsTrue(loginWindow == null);
    }
}

/// <summary>
/// This class is used to catch post request sent from the Login class for testing.
/// </summary>
public class FakeLoginApiClient : IApiClient
{
    public Task<string> PostRequest(string url, string json)
    {
        if (Login.token == "invalidToken")
        {
            return Task.FromResult("Not authorized\n");
        }

        if (Login.token == "Correct|token|username")
        {
            return Task.FromResult("Correct token");
        }

        return Task.FromResult("");
    }
}
