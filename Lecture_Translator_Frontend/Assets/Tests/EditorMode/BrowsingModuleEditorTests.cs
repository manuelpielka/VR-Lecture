using UnityEngine;
using NUnit.Framework;
using BrowsingModule;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

public class BrowsingModuleEditorTests
{
    private GameObject host;
    private BrowsingManager manager;

    [SetUp]
    public void Setup()
    {
        host = new GameObject("BrowsingManager_TestHost");
        manager = host.AddComponent<BrowsingManager>();
    }

    [TearDown]
    public void Teardown()
    {
        if (host != null) Object.DestroyImmediate(host);
    }

    [Test]
    public void GetContents_Null_Test()
    {
        FolderElement root = new FolderElement("/", "root", new List<GenericElement>());
        var field = typeof(BrowsingManager).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.GetContents("");
        Assert.That(list, Is.Null);
    }

    [Test]
    public void GetContents_Empty_Test()
    {
        FolderElement root = new FolderElement("/", "root", new List<GenericElement>());
        var field = typeof(BrowsingManager).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.GetContents("/");
        Assert.That(list, Is.Empty);
    }

    [Test]
    public void GetContents_Test()
    {
        FolderElement root = new FolderElement("/", "root", new List<GenericElement>());

        FolderElement element1 = new FolderElement("/element1", "element1", new List<GenericElement>());

        LectureElement element2 = new LectureElement("/element2", "element2", null);

        root.AddContents(element1);
        root.AddContents(element2);

        List<GenericElement> testList = new List<GenericElement>();
        testList.Add(element1);
        testList.Add(element2);
        
        
        var field = typeof(BrowsingManager).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.GetContents("/");
        Assert.That(list, Is.EqualTo(testList));
    }

    [Test]
    public void Search_Null_Test()
    {
        FolderElement root = new FolderElement("/", "root", new List<GenericElement>());
        var field = typeof(BrowsingManager).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.Search("", "");
        Assert.That(list, Is.Null);
    }

    [Test]
    public void Search_Empty_Test()
    {
        FolderElement root = new FolderElement("/", "root", new List<GenericElement>());
        var field = typeof(BrowsingManager).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.Search("", "/");
        Assert.That(list, Is.Empty);
    }

    [Test]
    public void Search_Test()
    {
        FolderElement root = new FolderElement("/", "root", new List<GenericElement>());

        FolderElement element1 = new FolderElement("/element1", "element1", new List<GenericElement>());

        LectureElement element2 = new LectureElement("/element2", "element2", null);

        root.AddContents(element1);
        root.AddContents(element2);

        List<GenericElement> testList = new List<GenericElement>();
        testList.Add(element1);


        var field = typeof(BrowsingManager).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.Search("element1", "/");

        Assert.AreEqual(1, list.Count);
        Assert.That(list, Is.EqualTo(testList));
    }

    [Test]
    public void GetSessionNames_Empty_Test()
    {
        string json = "";

        var method = typeof(BrowsingManager).GetMethod("GetSessionNames",
                   BindingFlags.NonPublic | BindingFlags.Instance);

        var result = (List<string>)method.Invoke(manager, new object[] { json });

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetSessionNames_Test()
    {
        string json = "[[\" Create Directory\",\"createdir\",\"{}\",false,\"read\"]," +
              "[\" Upload media\",\"upload\",\"{}\",false,\"read\"]," +
              "[\" Record session\",\"record\",\"{}\",false,\"read\"]," +
              "[\" Back\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"Interviews\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"Doehring\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"TV-recording-2024\",\"session\",\"{}\",false,\"write\"]," +
              "[\"offline_test\",\"session\",\"{\\\"title\\\":\\\"\\\",\\\"presenter\\\":\\\"\\\",\\\"event\\\":\\\"\\\"}\",true,\"write\"]," +
              "[\"Intro to AI\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"EMNLP2023\",\"dir\",\"{}\",false,\"read\"]]";

        var method = typeof(BrowsingManager).GetMethod("GetSessionNames",
                   BindingFlags.NonPublic | BindingFlags.Instance);

        var result = (List<string>)method.Invoke(manager, new object[] { json });

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.Contains("TV-recording-2024", result);
        Assert.Contains("offline_test", result);
    }

    [Test]
    public void GetFolderNames_Empty_Test()
    {
        string json = "";

        var method = typeof(BrowsingManager).GetMethod("GetSessionNames",
                   BindingFlags.NonPublic | BindingFlags.Instance);

        var result = (List<string>) method.Invoke(manager, new object[] { json });

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetFolderNames_Test()
    {
        string json = "[[\" Create Directory\",\"createdir\",\"{}\",false,\"read\"]," +
              "[\" Upload media\",\"upload\",\"{}\",false,\"read\"]," +
              "[\" Record session\",\"record\",\"{}\",false,\"read\"]," +
              "[\" Back\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"Interviews\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"Doehring\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"TV-recording-2024\",\"session\",\"{}\",false,\"write\"]," +
              "[\"offline_test\",\"session\",\"{\\\"title\\\":\\\"\\\",\\\"presenter\\\":\\\"\\\",\\\"event\\\":\\\"\\\"}\",true,\"write\"]," +
              "[\"Intro to AI\",\"dir\",\"{}\",false,\"read\"]," +
              "[\"EMNLP2023\",\"dir\",\"{}\",false,\"read\"]]";

        var method = typeof(BrowsingManager).GetMethod("GetFolderNames",
                   BindingFlags.NonPublic | BindingFlags.Instance);

        var result = (List<string>)method.Invoke(manager, new object[] { json });

        Assert.That(result.Count, Is.EqualTo(5));
        Assert.Contains(" Back", result);
        Assert.Contains("Interviews", result);
        Assert.Contains("Doehring", result);
        Assert.Contains("Intro to AI", result);
        Assert.Contains("EMNLP2023", result);
    }

    [Test]
    public async Task UpdateDir_Test()
    {
        FolderElement root = new FolderElement("/", "root", new List<GenericElement>());
        var field = typeof(BrowsingManager).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, root);

        await manager.UpdateFolders();

        root = (FolderElement) field.GetValue(manager);

        Assert.IsTrue(1 == root.GetContents().Count);
    }
}
