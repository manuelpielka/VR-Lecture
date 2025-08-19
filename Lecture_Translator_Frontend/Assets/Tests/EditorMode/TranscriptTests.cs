using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Linq;

public class TranscriptTests
{
    private const string VALID_INPUT_STRING =
    "WEBVTT\n\n0:00:08.448 --> 0:00:13.590\nHello everyone, I am Shobhit Kundu, a PhD\nstudent from the University of Southern California.\n\n0:00:13.535 --> 0:00:18.549\nToday I am going to present our accepted work\nat NeurIPS, analyzing the confidentiality of\n\n0:00:18.549 --> 0:00:21.304\nundistillable teachers in knowledge distillation.\n\n0:00:21.249 --> 0:00:23.289\nThis work was jointly done with M.S.";

    private const string INVALID_INPUT_STRING = "WEBVTT";

    [Test]
    public void FullTextTest()
    {

    }

    [Test]
    public void GetLineTest()
    {

    }

    [Test]
    public void InvalidTranscriptTest()
    {

    }
}