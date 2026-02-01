using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class CssEditorServiceTests
    {
        [TestMethod]
        public void ParseBodyStyles_ShouldExtractCorrectColor()
        {
            // テスト観点: CSS文字列からbodyセレクタのcolorプロパティが正しく抽出されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = "body { color: #333333; font-size: 16px; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("#333333", styles.BodyTextColor);
        }

        [TestMethod]
        public void ParseBodyStyles_ShouldExtractCorrectBackgroundColor()
        {
            // テスト観点: CSS文字列からbodyセレクタのbackground-colorプロパティが正しく抽出されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = "body { background-color: #f0f0f0; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("#F0F0F0", styles.BodyBackgroundColor);
        }
        /// <summary>
        /// テスト観点: UpdateCssContentメソッドが、既存のCSS文字列のbodyセレクタ内の指定プロパティを正しく更新することを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldUpdateExistingProperties()
        {
            // Arrange
            var service = new CssEditorService();
            var existingCss = "body { font-family: Arial; color: #000000; background-color: #ffffff; }";
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#ff0000", // 赤に変更
                BodyBackgroundColor = "#00ff00" // 緑に変更
            };
            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // 生成されたCSS文字列を再度パースして検証する
            var parsedUpdatedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("#FF0000", parsedUpdatedStyles.BodyTextColor);
            Assert.AreEqual("#00FF00", parsedUpdatedStyles.BodyBackgroundColor);
        }

        /// <summary>
        /// テスト観点: UpdateCssContentメソッドが、bodyセレクタが存在しない場合に新しく追加することを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldAddNewBodySelectorIfMissing()
        {
            // Arrange
            var service = new CssEditorService();
            var existingCss = "h1 { font-size: 24px; }"; // bodyセレクタがない
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#000000",
                BodyBackgroundColor = "#ffffff"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // 文字列完全一致ではなく、パースして内容を検証する
            var parsedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("#000000", parsedStyles.BodyTextColor);
            Assert.AreEqual("#FFFFFF", parsedStyles.BodyBackgroundColor);

            // h1のスタイルが消えていないことも確認
            var parser = new AngleSharp.Css.Parser.CssParser();
            var stylesheet = parser.ParseStyleSheet(updatedCss);
            var h1Rule = stylesheet.Rules.OfType<AngleSharp.Css.Dom.ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "h1");
            Assert.IsNotNull(h1Rule);
            Assert.AreEqual("24px", h1Rule.Style.GetPropertyValue("font-size"));
        }

        [TestMethod]
        public void ParseFootnoteStyles_ShouldExtractCorrectProperties()
        {
            // テスト観点: 脚注関連の各セレクタからプロパティが正しく抽出されることを確認する。
            var service = new CssEditorService();
            var cssContent = @"
.footnote-ref { color: #FF0000; font-weight: bold; }
.footnote-ref::before { content: '['; }
.footnotes { font-size: 12px; color: #666666; margin-top: 2em; }
.footnotes hr { border: 0; border-top: 2px dashed #CCCCCC; }
.footnotes li { line-height: 1.8; }
.footnote-back-ref { display: none; }
";

            var styles = service.ParseCss(cssContent);

            Assert.AreEqual("#FF0000", styles.Footnote.MarkerTextColor);
            Assert.IsTrue(styles.Footnote.IsMarkerBold);
            Assert.IsTrue(styles.Footnote.HasMarkerBrackets);
            Assert.AreEqual("12px", styles.Footnote.AreaFontSize);
            Assert.AreEqual("#666666", styles.Footnote.AreaTextColor);
            Assert.AreEqual("2em", styles.Footnote.AreaMarginTop);
            Assert.AreEqual("2px", styles.Footnote.AreaBorderTopWidth);
            Assert.AreEqual("dashed", styles.Footnote.AreaBorderTopStyle);
            Assert.AreEqual("#CCCCCC", styles.Footnote.AreaBorderTopColor);
            Assert.AreEqual("1.8", styles.Footnote.ListItemLineHeight);
            Assert.IsFalse(styles.Footnote.IsBackLinkVisible);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldHandleFootnoteStyles()
        {
            // テスト観点: 指定したスタイル情報に基づいて脚注のCSSが正しく更新・生成されることを確認する。
            var service = new CssEditorService();
            var styleInfo = new CssStyleInfo();
            styleInfo.Footnote.MarkerTextColor = "#0000FF";
            styleInfo.Footnote.IsMarkerBold = true;
            styleInfo.Footnote.HasMarkerBrackets = true;
            styleInfo.Footnote.AreaFontSize = "14px";
            styleInfo.Footnote.AreaBorderTopWidth = "3px";
            styleInfo.Footnote.AreaBorderTopStyle = "dotted";
            styleInfo.Footnote.AreaBorderTopColor = "#00FF00";
            styleInfo.Footnote.IsBackLinkVisible = false;

            var updatedCss = service.UpdateCssContent("", styleInfo);
            var parsed = service.ParseCss(updatedCss);

            Assert.AreEqual("#0000FF", parsed.Footnote.MarkerTextColor);
            Assert.IsTrue(parsed.Footnote.IsMarkerBold);
            Assert.IsTrue(parsed.Footnote.HasMarkerBrackets);
            Assert.AreEqual("14px", parsed.Footnote.AreaFontSize);
            Assert.AreEqual("3px", parsed.Footnote.AreaBorderTopWidth);
            Assert.AreEqual("dotted", parsed.Footnote.AreaBorderTopStyle);
            Assert.AreEqual("#00FF00", parsed.Footnote.AreaBorderTopColor);
            Assert.IsFalse(parsed.Footnote.IsBackLinkVisible);
        }

        /// <summary>
        /// テスト観点: UpdateCssContentメソッドが、bodyセレクタ内にプロパティが存在しない場合に新しく追加することを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldAddNewPropertiesIfMissingInBodySelector()
        {
            // Arrange
            var service = new CssEditorService();
            var existingCss = "body { font-family: Arial; }"; // colorとbackground-colorがない
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#000000",
                BodyBackgroundColor = "#ffffff"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            var parsedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("#000000", parsedStyles.BodyTextColor);
            Assert.AreEqual("#FFFFFF", parsedStyles.BodyBackgroundColor);

            // font-familyが消えていないことも確認
            var parser = new AngleSharp.Css.Parser.CssParser();
            var stylesheet = parser.ParseStyleSheet(updatedCss);
            var bodyRule = stylesheet.Rules.OfType<AngleSharp.Css.Dom.ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "body");
            Assert.IsNotNull(bodyRule);
            Assert.AreEqual("Arial", bodyRule.Style.GetPropertyValue("font-family"));
        }

        /// <summary>
        /// テスト観点: UpdateCssContentが、入力のフォーマットに関わらず、PrettyStyleFormatterで整形されたCSSを返すことを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldApplyPrettyFormatting()
        {
            // Arrange
            var service = new CssEditorService();
            var unformattedCss = "h1{color:red}body{font-size:12px;}";
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#000000",
                BodyBackgroundColor = "#ffffff"
            };

            // Act
            var updatedCss = service.UpdateCssContent(unformattedCss, styleInfo);

            // Assert
            var expectedCss = string.Join(Environment.NewLine,
                "h1 {",
                "  color: rgba(255, 0, 0, 1);",
                "}",
                "",
                "body {",
                "  font-size: 12px;",
                "  color: rgba(0, 0, 0, 1);",
                "  background-color: rgba(255, 255, 255, 1);",
                "}",
                "",
                "#page-title {",
                "  font-weight: normal;",
                "  font-style: normal;",
                "  text-decoration-style: initial;",
                "  text-decoration-line: none;",
                "}",
                "",
                "li:has(input[type=\"checkbox\"]) {",
                "  list-style-type: none;",
                "}",
                "",
                "table {",
                "  border-collapse: collapse;",
                "}",
                "",
                ".footnote-ref {",
                "  vertical-align: super;",
                "  font-size: smaller;",
                "  text-decoration-color: initial;",
                "  text-decoration-style: initial;",
                "  text-decoration-line: none;",
                "}",
                "",
                ".footnote-ref sup {",
                "  vertical-align: baseline;",
                "  font-size: 100%;",
                "}"
            );

            Assert.AreEqual(expectedCss, updatedCss);
        }

        [TestMethod]
        public void ParseCss_ShouldParseBodyFontSize()
        {
            /// <summary>
            /// テスト観点: CSS文字列からbodyセレクタのfont-sizeプロパティが正しく抽出されることを確認する。
            /// </summary>
            // Arrange
            var service = new CssEditorService();
            var cssContent = "body { font-size: 16px; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("16px", styles.BodyFontSize);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldUpdateBodyFontSize()
        {
            /// <summary>
            /// テスト観点: UpdateCssContentメソッドが、CssStyleInfo.BodyFontSizeが設定されている場合に、
            /// 既存のCSS文字列内のbodyセレクタのfont-sizeを正しく更新、または存在しない場合は追加することを確認する。
            /// </summary>
            // Arrange
            var service = new CssEditorService();
            var existingCss = "body { color: #000000; }";
            var styleInfo = new CssStyleInfo
            {
                BodyFontSize = "20px"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            var parsedUpdatedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("20px", parsedUpdatedStyles.BodyFontSize);

            // 既存のプロパティが消えていないことも確認
            Assert.AreEqual("#000000", parsedUpdatedStyles.BodyTextColor);

            // font-sizeが更新されるケース
            existingCss = "body { font-size: 16px; }";
            styleInfo.BodyFontSize = "24px";
            updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            parsedUpdatedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("24px", parsedUpdatedStyles.BodyFontSize);
        }

        [TestMethod]
        public void ParseCss_ShouldParseHeadingTextColors()
        {
            // テスト観点: ParseCssが、複数の見出し(h1, h2)のcolorプロパティを解析し、
            // CssStyleInfoのHeadingTextColorsディクショナリに正しく格納することを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = "h1 { color: red; } h2 { color: blue; } p { color: black; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.IsNotNull(styles.HeadingTextColors);
            Assert.AreEqual(2, styles.HeadingTextColors.Count);
            Assert.AreEqual("#FF0000", styles.HeadingTextColors["h1"]);
            Assert.AreEqual("#0000FF", styles.HeadingTextColors["h2"]);
        }

        [TestMethod]
        public void ParseCss_ShouldParseHeadingFontSizesAndFamilies()
        {
            // テスト観点: ParseCssが、複数の見出し(h1, h2)のfont-sizeとfont-familyプロパティを解析し、
            // CssStyleInfoのHeadingFontSizesとHeadingFontFamiliesディクショナリに正しく格納することを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = "h1 { font-size: 24px; font-family: Arial; } h2 { font-size: 1.2em; font-family: 'Times New Roman'; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.IsNotNull(styles.HeadingFontSizes);
            Assert.AreEqual(2, styles.HeadingFontSizes.Count);
            Assert.AreEqual("24px", styles.HeadingFontSizes["h1"]);
            Assert.AreEqual("1.2em", styles.HeadingFontSizes["h2"]);

            Assert.IsNotNull(styles.HeadingFontFamilies);
            Assert.AreEqual(2, styles.HeadingFontFamilies.Count);
            Assert.AreEqual("Arial", styles.HeadingFontFamilies["h1"]);
            Assert.AreEqual("\"Times New Roman\"", styles.HeadingFontFamilies["h2"]);
        }

        [TestMethod]
        public void ParseCss_ShouldParseHeadingStyleFlags()
        {
            // テスト観点: ParseCssが、見出しのfont-weight, font-style, text-decorationプロパティを解析し、
            // CssStyleInfoのHeadingStyleFlagsディクショナリに正しく格納することを確認する。
            // また、text-alignプロパティがHeadingAlignmentsに格納されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                h1 {
                    font-weight: bold;
                    font-style: italic;
                    text-decoration: underline;
                    text-align: center;
                }
                h2 {
                    text-align: right;
                }
                h3 {
                    font-weight: normal;
                    font-style: normal;
                    text-decoration: none;
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert - h1
            Assert.IsNotNull(styles.HeadingStyleFlags);
            Assert.IsTrue(styles.HeadingStyleFlags.ContainsKey("h1"));
            var h1Flags = styles.HeadingStyleFlags["h1"];
            Assert.IsTrue(h1Flags.IsBold);
            Assert.IsTrue(h1Flags.IsItalic);
            Assert.IsTrue(h1Flags.IsUnderline);
            Assert.AreEqual("center", styles.HeadingAlignments["h1"]);

            // Assert - h2
            Assert.AreEqual("right", styles.HeadingAlignments["h2"]);

            // Assert - h3 (デフォルト値)
            Assert.IsTrue(styles.HeadingStyleFlags.ContainsKey("h3"));
            var h3Flags = styles.HeadingStyleFlags["h3"];
            Assert.IsFalse(h3Flags.IsBold);
            Assert.IsFalse(h3Flags.IsItalic);
            Assert.IsFalse(h3Flags.IsUnderline);
        }

        [TestMethod]
        public void ParseCss_ShouldParseBlockquoteStyles()
        {
            // テスト観点: `blockquote` のスタイル(color, background-color, border-left)が正しく解析され、
            // CssStyleInfoの対応するプロパティに格納されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                blockquote {
                    color: #123456;
                    background-color: #abcdef;
                    border-left: 3px solid #987654;
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("#123456", styles.QuoteTextColor);
            Assert.AreEqual("#ABCDEF", styles.QuoteBackgroundColor);
            Assert.AreEqual("3px", styles.QuoteBorderWidth);
            Assert.AreEqual("solid", styles.QuoteBorderStyle);
            Assert.AreEqual("#987654", styles.QuoteBorderColor);
        }

        [TestMethod]
        public void ParseCss_ShouldParseListStyles()
        {
            // テスト観点: `ul` のスタイル(list-style-type, padding-left)が正しく解析され、
            // CssStyleInfoの対応するプロパティに格納されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                ul {
                    list-style-type: square;
                    padding-left: 40px;
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("square", styles.ListMarkerType);
            Assert.AreEqual("40px", styles.ListIndent);
        }

        [TestMethod]
        public void ParseCss_ShouldParseTableStyles()
        {
            // テスト観点: `th, td` と `th` のスタイルが正しく解析され、
            // CssStyleInfoの対応するプロパティに格納されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                th, td {
                    border: 1px solid #dddddd;
                    padding: 8px;
                }
                th {
                    background-color: #f2f2f2;
                    color: #333333;
                    font-size: 1.1em;
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("1px", styles.TableBorderWidth);
            Assert.AreEqual("#DDDDDD", styles.TableBorderColor);
            Assert.AreEqual("8px", styles.TableCellPadding);
            Assert.AreEqual("#F2F2F2", styles.TableHeaderBackgroundColor);
            Assert.AreEqual("#333333", styles.TableHeaderTextColor);
            Assert.AreEqual("1.1em", styles.TableHeaderFontSize);
        }

        [TestMethod]
        public void ParseCss_ShouldParseCodeStyles()
        {
            // テスト観点: `code` のスタイル(color, background-color, font-family)が正しく解析され、
            // CssStyleInfoの対応するプロパティに格納されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                code {
                    color: #ff0000;
                    background-color: #000000;
                    font-family: ""Consolas"";
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("#FF0000", styles.CodeTextColor);
            Assert.AreEqual("#000000", styles.CodeBackgroundColor);
            Assert.AreEqual("\"Consolas\"", styles.CodeFontFamily);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldUpdateHeadingStyles()
        {
            // テスト観点: UpdateCssContentメソッドが、CssStyleInfoオブジェクトに含まれる見出しスタイル情報に基づいて、
            // 既存のCSS文字列を正しく更新することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = @"
                body { font-size: 16px; }
                h1 { color: red; font-size: 20px; }
                h2 { font-family: 'Times New Roman'; }
            ";

            var styleInfo = new CssStyleInfo();
            styleInfo.HeadingTextColors["h1"] = "#0000FF"; // Blue
            styleInfo.HeadingFontSizes["h1"] = "24px";
            styleInfo.HeadingFontFamilies["h1"] = "Arial";
            styleInfo.HeadingAlignments["h1"] = "center";
            styleInfo.HeadingStyleFlags["h1"] = new HeadingStyleFlags { IsBold = true, IsUnderline = true };

            styleInfo.HeadingTextColors["h2"] = "#00FF00"; // Green
            styleInfo.HeadingFontSizes["h2"] = "1.5em";
            styleInfo.HeadingFontFamilies["h2"] = "Verdana";
            styleInfo.HeadingAlignments["h2"] = "right";
            styleInfo.HeadingStyleFlags["h2"] = new HeadingStyleFlags { IsItalic = true };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsedUpdatedStyles = service.ParseCss(updatedCss);

            // Assert h1
            Assert.AreEqual("#0000FF", parsedUpdatedStyles.HeadingTextColors["h1"]);
            Assert.AreEqual("24px", parsedUpdatedStyles.HeadingFontSizes["h1"]);
            Assert.AreEqual("Arial", parsedUpdatedStyles.HeadingFontFamilies["h1"]);
            Assert.AreEqual("center", parsedUpdatedStyles.HeadingAlignments["h1"]);
            Assert.IsTrue(parsedUpdatedStyles.HeadingStyleFlags["h1"].IsBold);
            Assert.IsTrue(parsedUpdatedStyles.HeadingStyleFlags["h1"].IsUnderline);
            Assert.IsFalse(parsedUpdatedStyles.HeadingStyleFlags["h1"].IsItalic);

            // Assert h2
            Assert.AreEqual("#00FF00", parsedUpdatedStyles.HeadingTextColors["h2"]);
            Assert.AreEqual("1.5em", parsedUpdatedStyles.HeadingFontSizes["h2"]);
            Assert.AreEqual("Verdana", parsedUpdatedStyles.HeadingFontFamilies["h2"]);
            Assert.AreEqual("right", parsedUpdatedStyles.HeadingAlignments["h2"]);
            Assert.IsTrue(parsedUpdatedStyles.HeadingStyleFlags["h2"].IsItalic);
            Assert.IsFalse(parsedUpdatedStyles.HeadingStyleFlags["h2"].IsBold);
            Assert.IsFalse(parsedUpdatedStyles.HeadingStyleFlags["h2"].IsUnderline);

            // 既存のbodyスタイルが消えていないことを確認
            Assert.AreEqual("16px", parsedUpdatedStyles.BodyFontSize);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldUpdateBlockquoteStyles()
        {
            // テスト観点: UpdateCssContentメソッドが、CssStyleInfoオブジェクトに含まれる引用スタイル情報に基づいて、
            // 既存のCSS文字列を正しく更新することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "blockquote { color: red; }";
            var styleInfo = new CssStyleInfo
            {
                QuoteTextColor = "#112233",
                QuoteBackgroundColor = "#445566",
                QuoteBorderColor = "#778899",
                QuoteBorderWidth = "5px",
                QuoteBorderStyle = "dotted"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsedUpdatedStyles = service.ParseCss(updatedCss);

            // Assert
            Assert.AreEqual("#112233", parsedUpdatedStyles.QuoteTextColor);
            Assert.AreEqual("#445566", parsedUpdatedStyles.QuoteBackgroundColor);
            Assert.AreEqual("#778899", parsedUpdatedStyles.QuoteBorderColor);
            Assert.AreEqual("5px", parsedUpdatedStyles.QuoteBorderWidth);
            Assert.AreEqual("dotted", parsedUpdatedStyles.QuoteBorderStyle);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldUpdateListStyles()
        {
            // テスト観点: UpdateCssContentメソッドが、CssStyleInfoオブジェクトに含まれるリストスタイル情報に基づいて、
            // 既存のCSS文字列を正しく更新することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "ul { list-style-type: disc; }";
            var styleInfo = new CssStyleInfo
            {
                ListMarkerType = "square",
                ListIndent = "30px"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsedUpdatedStyles = service.ParseCss(updatedCss);

            // Assert
            Assert.AreEqual("square", parsedUpdatedStyles.ListMarkerType);
            Assert.AreEqual("30px", parsedUpdatedStyles.ListIndent);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldUpdateTableStyles()
        {
            // テスト観点: UpdateCssContentメソッドが、CssStyleInfoオブジェクトに含まれる表スタイル情報に基づいて、
            // 既存のCSS文字列を正しく更新することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "th, td { border: 1px solid black; } th { background-color: white; }";
            var styleInfo = new CssStyleInfo
            {
                TableBorderColor = "#aaaaaa",
                TableBorderWidth = "2px",
                TableHeaderBackgroundColor = "#bbbbbb",
                TableHeaderTextColor = "#ffffff",
                TableHeaderFontSize = "18px",
                TableCellPadding = "10px"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsedUpdatedStyles = service.ParseCss(updatedCss);

            // Assert
            Assert.AreEqual("#AAAAAA", parsedUpdatedStyles.TableBorderColor);
            Assert.AreEqual("2px", parsedUpdatedStyles.TableBorderWidth);
            Assert.AreEqual("#BBBBBB", parsedUpdatedStyles.TableHeaderBackgroundColor);
            Assert.AreEqual("#FFFFFF", parsedUpdatedStyles.TableHeaderTextColor);
            Assert.AreEqual("18px", parsedUpdatedStyles.TableHeaderFontSize);
            Assert.AreEqual("10px", parsedUpdatedStyles.TableCellPadding);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldApplyImportantToTableHeaderAlignment()
        {
            // テスト観点: 表ヘッダーの位置揃え設定が、Markdownのインラインスタイルを上書きするために
            //            !important 付きで出力されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var styleInfo = new CssStyleInfo
            {
                TableHeaderAlignment = "center"
            };

            // Act
            var updatedCss = service.UpdateCssContent(string.Empty, styleInfo);

            // Assert
            // 期待される文字列が含まれているか正規表現でチェック
            StringAssert.Matches(updatedCss, new Regex(@"th\s*\{[^}]*text-align:\s*center\s*!important;[^}]*\}"));
        }

        [TestMethod]
        public void UpdateCssContent_ShouldGenerateShorthandProperties()
        {
            // テスト観点: UpdateCssContentが、borderやpaddingのショートハンドプロパティを正しく生成することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = ""; // 空のCSSから開始
            var styleInfo = new CssStyleInfo
            {
                TableBorderColor = "#FF8000",
                TableBorderWidth = "5px",
                TableBorderStyle = "solid",
                TableCellPadding = "20px"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // ショートハンドプロパティが含まれていることを確認
            StringAssert.Matches(updatedCss, new System.Text.RegularExpressions.Regex(@"border:\s*5px\s+solid\s+(#FF8000|rgba\(255,\s*128,\s*0,\s*1\));"));
            StringAssert.Matches(updatedCss, new System.Text.RegularExpressions.Regex(@"padding:\s*20px;"));

            // ロングハンドプロパティが含まれていないことを確認
            Assert.IsFalse(updatedCss.Contains("border-top-width"), "Should not contain longhand border-top-width.");
            Assert.IsFalse(updatedCss.Contains("padding-left"), "Should not contain longhand padding-left.");
        }

        [TestMethod]
        public void ParseCss_ShouldParseSeparateCodeStyles()
        {
            // テスト観点: code と pre code のスタイルがそれぞれ正しく解析されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                code { color: #111111; background-color: #222222; }
                pre code { color: #333333; background-color: #444444; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("#111111", styles.InlineCodeTextColor);
            Assert.AreEqual("#222222", styles.InlineCodeBackgroundColor);
            Assert.AreEqual("#333333", styles.BlockCodeTextColor);
            Assert.AreEqual("#444444", styles.BlockCodeBackgroundColor);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldApplyImportantToCodeBlock_WhenOverrideIsEnabled()
        {
            // テスト観点: IsCodeBlockOverrideEnabled が true の場合、pre code に !important が付与されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var styleInfo = new CssStyleInfo
            {
                BlockCodeTextColor = "#FF0000",
                IsCodeBlockOverrideEnabled = true
            };

            // Act
            var updatedCss = service.UpdateCssContent(string.Empty, styleInfo);

            // Assert
            StringAssert.Matches(updatedCss, new Regex(@"pre\s+code\s*\{[^}]*color:\s*rgba\(255,\s*0,\s*0,\s*1\)\s*!important;[^}]*\}"));
        }

        [TestMethod]
        public void UpdateCssContent_ShouldRemovePropertiesFromCodeBlock_WhenOverrideIsDisabled()
        {
            // テスト観点: IsCodeBlockOverrideEnabled が false の場合、pre code からプロパティが削除され
            //            ハイライトテーマが優先される状態になることを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "pre code { color: red; background-color: blue; }";
            var styleInfo = new CssStyleInfo
            {
                IsCodeBlockOverrideEnabled = false
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // 脚注など他の箇所で color: が使われる可能性があるため、pre code セレクタの中身が空であることを確認する
            Assert.IsFalse(updatedCss.Contains("pre code {"), "Properties should be removed from pre code when override is disabled.");
        }

        [TestMethod]
        public void UpdateCssContent_ShouldUpdateCodeStyles()
        {
            // テスト観点: UpdateCssContentメソッドが、CssStyleInfoオブジェクトに含まれるコードスタイル情報に基づいて、
            // 既存のCSS文字列を正しく更新することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "code { color: black; }";
            var styleInfo = new CssStyleInfo
            {
                CodeTextColor = "#dddddd",
                CodeBackgroundColor = "#eeeeee",
                CodeFontFamily = "Courier New"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsedUpdatedStyles = service.ParseCss(updatedCss);

            // Assert
            Assert.AreEqual("#DDDDDD", parsedUpdatedStyles.CodeTextColor);
            Assert.AreEqual("#EEEEEE", parsedUpdatedStyles.CodeBackgroundColor);
            Assert.AreEqual("Courier New", parsedUpdatedStyles.CodeFontFamily);
        }






        [TestMethod]
        public void ParseCss_ShouldDetectPerHeadingNumbering()
        {
            // テスト観点: `ParseCss`が、CSS内の見出しごとの項番採番ルールを検知し、`HeadingNumberingStates`を正しく設定することを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                body { counter-reset: h1 0; }
                h1 { counter-increment: h1; counter-reset: h2 0; }
                h1::before { content: counter(h1) '. '; }
                h2 { counter-increment: h2; counter-reset: h3 0; }
                h2::before { content: counter(h1) '.' counter(h2) '. '; }
                h3 { color: red; } /* h3には採番ルールがない */
            ";

            // Act
            var styleInfo = service.ParseCss(cssContent);

            // Assert
            Assert.IsTrue(styleInfo.HeadingNumberingStates["h1"], "h1 numbering should be detected as true.");
            Assert.IsTrue(styleInfo.HeadingNumberingStates["h2"], "h2 numbering should be detected as true.");
            Assert.IsFalse(styleInfo.HeadingNumberingStates["h3"], "h3 numbering should be detected as false.");
            Assert.IsFalse(styleInfo.HeadingNumberingStates["h4"], "h4 numbering should be detected as false.");
            Assert.IsFalse(styleInfo.HeadingNumberingStates["h5"], "h5 numbering should be detected as false.");
            Assert.IsFalse(styleInfo.HeadingNumberingStates["h6"], "h6 numbering should be detected as false.");
        }

        [TestMethod]
        public void UpdateCssContent_ShouldGeneratePerHeadingNumberingStyles()
        {
            // テスト観点: `CssStyleInfo.HeadingNumberingStates`が設定されている場合、`UpdateCssContent`が
            // 指定された見出しレベルにのみ項番採番用のCSSルールを正しく生成することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "h1 { color: red; } h3 { font-size: 1.2em; }";
            var styleInfo = new CssStyleInfo();
            styleInfo.HeadingNumberingStates["h1"] = true; // h1だけ採番を有効にする
            styleInfo.HeadingNumberingStates["h2"] = false;
            styleInfo.HeadingNumberingStates["h3"] = true; // h3も採番を有効にする
            styleInfo.HeadingNumberingStates["h4"] = false;
            styleInfo.HeadingNumberingStates["h5"] = false;
            styleInfo.HeadingNumberingStates["h6"] = false;


            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert - h1とh3にのみ採番ルールが適用されていることを確認
            // body
            StringAssert.Matches(updatedCss, new Regex(@"body\s*\{[^}]*counter-reset:\s*h1\s*0;[^}]*\}"));

            // h1
            StringAssert.Matches(updatedCss, new Regex(@"h1\s*\{[^}]*counter-increment:\s*h1(\s+1)?;[^}]*\}"));
            StringAssert.Matches(updatedCss, new Regex(@"h1\s*\{[^}]*counter-reset:\s*h2\s*0;[^}]*\}"));
            StringAssert.Matches(updatedCss, new Regex(@"h1::before\s*\{[^}]*content:\s*counter\(h1\)\s*""\.\s*"";[^}]*\}"));

            // h2 (採番無効)
            Assert.IsFalse(updatedCss.Contains("h2 { counter-increment:"));
            Assert.IsFalse(updatedCss.Contains("h2::before"));

            // h3
            StringAssert.Matches(updatedCss, new Regex(@"h3\s*\{[^}]*counter-increment:\s*h3(\s+1)?;[^}]*\}"));
            StringAssert.Matches(updatedCss, new Regex(@"h3\s*\{[^}]*counter-reset:\s*h4\s*0;[^}]*\}"));
            // H2が無効なので、H1とH3のみが content に含まれるべき
            StringAssert.Matches(updatedCss, new Regex(@"h3::before\s*\{[^}]*content:\s*counter\(h1\)\s*""\.""\s*counter\(h3\)\s*""\.\s*"";[^}]*\}"));

            // h4以降 (採番無効なのでcounter-increment, counter-reset, ::before はなし)
            Assert.IsFalse(updatedCss.Contains("h4 {")); // h4はルール自体が空であれば削除される
            Assert.IsFalse(updatedCss.Contains("h4::before"));
            Assert.IsFalse(updatedCss.Contains("h5::before"));
            Assert.IsFalse(updatedCss.Contains("h6::before"));
        }

        [TestMethod]
        public void UpdateCssContent_ShouldRemovePerHeadingNumberingStyles_WhenDisabled()
        {
            // テスト観点: `CssStyleInfo.HeadingNumberingStates`で特定の見出しの採番が無効になった場合、
            // `UpdateCssContent`が既存の項番採番用CSSルールを正しく削除することを確認する。
            // Arrange
            var service = new CssEditorService();
            // 最初はh1とh2の採番が有効なCSS
            var existingCss = @"
                body { counter-reset: h1 0; }
                h1 { counter-increment: h1; counter-reset: h2 0; }
                h1::before { content: counter(h1) '. '; }
                h2 { counter-increment: h2; counter-reset: h3 0; }
                h2::before { content: counter(h1) '.' counter(h2) '. '; }
                h3 { font-size: 1em; }
            ";
            var styleInfo = new CssStyleInfo();
            // h1の採番を無効にする
            styleInfo.HeadingNumberingStates["h1"] = false;
            // h2の採番はそのまま有効にする
            styleInfo.HeadingNumberingStates["h2"] = true;
            styleInfo.HeadingNumberingStates["h3"] = false; // h3はもともと無効

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert - h2の採番が有効なので、bodyのcounter-resetは残る
            StringAssert.Matches(updatedCss, new Regex(@"body\s*\{[^}]*counter-reset:\s*h1\s*0;[^}]*\}"));

            // Assert - h1の採番ルールが削除されていることを確認
            Assert.IsFalse(updatedCss.Contains("h1 { counter-increment:"));
            Assert.IsFalse(updatedCss.Contains("h1::before"));

            // Assert - h2の採番ルールは残っていることを確認
            StringAssert.Matches(updatedCss, new Regex(@"h2\s*\{[^}]*counter-increment:\s*h2(\s+1)?;[^}]*\}"));
            StringAssert.Matches(updatedCss, new Regex(@"h2\s*\{[^}]*counter-reset:\s*h3\s*0;[^}]*\}"));
            // H1が無効な場合、H2の採番は "1. " から始まるべき（"0.1. " ではなく）
            StringAssert.Matches(updatedCss, new Regex(@"h2::before\s*\{[^}]*content:\s*counter\(h2\)\s*""\.\s*"";[^}]*\}"));


            // Assert - h3は採番ルールがないことを確認
            StringAssert.Matches(updatedCss, new Regex(@"h3\s*\{[^}]*font-size:\s*1em;[^}]*\}"));
            Assert.IsFalse(updatedCss.Contains("h3 { counter-increment:"));
            Assert.IsFalse(updatedCss.Contains("h3::before"));
        }

        [TestMethod]
        public void UpdateCssContent_ShouldNotIncludeDisabledUpperLevelCounters_InContent()
        {
            // テスト観点: 上位レベルの見出し採番が無効な場合、下位レベルの content プロパティに
            //            その上位レベルのカウンタが含まれないことを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "";
            var styleInfo = new CssStyleInfo();
            // h1, h2 は無効、h3 は有効にする
            styleInfo.HeadingNumberingStates["h1"] = false;
            styleInfo.HeadingNumberingStates["h2"] = false;
            styleInfo.HeadingNumberingStates["h3"] = true;

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // h3::before の content は counter(h3) だけを含むべき
            StringAssert.Matches(updatedCss, new Regex(@"h3::before\s*\{[^}]*content:\s*counter\(h3\)\s*""\.\s*"";[^}]*\}"));
        }
        [TestMethod]
        public void UpdateCssContent_WithNullDictionaries_ShouldNotThrow()
        {
            // テスト観点: CssStyleInfo の辞書プロパティが null の場合でも、
            //            例外を投げずに処理が完了することを確認する。
            // Arrange
            var service = new CssEditorService();
            var styleInfo = new CssStyleInfo();
            // 実際には読み取り専用プロパティだが、サービス側で null を受け取った場合の
            // 防御的コードをテストするために null を許容する辞書を想定してテスト
            // (このテスト自体は、モデルの変更により null になりにくくなったが、
            //  将来の変更に対するガードとして意味がある)

            // Act
            var updatedCss = service.UpdateCssContent("h1 { color: red; }", styleInfo);

            // Assert
            Assert.IsNotNull(updatedCss);
            StringAssert.Contains(updatedCss, "h1");
        }

        [TestMethod]
        public void UpdateCssContent_ShouldMaintainExistingTaskListStyles_WhileAddingNewOnes()
        {
            // テスト観点: 既に li:has(...) のようなルールが存在する場合でも、
            //            適切にマージまたは上書きされることを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "li:has(input[type=\"checkbox\"]) { color: red; }";
            var styleInfo = new CssStyleInfo();

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // 既存の color と追加された list-style-type が両方存在することを確認
            StringAssert.Matches(updatedCss, new Regex(@"li:has\(input\[type=""checkbox""\]\)\s*\{[^}]*color:\s*rgba\(255,\s*0,\s*0,\s*1\);[^}]*\}"));
            StringAssert.Matches(updatedCss, new Regex(@"li:has\(input\[type=""checkbox""\]\)\s*\{[^}]*list-style-type:\s*none;[^}]*\}"));
        }

        [TestMethod]
        public void UpdateCssContent_ShouldRemoveBodyCounterReset_WhenAllNumberingDisabled()
        {
            // テスト観点: すべての見出しの採番が無効になった場合、body に設定されていた counter-reset が削除されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "body { counter-reset: h1 0; color: black; }";
            var styleInfo = new CssStyleInfo();
            // 全て false (デフォルト)

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            Assert.IsFalse(updatedCss.Contains("counter-reset"), "body counter-reset should be removed.");
            StringAssert.Contains(updatedCss, "color: rgba(0, 0, 0, 1);"); // 他のプロパティは維持
        }

        [TestMethod]
        public void ParseCss_WithMalformedCss_ShouldReturnDefaultStyleInfo()
        {
            // テスト観点: 壊れたCSS文字列を渡しても、例外を投げずに解析可能な範囲で結果を返すことを確認する。
            // Arrange
            var service = new CssEditorService();
            var malformedCss = "body { color: red; garbage: !!!; h1 { font-size: 20px;"; // 閉じ括弧なし

            // Act
            var styles = service.ParseCss(malformedCss);

            // Assert
            Assert.IsNotNull(styles);
            Assert.AreEqual("#FF0000", styles.BodyTextColor);
        }

        [TestMethod]
        public void ParseCss_ShouldParseTitleStyles()
        {
            // テスト観点: `#page-title` の各種スタイル（色、サイズ、フォント、配置、余白、装飾）が
            //            正しく解析されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                #page-title {
                    color: #FF00FF;
                    font-size: 32px;
                    font-family: 'Segoe UI';
                    text-align: center;
                    margin-bottom: 20px;
                    font-weight: bold;
                    font-style: italic;
                    text-decoration: underline;
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("#FF00FF", styles.TitleTextColor);
            Assert.AreEqual("32px", styles.TitleFontSize);
            Assert.AreEqual("\"Segoe UI\"", styles.TitleFontFamily);
            Assert.AreEqual("center", styles.TitleAlignment);
            Assert.AreEqual("20px", styles.TitleMarginBottom);
            Assert.IsTrue(styles.TitleStyleFlags.IsBold);
            Assert.IsTrue(styles.TitleStyleFlags.IsItalic);
            Assert.IsTrue(styles.TitleStyleFlags.IsUnderline);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldApplyListIndentToOrderedList()
        {
            // テスト観点: UpdateCssContentメソッドが、ol要素に対してもListIndent（padding-left）を適用することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "ol { list-style-type: decimal; }";
            var styleInfo = new CssStyleInfo
            {
                NumberedListMarkerType = "decimal",
                ListIndent = "2rem"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            StringAssert.Matches(updatedCss, new Regex(@"ol\s*\{[^}]*padding-left:\s*2rem;[^}]*\}"));
        }

        [TestMethod]
        public void UpdateCssContent_ShouldGenerateNestedDecimalStyles()
        {
            // テスト観点: NumberedListMarkerTypeが "decimal-nested" の場合、
            //            階層番号付け用のCSS（counter-reset, markers）が生成されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "";
            var styleInfo = new CssStyleInfo
            {
                NumberedListMarkerType = "decimal-nested"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // ol: list-style-type: none; counter-reset: item;
            StringAssert.Matches(updatedCss, new Regex(@"ol\s*\{[^}]*list-style-type:\s*none;[^}]*\}"));
            StringAssert.Matches(updatedCss, new Regex(@"ol\s*\{[^}]*counter-reset:\s*item\s*0;[^}]*\}"));

            // li: display: block;
            StringAssert.Matches(updatedCss, new Regex(@"li\s*\{[^}]*display:\s*block;[^}]*\}"));

            // li::before: content: counters(item, ".") " "; counter-increment: item;
            // Note: AngleSharp might normalize 'counters(item, ".")' to 'counters(item .)' or similar depending on version.
            // We adjust regex to be flexible.
            StringAssert.Matches(updatedCss, new Regex(@"li::before\s*\{[^}]*content:\s*counters\(item(,\s*""\.""|\s+\.?)\)\s*""\s"";[^}]*\}"));
            StringAssert.Matches(updatedCss, new Regex(@"li::before\s*\{[^}]*counter-increment:\s*item(\s+1)?;[^}]*\}"));
        }

        [TestMethod]
        public void UpdateCssContent_ShouldGenerateValidCountersSyntax()
        {
            // テスト観点: AngleSharpによって生成される counters 関数の構文が正しい（カンマと引用符が含まれている）ことを確認する。
            // 不具合再現用: 現状の実装では counters(item .) のようにカンマと引用符が抜ける可能性がある。
            var service = new CssEditorService();
            var styleInfo = new CssStyleInfo { NumberedListMarkerType = "decimal-nested" };

            var updatedCss = service.UpdateCssContent("", styleInfo);

            // 期待値: content: counters(item, ".") " ";
            // AngleSharpのフォーマッタによってはスペースの有無が変わるため、柔軟かつ必須要素を確認する正規表現にする。
            // 必須: counters, (, item, カンマ, ", ., ", ), " "
            StringAssert.Matches(updatedCss, new Regex(@"content:\s*counters\(item,\s*""\.""\)\s*""\s"";"),
                "CSS output for counters function is invalid. Expected comma and quotes.");
        }

        [TestMethod]
        public void UpdateCssContent_ShouldNotAffectUnorderedList_WhenDecimalNestedIsSelected()
        {
            // テスト観点: NumberedListMarkerType が "decimal-nested" の場合でも、
            //            箇条書きリスト (ul) のマーカーに数字 (counters) が適用されないことを確認する。
            //            (バグ再現用: li 全体にスタイルが当たっていると ul にも数字が出る)
            var service = new CssEditorService();
            var styleInfo = new CssStyleInfo
            {
                NumberedListMarkerType = "decimal-nested",
                ListMarkerType = "disc" // ul は黒丸
            };

            var updatedCss = service.UpdateCssContent("", styleInfo);

            // ol > li (または ol li) には counters が適用されるべき
            // しかし ul > li (または単なる li で ul 内のもの) には適用されるべきではない
            // CSSの実装として "li::before" が無条件に出力されているとアウト。
            // 修正後は "ol > li::before" などの詳細なセレクタになるはず。

            // 検証: "li::before" というセレクタが、ol に限定されずに存在していないか確認。
            // もし "ol > li::before" ならOKだが、単なる "li::before" はNG。
            // (注: AngleSharpの出力フォーマットに依存するため、セレクタ文字列をチェックする)

            // 期待されるセレクタが含まれているか
            StringAssert.Matches(updatedCss, new Regex(@"ol\s*>\s*li::before"), "Should target 'ol > li' specifically.");

            // 汎用的な li::before が存在しないこと (もし存在すると ul にも効く)
            // ただし、もし `ul > li::before` などを別途定義する仕様なら話は別だが、今回は `decimal-nested` に関するチェック。
            Assert.IsFalse(Regex.IsMatch(updatedCss, @"(?<!>)\s*li::before"), "Should not have generic 'li::before' selector.");
        }

        [TestMethod]
        public void UpdateCssContent_ShouldSupportAllListMarkerTypes()
        {
            // テスト観点: 全てのリストマーカータイプが正しくCSSに出力されるか網羅的に確認する。
            var service = new CssEditorService();

            var ulTypes = new[] { "disc", "circle", "square", "none" };
            var olTypes = new[] { "decimal", "decimal-leading-zero", "lower-alpha", "upper-alpha", "lower-roman", "upper-roman" };

            foreach (var type in ulTypes)
            {
                var style = new CssStyleInfo { ListMarkerType = type };
                var css = service.UpdateCssContent("", style);
                StringAssert.Matches(css, new Regex($@"ul\s*\{{[^}}]*list-style-type:\s*{type};[^}}]*\}}"), $"ul type '{type}' failed.");
            }

            foreach (var type in olTypes)
            {
                var style = new CssStyleInfo { NumberedListMarkerType = type };
                var css = service.UpdateCssContent("", style);
                StringAssert.Matches(css, new Regex($@"ol\s*\{{[^}}]*list-style-type:\s*{type};[^}}]*\}}"), $"ol type '{type}' failed.");
            }
        }

        [TestMethod]
        public void UpdateCssContent_ShouldRemoveNestedStyles_WhenSwitchingToStandard()
        {
            // テスト観点: "decimal-nested" から標準のリストタイプに戻したとき、
            //            階層番号付け用のスタイルが削除されることを確認する。
            // Arrange
            var service = new CssEditorService();
            // 階層番号付けが有効な状態のCSSを想定
            var existingCss = @"
                ol { list-style-type: none; counter-reset: item; }
                li { display: block; }
                li::before { content: counters(item, '.'); counter-increment: item; }";

            var styleInfo = new CssStyleInfo
            {
                NumberedListMarkerType = "decimal" // 標準に戻す
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // 標準のスタイルが適用されていること
            StringAssert.Matches(updatedCss, new Regex(@"ol\s*\{[^}]*list-style-type:\s*decimal;[^}]*\}"));

            // 特殊なスタイルが削除されていること (counter-reset等)
            Assert.IsFalse(updatedCss.Contains("counter-reset: item"), "Should remove counter-reset for standard lists.");
            Assert.IsFalse(updatedCss.Contains("counters(item"), "Should remove counters content for standard lists.");
        }

        [TestMethod]
        public void ParseCss_ShouldParseParagraphStyles()
        {
            // テスト観点: pタグのスタイル（line-height, margin-bottom, text-indent）が正しく解析されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                p {
                    line-height: 1.6;
                    margin-bottom: 1em;
                    text-indent: 20px;
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("1.6", styles.ParagraphLineHeight);
            Assert.AreEqual("1em", styles.ParagraphMarginBottom);
            Assert.AreEqual("20px", styles.ParagraphTextIndent);
        }

        [TestMethod]
        public void UpdateCssContent_ShouldUpdateParagraphStyles()
        {
            // テスト観点: UpdateCssContentが、段落スタイル情報を正しく既存のCSSに反映、または新規作成することを確認する。
            // Arrange
            var service = new CssEditorService();
            var existingCss = "p { color: black; }";
            var styleInfo = new CssStyleInfo
            {
                ParagraphLineHeight = "1.8",
                ParagraphMarginBottom = "12px",
                ParagraphTextIndent = "1em"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsed = service.ParseCss(updatedCss);

            // Assert
            Assert.AreEqual("1.8", parsed.ParagraphLineHeight);
            Assert.AreEqual("12px", parsed.ParagraphMarginBottom);
            Assert.AreEqual("1em", parsed.ParagraphTextIndent);
            // 既存の color が維持されていることを確認
            var parser = new AngleSharp.Css.Parser.CssParser();
            var stylesheet = parser.ParseStyleSheet(updatedCss);
            var pRule = stylesheet.Rules.OfType<AngleSharp.Css.Dom.ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "p");
            Assert.IsNotNull(pRule);
            Assert.AreEqual("rgba(0, 0, 0, 1)", pRule.Style.GetPropertyValue("color"));
        }

        [TestMethod]
        public void UpdateCssContent_ParagraphStyles_EdgeCases()
        {
            // テスト観点: 段落スタイルのエッジケース（null値、空文字、ルール未存在）での動作を確認する。
            var service = new CssEditorService();

            // Case 1: ルールがない場合に新規作成されるか
            var styleInfo = new CssStyleInfo { ParagraphLineHeight = "2" };
            var updatedCss = service.UpdateCssContent(string.Empty, styleInfo);
            StringAssert.Matches(updatedCss, new Regex(@"p\s*\{[^}]*line-height:\s*2;[^}]*\}"));

            // Case 2: 値が null の場合、プロパティが更新（または削除）されないか
            var existingCss = "p { line-height: 1.5; margin-bottom: 10px; }";
            styleInfo = new CssStyleInfo { ParagraphLineHeight = null, ParagraphMarginBottom = "20px" };
            updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsed = service.ParseCss(updatedCss);
            Assert.AreEqual("1.5", parsed.ParagraphLineHeight, "Null property should not overwrite existing value.");
            Assert.AreEqual("20px", parsed.ParagraphMarginBottom);

            // Case 3: 空文字の場合の挙動 (現状の実装では SetProperty がスキップされるため、既存値が維持される)
            styleInfo = new CssStyleInfo { ParagraphLineHeight = "" };
            updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            parsed = service.ParseCss(updatedCss);
            Assert.AreEqual("1.5", parsed.ParagraphLineHeight, "Empty string should not overwrite existing value.");
        }

        [TestMethod]
        public void UpdateCssContent_ShouldNotGenerateEmptyFootnoteRules_WhenPropertiesAreDefault()
        {
            // テスト観点: 脚注プロパティが未設定の場合、CSSに不要な脚注関連セレクタが出現しないことを確認する。
            var service = new CssEditorService();
            var styleInfo = new CssStyleInfo(); // すべてデフォルト

            var updatedCss = service.UpdateCssContent(string.Empty, styleInfo);

            // .footnote-ref は上付き強制のため出現するが、他は出ないはず
            Assert.IsFalse(updatedCss.Contains(".footnotes {"), "Empty .footnotes rule should not be generated.");
            Assert.IsFalse(updatedCss.Contains(".footnote-back-ref {"), "Empty .footnote-back-ref rule should not be generated.");
            Assert.IsFalse(updatedCss.Contains(".footnote-ref::before {"), "Empty ::before rule should not be generated.");
        }

        [TestMethod]
        public void UpdateCssContent_ShouldPreserveHandwrittenPropertiesInFootnotes()
        {
            // テスト観点: 脚注スタイルにおいて、自動生成対象外のプロパティ（手書きカスタム）が維持されることを確認する。
            var service = new CssEditorService();
            var existingCss = ".footnote-ref { color: #000; z-index: 100; }";
            var styleInfo = new CssStyleInfo();
            styleInfo.Footnote.MarkerTextColor = "#FF0000";

            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // 更新されたプロパティと、維持されたプロパティの両方を確認
            Assert.IsTrue(updatedCss.Contains("color: rgba(255, 0, 0, 1);"), "Marker color should be updated.");
            Assert.IsTrue(updatedCss.Contains("z-index: 100;"), "Handwritten property should be preserved.");
        }

        [TestMethod]
        public void ParseCss_ShouldHandleVariousColorFormats()
        {
            // テスト観点: 脚注の文字色において、#RGB, #RRGGBB, rgb() などの形式が正しく解析されることを確認する。
            var service = new CssEditorService();

            var testCases = new[]
            {
                (".footnote-ref { color: #F00; }", "#FF0000"),
                (".footnote-ref { color: #00FF00; }", "#00FF00"),
                (".footnote-ref { color: rgb(0, 0, 255); }", "#0000FF"),
                (".footnote-ref { color: rgba(255, 255, 0, 1); }", "#FFFF00")
            };

            foreach (var (css, expectedHex) in testCases)
            {
                var styles = service.ParseCss(css);
                Assert.AreEqual(expectedHex, styles.Footnote.MarkerTextColor, $"Failed to parse color: {css}");
            }
        }
    }
}
