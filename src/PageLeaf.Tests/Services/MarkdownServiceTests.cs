using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using System.Text.RegularExpressions;
using System;
using System.IO;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class MarkdownServiceTests
    {
        [TestMethod]
        public void ConvertToHtml_ShouldConvertMarkdownToHtml_WhenCssPathIsNull()
        {
            // テスト観点: CSSパスがnullの場合、単純なMarkdownがCSSリンクなしの完全なHTMLに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            string markdown = "# Hello";

            // Act
            string actualHtml = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Matches(actualHtml, new Regex(@"<h1[^>]*>Hello</h1>"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldIncludeCssLink_WhenCssPathIsProvided()
        {
            // テスト観点: CSSパスが提供された場合、生成されるHTMLの<head>内に<link>タグが正しく挿入され、
            //             キャッシュバスティングのためのタイムスタンプが付与されることを確認する。
            // Arrange
            var service = new MarkdownService();
            string markdown = "# Hello";
            string cssPath = "C:\\styles\\github.css";

            // Act
            string actualHtml = service.ConvertToHtml(markdown, cssPath);

            // Assert
            // <link rel="stylesheet" href="C:\styles\github.css?v={timestamp}"> の形式を正規表現で検証
            StringAssert.Matches(actualHtml, new Regex($"<link rel=\"stylesheet\" href=\"{Regex.Escape(cssPath)}\\?v=\\d+\">"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertEmphasis()
        {
            // テスト観点: 太字、イタリック、打ち消し線が正しくHTMLタグに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "**bold** *italic* ~~strike~~";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Contains(html, "<strong>bold</strong>");
            StringAssert.Contains(html, "<em>italic</em>");
            StringAssert.Contains(html, "<del>strike</del>");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertLists()
        {
            // テスト観点: 順序なしリストと順序付きリストが正しくHTMLタグに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "* Unordered\n1. Ordered";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Contains(html, "<ul>\n<li>Unordered</li>\n</ul>");
            StringAssert.Contains(html, "<ol>\n<li>Ordered</li>\n</ol>");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertLinksAndImages()
        {
            // テスト観点: リンクと画像が正しくHTMLタグに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "[PageLeaf](https://example.com)\n![alt text](image.png)";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Contains(html, "<a href=\"https://example.com\">PageLeaf</a>");
            StringAssert.Contains(html, "<img src=\"image.png\" alt=\"alt text\" />");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertQuotesAndCode()
        {
            // テスト観点: 引用、インラインコード、コードブロックが正しくHTMLタグに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "> quote\n`code`\n```csharp\nvar x = 1;\n```";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Matches(html, new Regex(@"<blockquote>\s*<p>quote\s+<code>code</code></p>\s*</blockquote>"));
            StringAssert.Matches(html, new Regex(@"<pre><code[^>]*>var x = 1;\n</code></pre>"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertTaskListsAndHorizontalRules()
        {
            // テスト観点: タスクリストと水平線が正しくHTMLタグに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "- [x] Done\n- [ ] Todo\n\n---";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Matches(html, new Regex(@"<input[^>]+checked=""checked""[^>]*>"));
            Assert.IsTrue(Regex.IsMatch(html, @"<li[^>]*>\s*<input[^>]+type=""checkbox""(?!.*checked)[^>]*>"), "Unchecked task list item not found.");
            StringAssert.Contains(html, "<hr />");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertPipeTableToHtmlTable()
        {
            // テスト観点: Markdownのパイプテーブルが、意図したHTMLのテーブル構造に正しく変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "|赤身|白身|軍艦|\n" +
                           "|:---|:---:|---:|\n" +
                           "|マグロ|ヒラメ|ウニ|\n" +
                           "|カツオ|タイ|イクラ|\n" +
                           "|トロ|カンパチ|ネギトロ|";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Contains(html, "<table>");
            StringAssert.Contains(html, "<thead>");
            StringAssert.Contains(html, "<tbody>");
            StringAssert.Matches(html, new Regex(@"<th.*>赤身</th>"));
            StringAssert.Matches(html, new Regex(@"<td.*>マグロ</td>"));

            // ヘッダー1行 + データ3行 = 4つの<tr>タグがあるはず
            int trCount = System.Text.RegularExpressions.Regex.Matches(html, "<tr>").Count;
            Assert.AreEqual(4, trCount, "Expected 4 <tr> tags for header and 3 data rows.");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldAddLanguageClassToCodeBlocks()
        {
            /// テスト観点: 言語指定されたMarkdownコードブロックが、変換後に適切な言語クラスを持つ
            ///             `<code>` タグを生成することを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "```csharp\nConsole.WriteLine(\"Hello\");\n```";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            // <pre><code class="language-csharp">Console.WriteLine(&quot;Hello&quot;);
            // </code></pre> のようなHTMLが生成されることを期待
            StringAssert.Contains(html, "<pre><code class=\"language-csharp\">Console.WriteLine(&quot;Hello&quot;);");
            StringAssert.Contains(html, "</code></pre>");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldLinkToHighlightJsResources()
        {
            /// テスト観点: ConvertToHtml メソッドが生成するHTMLに、シンタックスハイライト用の
            ///             JavaScriptへの絶対パスリンクが正しく含まれていることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "```csharp\nvar x = 1;\n```";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // 絶対パスの構築 (テスト内でMarkdownServiceと同じロジックを使う)
            var scriptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highlight", "highlight.min.js");
            var scriptFileUri = new Uri(scriptFilePath).AbsoluteUri;

            var cssFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highlight", "styles", "github.css");
            var cssFileUri = new Uri(cssFilePath).AbsoluteUri;

            // Assert
            // 1. スタイルシートへのリンクが含まれているか
            StringAssert.Matches(html, new Regex($@"<link\s+rel=""stylesheet""\s+href=""{Regex.Escape(cssFileUri)}"">"), "highlight.jsのCSSへの<link>タグが見つかりません。");

            // 2. JavaScriptライブラリへのリンクが絶対パスで含まれているか
            StringAssert.Matches(html, new Regex($@"<script\s+src=""{Regex.Escape(scriptFileUri)}""></script>"), "highlight.jsライブラリへの<script>タグが見つかりません。");

            // 3. ハイライトを有効化するスクリプトが埋め込まれているか
            StringAssert.Matches(html, new Regex(@"<script>hljs\.highlightAll\(\);</script>"), "ハイライトを初期化するスクリプト 'hljs.highlightAll()' が見つかりません。");
        }
    }
}
