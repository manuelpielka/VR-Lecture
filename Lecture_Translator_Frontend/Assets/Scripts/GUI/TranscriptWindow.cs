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

    public async void AssignLecture(Lecture lecture)
    {
        Task transcriptDownload = transcriptManager.AssignLecture(lecture);

        titleText.text = lecture.GetName();
        languageDropdown.AddOptions(lecture.GetTranscriptLanguages());

        await transcriptDownload;
        transcriptText.text = transcriptManager.GetTranscript(STANDARD_LANGUAGE).getFullText();
    }

    public void LanguageSelected()
    {
        string selectedOption = languageDropdown.options[languageDropdown.value].text;
        transcriptText.text = transcriptManager.GetTranscript(selectedOption).getFullText();
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
