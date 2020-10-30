using System;
using System.Linq;

namespace Statyk.HtmlGen
{
    public static class HtmlUtil
    {
        private static readonly string[] voidTags =
        {
            "area", "base", "br", "col", "command", "embed", "hr", "img", "input",
            "keygen", "link", "meta", "param", "source", "track", "wbr"
        };

        private static readonly string[] closedVoidTags =
        {
            "br", "img"
        };

        public static readonly string[] AllTags =
        {
            "A", "Abbr", "Address", "Area", "Article", "Aside", "Audio", "B", "Base", "Bdi",
            "Bdo", "Blockquote", "Body", "Br", "Button", "Canvas", "Caption", "Cite", "Code",
            "Col", "Colgroup", "Command", "Datalist", "Dd", "Del", "Details", "Dfn", "Div",
            "Dl", "Dt", "Em", "Embed", "Fieldset", "Figcaption", "Figure", "Footer", "Form",
            "H1", "H2", "H3", "H4", "H5", "H6", "Head", "Header", "Hgroup", "Hr", "Html", "I",
            "Iframe", "Img", "Input", "Ins", "Kbd", "Keygen", "Label", "Legend", "Li", "Link",
            "Main", "Map", "Mark", "Menu", "Meta", "Meter", "Nav", "Noscript", "Object", "Ol",
            "Optgroup", "Option", "Output", "P", "Param", "Pre", "Progress", "Q", "Rp", "Rt",
            "Ruby", "S", "Samp", "Script", "Section", "Select", "Small", "Source", "Span", "Wbr",
            "Strong", "Style", "Sub", "Summary", "Sup", "Table", "Tbody", "Td", "Textarea",
            "Tfoot", "Th", "Thead", "Time", "Title", "Tr", "Track", "U", "Ul", "Var", "Video",
        };

        public static bool IsVoid(string tagName) => voidTags.Contains(tagName, StringComparer.InvariantCulture);
        public static bool IsClosedVoid(string tagName) => closedVoidTags.Contains(tagName, StringComparer.InvariantCulture);
    }
}
