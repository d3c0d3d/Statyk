namespace Statyk.HtmlGen
{
    public class Button : HtmlNode
    {
        /// <summary>
        /// Html Button Tag
        /// </summary>
        public Button() : base("button") { }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class Div : HtmlNode
    {
        /// <summary>
        /// Html Div Tag
        /// </summary>
        public Div() : base("div") { }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
