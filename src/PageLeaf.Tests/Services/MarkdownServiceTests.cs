using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using System.Text.RegularExpressions;
using System;
using System.IO;
using Moq;
using PageLeaf.Models;
using System.Collections.Generic;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class MarkdownServiceTests
    {
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private MarkdownService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings());
            _service = new MarkdownService(_settingsServiceMock.Object);
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertMarkdownToHtml_WhenCssPathIsNull()
        {
            // テスト観点: CSSパスがnullの場合、単純なMarkdownがCSSリンクなしの完全なHTMLに変換されることを確認する。
            // Arrange
            string markdown = "# Hello";

            // Act
            string actualHtml = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Matches(actualHtml, new Regex(@"<h1[^>]*>Hello</h1>"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldIncludeCssLink_WhenCssPathIsProvided()
        {
            // テスト観点: CSSパスが提供された場合、生成されるHTMLの<head>内に<link>タグが正しく挿入され、
            //             キャッシュバスティングのためのタイムスタンプが付与されることを確認する。
            // Arrange
            string markdown = "# Hello";
            string cssPath = @"C:\styles\github.css";

            // Act
            string actualHtml = _service.ConvertToHtml(markdown, cssPath, null);

            // Assert
            // Regex.Escape を使用してパスを安全にエスケープしつつ検証
            string expectedPattern = @"<link\s+rel=""stylesheet""\s+href=""" + Regex.Escape(cssPath) + @"\?v=\d+"">";
            StringAssert.Matches(actualHtml, new Regex(expectedPattern));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertEmphasis()
        {
            // テスト観点: 太字、イタリック、打ち消し線が正しくHTMLタグに変換されることを確認する。
            // Arrange
            var markdown = "**bold** *italic* ~~strike~~";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

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
            var markdown = "* Unordered\n1. Ordered";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Contains(html, "<ul>\n<li>Unordered</li>\n</ul>");
            StringAssert.Contains(html, "<ol>\n<li>Ordered</li>\n</ol>");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertLinksAndImages()
        {
            // テスト観点: リンクと画像が正しくHTMLタグに変換されることを確認する。
            // Arrange
            var markdown = "[PageLeaf](https://example.com)\n![alt text](image.png)";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Contains(html, "<a href=\"https://example.com\">PageLeaf</a>");
            StringAssert.Contains(html, "<img src=\"image.png\" alt=\"alt text\" />");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertQuotesAndCode()
        {
            // テスト観点: 引用、インラインコード、コードブロックが正しくHTMLタグに変換されることを確認する。
            // Arrange
            var markdown = "> quote\n`code`\n```csharp\nvar x = 1;\n```";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Matches(html, new Regex(@"<blockquote>\s*<p>quote\s+<code>code</code></p>\s*</blockquote>"));
            StringAssert.Matches(html, new Regex(@"<pre><code[^>]*>var x = 1;\n</code></pre>"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertTaskListsAndHorizontalRules()
        {
            // テスト観点: タスクリストと水平線が正しくHTMLタグに変換されることを確認する。
            // Arrange
            var markdown = "- [x] Done\n- [ ] Todo\n\n---";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

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
            var markdown = "|赤身|白身|軍艦|\n" +
                           "|:---|:---:|---:|\n" +
                           "|マグロ|ヒラメ|ウニ|\n" +
                           "|カツオ|タイ|イクラ|\n" +
                           "|トロ|カンパチ|ネギトロ|";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Contains(html, "<table>");
            StringAssert.Contains(html, "<thead>");
            StringAssert.Contains(html, "<tbody>");
            StringAssert.Matches(html, new Regex(@"<th.*>赤身</th>"));
            StringAssert.Matches(html, new Regex(@"<td.*>マグロ</td>"));

            // ヘッダー1行 + データ3行 = 4つの<tr>タグがあるはず
            int trCount = Regex.Matches(html, "<tr>").Count;
            Assert.AreEqual(4, trCount, "Expected 4 <tr> tags for header and 3 data rows.");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldAddLanguageClassToCodeBlocks()
        {
            /// テスト観点: 言語指定されたMarkdownコードブロックが、変換後に適切な言語クラスを持つ
            ///             `<code>` タグを生成することを確認する。
            // Arrange
            var markdown = "```csharp\nConsole.WriteLine(\"Hello\");\n```";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Contains(html, "<pre><code class=\"language-csharp\">Console.WriteLine(&quot;Hello&quot;);");
            StringAssert.Contains(html, "</code></pre>");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldLinkToHighlightJsResources()
        {
            // テスト観点: ConvertToHtml メソッドが生成するHTMLに、設定されたテーマのCSSへの絶対パスリンクが正しく含まれていることを確認する。
            // Arrange
            var customTheme = "vs2015.css";
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings { CodeBlockTheme = customTheme });
            var markdown = "```csharp" + Environment.NewLine + "var x = 1;" + Environment.NewLine + "```";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // 絶対パスの構築
            var scriptFilePath = Path.Combine(App.BaseDirectory, "highlight", "highlight.min.js");
            var scriptFileUri = new Uri(scriptFilePath).AbsoluteUri;

            var cssFilePath = Path.Combine(App.BaseDirectory, "highlight", "styles", customTheme);
            var cssFileUri = new Uri(cssFilePath).AbsoluteUri;

            // Assert
            // 1. スタイルシートへのリンクが含まれているか (Regex.Escape を使用して安全に)
            var cssPattern = @"<link\s+rel=""stylesheet""\s+href=""" + Regex.Escape(cssFileUri) + @""">";
            StringAssert.Matches(html, new Regex(cssPattern));

            // 2. JavaScriptライブラリへのリンクが絶対パスで含まれているか
            var scriptPattern = @"<script\s+src=""" + Regex.Escape(scriptFileUri) + @"""></script>";
            StringAssert.Matches(html, new Regex(scriptPattern));

            // 3. ハイライトを有効化するスクリプトが埋め込まれているか
            StringAssert.Matches(html, new Regex(@"<script>hljs\.highlightAll\(\);</script>"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldPrioritizeFrontMatterHighlightTheme()
        {
            // テスト観点: フロントマターで syntax_highlight が指定されている場合、引数で渡されたテーマよりも優先されることを確認する。
            // Arrange
            // 実際に存在する可能性の高いテーマ名（githubなど）をシミュレート
            var markdown = "---\nsyntax_highlight: github\n---\n```csharp\ncode\n```";
            var defaultTheme = "vs2015.css";
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings { CodeBlockTheme = defaultTheme });

            // Act
            var html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Contains(html, "highlight/styles/github");
            Assert.IsFalse(html.Contains("highlight/styles/vs2015"), "Default theme should be ignored.");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldFallbackToDefaultTheme_WhenFrontMatterThemeDoesNotExist()
        {
            // テスト観点: フロントマターで指定されたテーマファイルが存在しない場合、
            //             設定で指定されたデフォルトテーマを使用することを確認する。
            // Arrange
            var markdown = "---\nsyntax_highlight: invalid-theme-name\n---\n```csharp\ncode\n```";
            var defaultTheme = "github.css";
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings { CodeBlockTheme = defaultTheme });

            // Act
            var html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Contains(html, "highlight/styles/github.css");
            Assert.IsFalse(html.Contains("highlight/styles/invalid-theme-name"), "Invalid theme should be ignored.");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldIncludeAllRequiredResources()
        {
            // テスト観点: 生成されたHTMLに、ハイライト、拡張機能、Mermaidのすべての必要なリソースが含まれていることを確認する。
            // Arrange
            var markdown = "Check resources";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            // CSS
            StringAssert.Contains(html, "css/extensions.css");
            StringAssert.Contains(html, "highlight/styles/github.css");

            // JS
            StringAssert.Contains(html, "highlight/highlight.min.js");
            StringAssert.Contains(html, "highlight/pageleaf-extensions.js");
            StringAssert.Contains(html, "mermaid/mermaid.min.js");

            // Initialization
            StringAssert.Contains(html, "mermaid.initialize");
            StringAssert.Contains(html, "mermaid.contentLoaded()");
            StringAssert.Contains(html, "hljs.highlightAll()");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldInsertBaseTag_WhenBaseDirectoryIsProvided()
        {
            // テスト観点: ベースディレクトリが提供された場合、<head>に<base>タグが挿入されることを確認する。
            // Arrange
            var markdown = "Test";
            var baseDir = App.BaseDirectory; // 実際に存在するディレクトリを使用

            // Act
            var html = _service.ConvertToHtml(markdown, null, baseDir);

            // Assert
            // URI変換
            var baseUri = new Uri(baseDir).AbsoluteUri;
            if (!baseUri.EndsWith("/")) baseUri += "/";

            StringAssert.Contains(html, $"<base href=\"{baseUri}\" />");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldHideYamlFrontMatter()
        {
            // テスト観点: YAMLフロントマターがHTML出力に含まれないことを確認する。
            // Arrange
            var markdown = "---\ntitle: test\n---\n\n# Content";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            Assert.IsFalse(html.Contains("title: test"), "YAML front matter should not be present in HTML body.");
            StringAssert.Matches(html, new Regex(@"<h1.*>Content</h1>"), "Content should be present in HTML body as h1.");
        }

        [TestMethod]
        public void ParseFrontMatter_ShouldReturnDictionary_WhenFrontMatterExists()
        {
            // テスト観点: フロントマターが存在する場合、正しく辞書形式で取得できることを確認する。
            // Arrange
            var markdown = "---\ntitle: test\ndate: 2026-01-01\n---\n# Content";

            // Act
            var result = _service.ParseFrontMatter(markdown);

            // Assert
            Assert.IsTrue(result.ContainsKey("title"));
            Assert.AreEqual("test", result["title"]);
            Assert.IsTrue(result.ContainsKey("date"));
            // YamlDotNetのデフォルトのデシリアライズ挙動により型が変わる可能性があるため文字列表現で比較
            Assert.AreEqual("2026-01-01", result["date"].ToString());
        }

        [TestMethod]
        public void ParseFrontMatter_ShouldReturnEmptyDictionary_WhenNoFrontMatter()
        {
            // テスト観点: フロントマターが存在しない場合、空の辞書が返されることを確認する。
            // Arrange
            var markdown = "# Content";

            // Act
            var result = _service.ParseFrontMatter(markdown);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Split_ShouldSeparateFrontMatterAndBody()
        {
            // テスト観点: フロントマターと本文が正しく分離されることを確認する。
            // Arrange
            var nl = Environment.NewLine;
            var markdown = "---" + nl + "title: test" + nl + "---" + nl + nl + "# Body";

            // Act
            var (frontMatter, body) = _service.Split(markdown);

            // Assert
            Assert.AreEqual(1, frontMatter.Count);
            Assert.AreEqual("test", frontMatter["title"]);
            Assert.AreEqual(nl + "# Body", body);
        }

        [TestMethod]
        public void Join_ShouldCombineFrontMatterAndBody()
        {
            // テスト観点: 辞書と本文が正しく結合されることを確認する。
            // Arrange
            var nl = Environment.NewLine;
            var frontMatter = new Dictionary<string, object> { { "title", "test" } };
            var body = "# Body";

            // Act
            var result = _service.Join(frontMatter, body);

            // Assert
            StringAssert.StartsWith(result, "---" + nl);
            StringAssert.Contains(result, "title: test" + nl);
            StringAssert.Contains(result, nl + "---" + nl + "# Body");
        }

        [TestMethod]
        public void SplitAndJoin_ShouldPreserveStructure_IncludingEmptyLines()
        {
            // テスト観点: 分離して再度結合した際に、構造（空行など）が維持されることを確認する。
            // Arrange
            var nl = Environment.NewLine;
            var markdown = "---" + nl + "title: test" + nl + "---" + nl + nl + "# Content";

            // Act
            var (fm, body) = _service.Split(markdown);
            var result = _service.Join(fm, body);

            // Assert
            StringAssert.Contains(result, "title: test");
            StringAssert.Contains(result, "---" + nl + nl + "# Content");
        }

        [TestMethod]
        public void ExtractHeaders_ShouldReturnH1toH3()
        {
            // テスト観点: H1からH3までの見出しが正しく抽出されることを確認する。
            // Arrange
            var markdown = "# Header 1\n## Header 2\n### Header 3\n#### Header 4\nText";

            // Act
            var result = _service.ExtractHeaders(markdown);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0].Level);
            Assert.AreEqual("Header 1", result[0].Text);
            Assert.AreEqual(2, result[1].Level);
            Assert.AreEqual("Header 2", result[1].Text);
            Assert.AreEqual(3, result[2].Level);
            Assert.AreEqual("Header 3", result[2].Text);
        }

        [TestMethod]
        public void ExtractHeaders_ShouldGenerateIds()
        {
            // テスト観点: 見出しに対応するIDが生成されていることを確認する。
            // MarkdigのAutoIdentifiersはデフォルトで小文字化・ハイフン連結を行うはず
            // Arrange
            var markdown = "# My Header";

            // Act
            var result = _service.ExtractHeaders(markdown);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("my-header", result[0].Id);
        }

        [TestMethod]
        public void ExtractHeaders_ShouldGenerateIdsForJapanese()
        {
            // テスト観点: 日本語の見出しに対してもIDが生成されることを確認する。
            // Arrange
            var markdown = "# 日本語の見出し";

            // Act
            var result = _service.ExtractHeaders(markdown);

            // Assert
            Assert.AreEqual(1, result.Count);
            // MarkdigのAutoIdentifiers(AutoLink)のデフォルトでは日本語はIDにならない場合があるため確認
            // 何らかのIDが生成されていることを期待
            Assert.IsFalse(string.IsNullOrEmpty(result[0].Id), $"ID should not be empty for Japanese headers. Actual: '{result[0].Id}'");
        }

        [TestMethod]
        public void ExtractHeaders_ShouldIncludeLineNumbers()
        {
            // テスト観点: 見出しの行番号が正しく取得されていることを確認する。
            // Arrange
            var markdown = "First line\n\n# Header at line 2\n\n## Header at line 4";

            // Act
            var result = _service.ExtractHeaders(markdown);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0].LineNumber); // Markdigの行番号は0始まり
            Assert.AreEqual(4, result[1].LineNumber);
        }
    }
}
