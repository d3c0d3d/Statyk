using System;
using System.Text.RegularExpressions;

namespace Statyk.HtmlGen
{
    public class HtmlNode
    {
        /// <summary>
        /// tag in context
        /// </summary>
        private string _tag;

        private string _cssValue;

        /// <summary>
        /// Use <see cref="CreateElement"/>
        /// </summary>
        /// <param name="elementName"></param>
        public HtmlNode(string elementName, bool voidTag = false, bool closeVoidTag = true)
        {
            if (elementName == "html")
                elementName = "<!DOCTYPE html>";

            CreateElement(elementName, voidTag, closeVoidTag);
        }
        /// <summary>
        /// Create 'html' tag
        /// </summary>
        public HtmlNode()
        {
            CreateElement("<!DOCTYPE html>");
        }

        /// <summary>
        /// Create HTML Element (Tag) <![CDATA[ex: <div></div> ]]>
        /// </summary>
        /// <param name="elementName">Name of element ex: span, div, title...</param>
        /// <returns></returns>
        public HtmlNode CreateElement(string elementName, bool voidTag = false, bool closeVoidTag = true)
        {
            if (!string.IsNullOrEmpty(_tag))
                throw new Exception($"'{nameof(CreateElement)}' it cannot be used more than once in the creation context.");

            elementName = elementName.Replace("<", "").Replace(">", "");

            if (voidTag)
                _tag = $"<{elementName}{(closeVoidTag ? $"/>" : ">")}";
            else
                _tag = $"<{elementName}></{elementName.Replace("!DOCTYPE ", string.Empty)}>";
            return this;
        }

        /// <summary>
        /// Set a parameter to an HTML tag you have previously defined <![CDATA[ex: <input type="text"></input> ]]>
        /// </summary>
        /// <param name="propertyName">Property to be inserted or modified ex: 'type'</param>
        /// <param name="value">Value for property ex: 'text'</param>
        /// <returns><see cref="ToString()"/> returns the updated tag</returns>
        public HtmlNode SetAttr(string propertyName, dynamic value)
        {
            if (string.IsNullOrEmpty(_tag))
                throw new Exception("'Tag' must be created first.");

            _tag = BuildPropertyValue(propertyName, value);

            return this;
        }

        /// <summary>
        /// Add or Append in 'style' attribute
        /// </summary>
        /// <param name="propertyName">ex: margin-right</param>
        /// <param name="value">ex: 5px</param>
        /// <returns></returns>
        public HtmlNode CssAttr(string propertyName, string value)
        {
            if (string.IsNullOrEmpty(_tag))
                throw new Exception("'Tag' must be created first.");

            var newValue = $"{(!string.IsNullOrEmpty(_cssValue) ? _cssValue + " " : string.Empty)}{propertyName}: {value};";
            _cssValue = $"{newValue}";

            return SetAttr("style", newValue);
        }

        /// <summary>
        /// Insert the content informed in <param name="unsafeElement">TAG</param> for a tag in context that can be, for example, a 'div' or css code
        /// </summary>
        /// <param name="unsafeElement">Content <![CDATA[ex: <span class="bs3"></span>]]></param>
        /// <param name="indenting">Final result with indentation?</param>
        /// <returns><see cref="ToString()"/> returns the updated tag</returns>
        public HtmlNode Append(string unsafeElement, bool indenting = true)
        {
            if (!string.IsNullOrEmpty(unsafeElement))
            {
                var tag = _tag.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                var initialPart = string.Empty;

                for (var i = 0; i < tag.Length - 2; i++)
                {
                    initialPart += tag[i] + ">";
                }

                var rightPart = tag[tag.Length - 2];
                var leftPart = tag[tag.Length - 1];

                _tag = $"{initialPart}{rightPart}>{(indenting ? "\n  " : "")}{unsafeElement}{(indenting && !leftPart.Contains("\n") ? "\n" : "")}{leftPart}>";
            }

            return this;
        }
        /// <summary>
        /// Insert the content informed in <see cref="HtmlNode"/> TAG for a tag in context that can be, for example, a 'div'
        /// </summary>
        /// <param name="element"></param>
        /// <param name="indenting"></param>
        /// <returns></returns>
        public HtmlNode Append(HtmlNode element, bool indenting = true)
        {
            if (element != null)
            {
                var tag = _tag.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                var initialPart = string.Empty;

                for (var i = 0; i < tag.Length - 2; i++)
                {
                    initialPart += tag[i] + ">";
                }

                var rightPart = tag[tag.Length - 2];
                var leftPart = tag[tag.Length - 1];

                _tag = $"{initialPart}{rightPart}>{(indenting ? "\n  " : "")}{element}{(indenting && !leftPart.Contains("\n") ? "\n" : "")}{leftPart}>";
            }
            return this;
        }

        /// <summary>
        /// Generate HTML String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _tag;
        }

        private string BuildPropertyValue(string propertyName, dynamic value)
        {
            var tagParts = _tag.Split(new char[] { '>' } , StringSplitOptions.RemoveEmptyEntries);

            var prop = tagParts[0].Contains(propertyName);
            if (prop)
            {
                var newPropValue = Regex.Replace(tagParts[0], $"{propertyName}=\"[^?]*\"", $"{propertyName}=\"{value}\"");
                return $"{newPropValue}>{(tagParts.Length == 2 ? $"{tagParts[1]}>" : "")}";
            }
            return $"{tagParts[0]} {propertyName}=\"{value}\">{(tagParts.Length == 2 ? $"{tagParts[1]}>" : "")}";
        }
    }
}
