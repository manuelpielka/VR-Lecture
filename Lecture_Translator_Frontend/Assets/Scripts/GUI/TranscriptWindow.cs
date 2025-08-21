using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TranscriptWindow : Window
{
    [SerializeField] private TextMeshProUGUI transcriptText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TranscriptManager transcriptManager;
    private const string STANDARD_LANGUAGE = "Multilingual";

    public async Task AssignLecture(Lecture lecture)
    {
        Task transcriptDownload = transcriptManager.AssignLecture(lecture);

        titleText.text = lecture.GetName();
        languageDropdown.AddOptions(lecture.GetTranscriptLanguages());

        await transcriptDownload;
        try
        {
            transcriptText.text = transcriptManager.GetTranscript(STANDARD_LANGUAGE).getFullText();
        }
        catch (KeyNotFoundException)
        {
            //Fallback in case standard language does not exist
            //just pick the top of the list
            transcriptText.text = transcriptManager.GetTranscript(lecture.GetTranscriptLanguages()[0]).getFullText();
        }

    }

    public void LanguageSelected()
    {
        string selectedOption = languageDropdown.options[languageDropdown.value].text;
        try
        {
            transcriptText.text = transcriptManager.GetTranscript(selectedOption).getFullText();
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("Selected Language does not exist! This is not supposed to happen!");
        }
    }

    public void SizePlusBtnPressed()
    {
        transcriptText.fontSize++;
        sizeText.text = transcriptText.fontSize.ToString();
    }
    public void SizeMinusBtnPressed()
    {
        transcriptText.fontSize--;
        sizeText.text = transcriptText.fontSize.ToString();
    }

    public void CloseBtnPressed()
    {
        Close();
    }
}
