using System.Linq;

namespace Statyk.HtmlGen
{
    public static class HtmlNodeEx
    {
        public static HtmlNode CreatePage(string title, HtmlNode[] headTags, HtmlNode[] bodyTags, string lang = "en")
        {
            var header = new Head()
            .Append(new Meta()
                .SetAttr("charset", "utf-8"))
            .Append(new Meta()
                .SetAttr("http-equiv", "X-UA-Compatible")
                .SetAttr("content", "IE-edge"))
            .Append(new Meta()
                .SetAttr("name", "viewport"))
            .Append(new Title()
                .Append(title,false));

            if (headTags != null)
                foreach (var head in headTags)
                {
                    header.Append(head);
                }

            var bodyr = new Body();

            if (bodyTags != null)
                foreach (var body in bodyTags)
                {
                    bodyr.Append(body);
                }

            var html = new HtmlNode().SetAttr("lang", lang)
                .Append(header)
                .Append(bodyr);

            return html;
        }

        public static HtmlNode AddLink(string href)
        {
            var htmlNode = new Link().SetAttr("rel", "stylesheet").SetAttr("href", href);
            return htmlNode;
        }

        public static HtmlNode AddLink(this HtmlNode htmlNode, string href)
        {
            htmlNode.Append(new Link().SetAttr("rel", "stylesheet").SetAttr("href", href));
            return htmlNode;
        }
        public static HtmlNode AddScript(string src)
        {
            var htmlNode = new Script().SetAttr("type", "application/javascript").SetAttr("src", src);
            return htmlNode;
        }

        public static HtmlNode AddScript(this HtmlNode htmlNode, string src)
        {
            htmlNode.Append(new Script().SetAttr("type", "application/javascript").SetAttr("src", src));
            return htmlNode;
        }
        /// <summary>
        /// Add 'class' attribute with <see cref="HtmlNode.SetAttr"/>
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static HtmlNode AddClass(this HtmlNode htmlNode, string className)
        {
            return htmlNode.SetAttr("class", className);
        }


    }
}
