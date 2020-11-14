using Statyk.HtmlGen;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using XStd.Net;
using static XStd.Cli;
using static Statyk.HtmlGen.HtmlNodeEx;
using System;
using XStd.TestEngine;

namespace Statyk.IntegratedTests
{
    public class Tests : TestEx
    {
        private static int _wsPort = 5000;
        //private string _wsUrl = $"ws://localhost:{_wsPort}";

        [TestMethod("controller-test")]
        public void ControllerTest()
        {
            var statykServer = StatykServer.Create(_wsPort)
                .AddController<PersonController>()
                .Listen();

            if (statykServer.IsListen)
            {
                PrintLn($"RootPath => {statykServer.RootPath}");
                PrintLn($"Server at running in {statykServer.UriPrefix}");
            }

            TestRunner.TestRun(() => statykServer.IsListen.IsTrue());
        }

        [TestMethod("controller-index-test")]
        public void ControllerIndexTest()
        {
            var statykServer = StatykServer.Create(_wsPort)
                .AddController<IndexController>()
                .AddController<PersonController>()
                .Listen();

            if (statykServer.IsListen)
            {
                PrintLn($"RootPath => {statykServer.RootPath}");
                PrintLn($"Server at running in {statykServer.UriPrefix}");
            }

            TestRunner.TestRun(() => statykServer.IsListen.IsTrue());
        }

        [TestMethod("html-simple-test")]
        public void HtmlSimpleTest()
        {
            var bgColor = "green";

            var html = new HtmlNode()
                .Append(new HtmlNode("head")
                .Append(new HtmlNode("meta")
                    .SetAttr("charset", "uft-8"))
                .Append(new HtmlNode("meta")
                    .SetAttr("http-equiv", "X-UA-Compatible")
                    .SetAttr("content", "IE-edge"))
                .Append(new HtmlNode("meta")
                    .SetAttr("name", "viewport"))
                .Append(new HtmlNode("title")
                    .SetAttr("name", $"{nameof(Statyk)} Server"))
                .Append(new HtmlNode("style")
                    .SetAttr("media", "screen").Append("font-face {\r\n" +
                "            font-family: 'SegoeLight', helvetica, sans-serif;\r\n" +
                "            font-weight: normal;\r\n" +
                "            font-style: normal;\r\n" +
                "        }\r\n" +
                "        body {\r\n" +
                "            background-color: " + bgColor + ";\r\n" +
                "            color: #fff;\r\n" +
                "            font-family: 'SegoeLight', helvetica, sans-serif;\r\n" +
                "            font-size: 18px;\r\n" +
                "            margin: 0;\r\n" +
                "            padding: 0;\r\n" +
                "        }\r\n" +
                "        .content {\r\n" +
                "            position: absolute;\r\n" +
                "            left: 50px;\r\n" +
                "            top: 38px;\r\n" +
                "            width: 560px;\r\n" +
                "        }\r\n")))
                .Append(new HtmlNode("body"));

            PrintLn(html.ToString());

        }

        [TestMethod("template-default-page-test")]
        public void TemplatePageTest()
        {
            PrintLn(TemplatePage.DefaultPageFactory("200 OK", "#125c17", ":)", "200 - Tudo OK!"));
        }

        [TestMethod("elements-test")]
        public void ElementsTest()
        {
            PrintLnC(new Button().AddClass("btn-default").ToString(), ConsoleColor.Yellow);
        }

        [TestMethod("create-page-test")]
        public void CreatePageTest()
        {
            PrintLnC(CreatePage("Server Test",
                new HtmlNode[] { AddLink("/site.css"), AddScript("/site.js") },
                new HtmlNode[] { AddLink("/site.css"), AddScript("/site.js"),new Br() }).ToString(), ConsoleColor.Yellow);
        }
        public override List<string> ListTestMethods()
        {
            foreach (var item in base.ListTestMethods())
            {
                PrintLnC(item, ConsoleColor.White);
            }

            return null;
        }
    }

    internal class IndexController
    {
        public HtmlNode Get()
        {
            var person = new { Id = 1, Name = "tester" };

            var json = JsonSerializer.Serialize<dynamic>(person);


            var page = CreatePage("Server Test",
                new HtmlNode[] { AddLink("/site.css"), AddScript("/site.js") },
                new HtmlNode[] { new H1().Append($"Hello World - {person.Name}"), new Br(), new H6().Append($"{nameof(Statyk)}"), });

            return page;
        }
    }

    internal class PersonController
    {
        [GET("index")]
        public string Index()
        {
            string component = $"<h1>Index</h1>";

            return component;
        }

        [GET("Person")]
        public string GetPerson(int id)
        {
            var person = new { Id = id, Name = "tester" };

            var json = System.Text.Json.JsonSerializer.Serialize<dynamic>(person);

            return json;
        }

        [GET("Person2")]
        public string GetPerson2([FromBody] string json, int id)
        {
            string person = $"person{id}";

            return person;
        }

        [POST("Login")]
        public string MakeLogin([FromBody] string json)
        {
            var login = JsonTools.JSON<Login>.Deserialize(json);

            return $"User '{login.Username}' Logged";
        }
    }

    [DataContract]
    public class Login
    {

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }


}
