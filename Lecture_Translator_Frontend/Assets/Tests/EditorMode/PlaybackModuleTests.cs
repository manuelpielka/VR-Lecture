using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlaybackModuleTests
{
    private const string VALID_INPUT_STRING =
    "WEBVTT\n\n0:00:08.448 --> 0:00:13.590\nHello everyone, I am Shobhit Kundu, a PhD\nstudent from the University of Southern California.\n\n0:00:13.535 --> 0:00:18.549\nToday I am going to present our accepted work\nat NeurIPS, analyzing the confidentiality of\n\n0:00:18.549 --> 0:00:21.304\nundistillable teachers in knowledge distillation.\n\n0:00:21.249 --> 0:00:23.289\nThis work was jointly done with M.S.";
    private const string EXPECTED_FULL_TEXT = "Hello everyone, I am Shobhit Kundu, a PhD student from the University of Southern California. Today I am going to present our accepted work at NeurIPS, analyzing the confidentiality of undistillable teachers in knowledge distillation. This work was jointly done with M.S.";

    private const string INVALID_INPUT_STRING = "WEBVTT";

    [Test]
    public void FullTextTest()
    {
        Transcript testTranscript = new Transcript(VALID_INPUT_STRING);
        Assert.IsTrue(testTranscript.getFullText() == EXPECTED_FULL_TEXT);
    }

    [Test]
    public void GetLineTest()
    {
        Transcript testTranscript = new Transcript(VALID_INPUT_STRING);
        Assert.IsTrue(testTranscript.getLine(9f) == "Hello everyone, I am Shobhit Kundu, a PhD student from the University of Southern California.", "Line 1 Correct");
        Assert.IsTrue(testTranscript.getLine(18f) == "Today I am going to present our accepted work at NeurIPS, analyzing the confidentiality of", "Line 2 Correct");
        Assert.IsTrue(testTranscript.getLine(20f) == "undistillable teachers in knowledge distillation.", "Line 3 Correct");
        Assert.IsTrue(testTranscript.getLine(21.3f) == "This work was jointly done with M.S.", "Line 4 Correct");
    }

    [Test]
    public void InvalidTranscriptTest()
    {
        Transcript testTranscript = new Transcript(INVALID_INPUT_STRING);
        Assert.AreEqual(testTranscript.getFullText(), "Unavailable");
        Assert.AreEqual(testTranscript.getLine(60), "Unavailable");
    }

    [Test]
    public async Task PlaybackManagerAssigningLectureTest()
    {
        List<string> example_languages = new List<string>(["Multilingual", "Chinese", "English", "German", "Spanish"]);
        Lecture example = new Lecture("offline_test", "Test/Other/offline_test.mp4", "Test/Other/offline_test", example_languages);
        example.SetDownloaded(true);
        PlaybackManager test = new PlaybackManager();
        await test.AssignLecture(example);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator PlaybackManagerTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}