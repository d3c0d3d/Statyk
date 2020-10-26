using Statyk.HtmlGen;
using System.Net;
using System.Text;

namespace Statyk
{
    public static class TemplatePage
    {
        internal static void SendResponse(HttpListenerResponse response, object content = null, int code = 200)
        {
            byte[] buffer = null;

            if (content == null)
            {
                buffer = Encoding.UTF8.GetBytes(string.Empty);
            }
            else
            {
                if (content is string)
                {
                    buffer = Encoding.UTF8.GetBytes(content as string);
                }
                if (content is byte[])
                {
                    buffer = content as byte[];
                }
            }

            response.Headers.Add(HttpResponseHeader.Server, string.Empty);
            response.AddHeader("_server", nameof(Statyk));
            response.ContentLength64 = buffer.Length;
            response.StatusCode = code;
            var output = response.OutputStream;

            try
            {
                output.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                output.Close();
                response.Close();
            }
        }

        internal static void NotImplemented(HttpListenerResponse response, string errorMessage = null)
        {
            SendResponse(response, DefaultPageFactory("Erro 501", "#AD0023", ";(", "501 - Não Implementado", errorMessage, 501), 501);
        }

        internal static void ServerInternalError(HttpListenerResponse response, string errorMessage = null)
        {
            SendResponse(response, DefaultPageFactory("Erro 501", "#AD0023", ";(", "500 - Error Interno", errorMessage, 500), 500);
        }

        internal static void NotFound(HttpListenerResponse response, string errorMessage = null)
        {
            SendResponse(response, DefaultPageFactory("Erro 404", "#00ACC8", ";(", $"404 - {errorMessage} não encontrado", null, 404), 404);
        }

        internal static void WelcomePage(HttpListenerResponse response)
        {
            SendResponse(response, DefaultPageFactory("200 OK", "#125c17", ":)", "200 - Tudo OK!"));
        }

        public static string DefaultPageFactory(string title, string bgColor, string asciiSmile, string msg, string extraMsg = null, int code = 200)
        {
            var htmlMsgTag = new HtmlNode("div")
                .AddClass("bodyHeadline")
                .CssAttr("font-size","16px")
                .CssAttr("line-height", "25px")
                .CssAttr("width","900px")
                .Append(extraMsg);

            var htmlPage = new HtmlNode()
                .Append(new HtmlNode("head")
                .Append(new HtmlNode("meta")
                    .SetAttr("charset", "utf-8"))
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
                "        }\r\n" +
                "            .content .bodyHeadline {\r\n" +
                "                margin: 35px 0 0; \r\n" +
                "                font-size: 40px; \r\n" +
                "                line-height: 43px; \r\n" +
                "            }\r\n" +
                "            .content .bodyContent {\r\n" +
                "                margin: 10px 0 30px 0; \r\n" +
                "                line-height: 22px; \r\n" +
                "            }\r\n" +
                "               .content .bodyContent a {\r\n" +
                "                   color: #fff;\r\n" +
                "                   text-decoration: none; \r\n" +
                "               }\r\n" +
                "                    .content.bodyContent a:hover {\r\n" +
                "                        opacity: .7;\r\n" +
                "                    }\r\n" +
                "            .content .bodyLink {\r\n" +
                "                color: #fff;\r\n" +
                "                display: block;\r\n" +
                "                line-height: 30px;\r\n" +
                "                height: 29px;\r\n" +
                "                width: 230px;\r\n" +
                "                cursor: pointer;\r\n" +
                "                text-decoration: none;\r\n" +
                "                position: relative;\r\n" +
                "            }\r\n" +
                "                .content .bodyLink.small {\r\n" +
                "                    margin-top: 10px;\r\n" +
                "                    width: 135px;\r\n" +
                "                }\r\n" +
                "                .content .bodyLink div {\r\n" +
                "                    position: absolute;\r\n" +
                "                    overflow: hidden;\r\n" +
                "                    width: 29px;\r\n" +
                "                    height: 29px;\r\n" +
                "                    float: right;\r\n" +
                "                    top: 0;\r\n" +
                "                    right: 0;\r\n" +
                "                }\r\n" +
                "                    .content .bodyLink div img {\r\n" +
                "                        position: absolute;\r\n" +
                "                        top: 0;\r\n" +
                "                        left: 0;\r\n" +
                "                        border: 0;\r\n" +
                "                    }\r\n" +
                "                    .content .bodyLink:hover div img {\r\n" +
                "                        left: -29px;\r\n" +
                "                    }\r\n" +
                "                .content .bodyLink:hover {\r\n" +
                "                   opacity: .7;\r\n" +
                "                }\r\n")))
                .Append(new HtmlNode("body")
                .Append(new HtmlNode("div")
                    .AddClass("content")
                    .Append(new HtmlNode("div").AddClass("bodyHeadline").Append(asciiSmile, false))
                    .Append(new HtmlNode("div").AddClass("bodyHeadline").Append(msg,false))
                    .Append(code == 500 || code == 501 && !string.IsNullOrEmpty(extraMsg) ? htmlMsgTag : null)
                    .Append(new HtmlNode("div").AddClass("bodyContent").Append($"{nameof(Statyk)} - Server",false))
                    .Append(new HtmlNode("a").AddClass("bodyLink small").SetAttr("href", "index")
                        .Append("Ir para Index",false)
                            .Append(new HtmlNode("div")
                                .Append(new HtmlNode("img")
                                .SetAttr("src", "data:image/png;base64," +
                                           "iVBORw0KGgoAAAANSUhEUgAAADoAAAAdCAYAAAD7En+mAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZ" +
                                           "VJlYWR5ccllPAAAAyRpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/Ii" +
                                           "BpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM" +
                                           "6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8w" +
                                           "Ni0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xO" +
                                           "Tk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbn" +
                                           "M6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmF" +
                                           "kb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEu" +
                                           "MC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiAoT" +
                                           "WFjaW50b3NoKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDozMjVCMDEwM0FBQkExMUUyQjdGNEEwOD" +
                                           "g0RjhFODY4OCIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDozMjVCMDEwNEFBQkExMUUyQjdGNEEwODg" +
                                           "0RjhFODY4OCI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjMyNUIw" +
                                           "MTAxQUFCQTExRTJCN0Y0QTA4ODRGOEU4Njg4IiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjMyNUIwM" +
                                           "TAyQUFCQTExRTJCN0Y0QTA4ODRGOEU4Njg4Ii8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+ID" +
                                           "wveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+I1MZRAAAA4FJREFUeNq8md1x2kAQx3UaCoAKAg0" +
                                           "YkgYQHTgV2KoAeMqjncfkBbsCSAd0AK5AOA1Y6YAOyJ7mf5rV6T4l8M3c2ALd/fZ/u/exh0gCy+VyyeiP" +
                                           "rHPtqzPVd6p7IcTJ0rbxTO8FMe++/vIy/xY/TiF9CY+4If1ZUV1SHQb0V1L9Q/WFxJy7CCVxnZkk+hwtl" +
                                           "IyTsCcGk53sMZJ8FGcY8Ux7NydB+xihJLI3k8Tug4TCi1uq92zEfpJxu4DwfoShY3y0o3a5Tyi82GKS0b" +
                                           "uA8G4xqV3uFAqRB4xYAoHPSURBH08IvwosR9omFCIbTDI0iok+GkxdrC5UjuqjHnpdCry7xaOcs2uLkQ2" +
                                           "mLfQCBTeY1Ne6JRRzcoPH731EWsQuqM+jYU7WzD4iLWIX1GfFTLVwU+HaG4gQ3WExSRhcDzcVrldhYl63" +
                                           "mAJCnwEtybiJx0tjNvETtCk9c/YDq2OuFjUSWjPJuInHSy0mtSk9c7ZmSvEpvntQIxswaGeE2wG1IDEzh" +
                                           "1fl+694XLKvejFJzMzh1RZTwMhCdkZGjSJWVr5SnjEHT44o+MDjaPrt91gxyahRxMraYtpORoiCmpmyve" +
                                           "sYMfcqCNvEKyNsnkVoq3ezLkx4qcW0eRahXTOl0C94eI9caGxiM0uTEzvVdGI6xHqZAzbJT4aQO8ADoUW" +
                                           "JzQ0nqX/sfyuTjO7EpHa54SRVM1Ntwl+rbALf+zTmwDDKJtf7SqYZvwhs92nMATrOTFDbsY2F9gwrITdy" +
                                           "wVM0Vqbae0YmP7ZZVlMj05KiTXnoqjieRx7vFHAYIDJh28KxK5OJHAaIbDCFvsc5DO0sku3VMkqEvse5E" +
                                           "uauIvF+gSgRqbbHrW7gSX4i2hn2uNUNPNliCi3LkA0nIV6NDPFCz2BYllExQ7waGeIFz2BSlmWUGLHtFU" +
                                           "Xy/o48TcOed3Umu62omI00DUVl5PdIwK+1t81UUm34vmYiAb8ZUzgS5eq+p4cnN7g5cCbyWqJsvO+J8GS" +
                                           "DyXNc0+XYlr18RA5ZRt7/btjSnqsFwXE51mK68k3L/W+DqR8HRcAViFq5Xm1pGBP4wAyu751Crjs1z9ZM" +
                                           "1wU1BLaYptsK4VktN9pRq0R9Y5/NMZL8slmC1ioSIu51ezNtkSACQ3HJckjXAX1v8nzsTxLwVBTT99OEi" +
                                           "FxkMsNIVpu/J6yjhBpEG5mhv7vI8l+AAQB7WiwH/DuungAAAABJRU5ErkJggg==")
                                .SetAttr("alt","Ir Para Index")
                                .SetAttr("title","Ir Para Index")
                                .SetAttr("witdh","58")
                                .SetAttr("height","29"))))));

            return htmlPage.ToString();
        }
    }
}
