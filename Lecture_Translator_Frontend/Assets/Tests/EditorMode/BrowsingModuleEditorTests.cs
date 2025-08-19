using UnityEngine;
using NUnit.Framework;
using BrowsingModule;
using System.Collections.Generic;
using System.IO;

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
        List<GenericElement> list = manager.GetContents("");
        Assert.That(list, Is.Null);
    }

    [Test]
    public void GetContents_Empty_Test()
    {
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
        
        
        var field = typeof(BrowsingManager).GetField("root", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.GetContents("/");
        Assert.That(list, Is.EqualTo(testList));
    }

    [Test]
    public void Search_Null_Test()
    {
        List<GenericElement> list = manager.Search("", "");
        Assert.That(list, Is.Null);
    }

    [Test]
    public void Search_Empty_Test()
    {
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


        var field = typeof(BrowsingManager).GetField("root", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(manager, root);

        List<GenericElement> list = manager.Search("element1", "/");
        Assert.That(list, Is.EqualTo(testList));
    }
}
