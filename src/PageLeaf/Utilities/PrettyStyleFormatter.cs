using AngleSharp;
using AngleSharp.Css.Dom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PageLeaf.Utilities
{
    public class PrettyStyleFormatter : IStyleFormatter
    {
        private int _level = 0;

        private void Indent(TextWriter writer)
        {
            for (var i = 0; i < _level; i++)
            {
                writer.Write("  "); // スペース2つでインデント
            }
        }

        public string Sheet(IEnumerable<IStyleFormattable> rules)
        {
            using (var writer = new StringWriter())
            {
                var first = true;
                foreach (var rule in rules)
                {
                    if (rule is ICssStyleRule styleRule && styleRule.Style.Length == 0)
                    {
                        continue;
                    }

                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                    writer.Write(Rule(rule));
                }
                return writer.ToString();
            }
        }

        public string Block(IEnumerable<IStyleFormattable> rules)
        {
            using (var writer = new StringWriter())
            {
                var first = true;
                foreach (var rule in rules)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                    writer.Write(Rule(rule));
                }
                return writer.ToString();
            }
        }

        public string BlockDeclarations(IEnumerable<IStyleFormattable> declarations)
        {
            using (var writer = new StringWriter())
            {
                foreach (var declaration in declarations)
                {
                    Indent(writer);
                    writer.Write(Declaration(declaration));
                    writer.WriteLine();
                }
                return writer.ToString();
            }
        }

        public string BlockRules(IEnumerable<IStyleFormattable> rules)
        {
            using (var writer = new StringWriter())
            {
                var first = true;
                foreach (var rule in rules)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                    writer.Write(Rule(rule));
                }
                return writer.ToString();
            }
        }

        public string Rule(IStyleFormattable rule)
        {
            using (var writer = new StringWriter())
            {
                if (rule is ICssStyleRule styleRule)
                {
                    Indent(writer);
                    writer.Write(styleRule.SelectorText);
                    writer.WriteLine(" {");
                    _level++;
                    writer.Write(BlockDeclarations(styleRule.Style.OfType<IStyleFormattable>()));
                    _level--;
                    Indent(writer);
                    writer.Write("}");
                }
                else if (rule is ICssRule cssRule)
                {
                    // 他のルール（@import, @charsetなど）はデフォルトのフォーマットを使用
                    writer.Write(cssRule.ToCss());
                }
                return writer.ToString();
            }
        }

        public string Rule(string selector, string declarations)
        {
            using (var writer = new StringWriter())
            {
                Indent(writer);
                writer.Write(selector);
                writer.WriteLine(" {");
                _level++;
                Indent(writer);
                writer.Write(declarations);
                writer.WriteLine();
                _level--;
                Indent(writer);
                writer.Write("}");
                return writer.ToString();
            }
        }

        public string Rule(string prelude, string rules, string suffix)
        {
            using (var writer = new StringWriter())
            {
                Indent(writer);
                writer.Write(prelude);
                writer.WriteLine(" {");
                _level++;
                writer.Write(rules);
                _level--;
                Indent(writer);
                writer.Write("}");
                writer.Write(suffix);
                return writer.ToString();
            }
        }

        public string Declaration(IStyleFormattable property)
        {
            if (property is ICssProperty cssProperty)
            {
                var important = cssProperty.IsImportant ? " !important" : "";
                return $"{cssProperty.Name}: {cssProperty.Value}{important};";
            }
            return property.ToCss(); // Fallback
        }

        public string Declaration(string name, string value, bool important)
        {
            return $"{name}: {value}{(important ? " !important" : "")};";
        }

        public string Comment(string data)
        {
            using (var writer = new StringWriter())
            {
                Indent(writer);
                writer.WriteLine($"/* {data} */");
                return writer.ToString();
            }
        }
    }
}
