using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using PageLeaf.Models;
using System.Linq;
using System;

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
                "h2 {",
                "}",
                "",
                "h3 {",
                "}",
                "",
                "h4 {",
                "}",
                "",
                "h5 {",
                "}",
                "",
                "h6 {",
                "}",
                "",
                "blockquote {",
                "}",
                "",
                "ul {",
                "}",
                "",
                "th, td {",
                "}",
                "",
                "th {",
                "}",
                "",
                "code {",
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
            Assert.AreEqual("rgba(255, 0, 0, 1)", styles.HeadingTextColors["h1"]);
            Assert.AreEqual("rgba(0, 0, 255, 1)", styles.HeadingTextColors["h2"]);
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
            // Arrange
            var service = new CssEditorService();
            var cssContent = @"
                h1 { 
                    font-weight: bold; 
                    font-style: italic; 
                    text-decoration: underline; 
                } 
                h2 { 
                    text-decoration: line-through; 
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
            Assert.IsFalse(h1Flags.IsStrikethrough);

            // Assert - h2
            Assert.IsTrue(styles.HeadingStyleFlags.ContainsKey("h2"));
            var h2Flags = styles.HeadingStyleFlags["h2"];
            Assert.IsFalse(h2Flags.IsBold);
            Assert.IsFalse(h2Flags.IsItalic);
            Assert.IsFalse(h2Flags.IsUnderline);
            Assert.IsTrue(h2Flags.IsStrikethrough);

            // Assert - h3 (デフォルト値)
            Assert.IsTrue(styles.HeadingStyleFlags.ContainsKey("h3"));
            var h3Flags = styles.HeadingStyleFlags["h3"];
            Assert.IsFalse(h3Flags.IsBold);
            Assert.IsFalse(h3Flags.IsItalic);
            Assert.IsFalse(h3Flags.IsUnderline);
            Assert.IsFalse(h3Flags.IsStrikethrough);
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
                }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("1px", styles.TableBorderWidth);
            Assert.AreEqual("#DDDDDD", styles.TableBorderColor);
            Assert.AreEqual("8px", styles.TableCellPadding);
            Assert.AreEqual("#F2F2F2", styles.TableHeaderBackgroundColor);
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
            styleInfo.HeadingTextColors["h1"] = "rgba(0, 0, 255, 1)"; // Blue
            styleInfo.HeadingFontSizes["h1"] = "24px";
            styleInfo.HeadingFontFamilies["h1"] = "Arial";
            styleInfo.HeadingStyleFlags["h1"] = new HeadingStyleFlags { IsBold = true, IsUnderline = true };

            styleInfo.HeadingTextColors["h2"] = "rgba(0, 255, 0, 1)"; // Green
            styleInfo.HeadingFontSizes["h2"] = "1.5em";
            styleInfo.HeadingFontFamilies["h2"] = "Verdana";
            styleInfo.HeadingStyleFlags["h2"] = new HeadingStyleFlags { IsItalic = true, IsStrikethrough = true };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsedUpdatedStyles = service.ParseCss(updatedCss);

            // Assert h1
            Assert.AreEqual("rgba(0, 0, 255, 1)", parsedUpdatedStyles.HeadingTextColors["h1"]);
            Assert.AreEqual("24px", parsedUpdatedStyles.HeadingFontSizes["h1"]);
            Assert.AreEqual("Arial", parsedUpdatedStyles.HeadingFontFamilies["h1"]);
            Assert.IsTrue(parsedUpdatedStyles.HeadingStyleFlags["h1"].IsBold);
            Assert.IsTrue(parsedUpdatedStyles.HeadingStyleFlags["h1"].IsUnderline);
            Assert.IsFalse(parsedUpdatedStyles.HeadingStyleFlags["h1"].IsItalic);
            Assert.IsFalse(parsedUpdatedStyles.HeadingStyleFlags["h1"].IsStrikethrough);

            // Assert h2
            Assert.AreEqual("rgba(0, 255, 0, 1)", parsedUpdatedStyles.HeadingTextColors["h2"]);
            Assert.AreEqual("1.5em", parsedUpdatedStyles.HeadingFontSizes["h2"]);
            Assert.AreEqual("Verdana", parsedUpdatedStyles.HeadingFontFamilies["h2"]);
            Assert.IsTrue(parsedUpdatedStyles.HeadingStyleFlags["h2"].IsItalic);
            Assert.IsTrue(parsedUpdatedStyles.HeadingStyleFlags["h2"].IsStrikethrough);
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
                TableCellPadding = "10px"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);
            var parsedUpdatedStyles = service.ParseCss(updatedCss);

            // Assert
            Assert.AreEqual("#AAAAAA", parsedUpdatedStyles.TableBorderColor);
            Assert.AreEqual("2px", parsedUpdatedStyles.TableBorderWidth);
            Assert.AreEqual("#BBBBBB", parsedUpdatedStyles.TableHeaderBackgroundColor);
            Assert.AreEqual("10px", parsedUpdatedStyles.TableCellPadding);
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
            var nl = Environment.NewLine; // 比較用に改行コードを取得

            // Assert - h1とh3にのみ採番ルールが適用されていることを確認
            // body
            StringAssert.Contains(updatedCss, "body {" + nl + "  counter-reset: h1 0;" + nl + "}");

            // h1
            StringAssert.Contains(updatedCss, "h1 {" + nl + "  color: rgba(255, 0, 0, 1);" + nl + "  counter-increment: h1;" + nl + "  counter-reset: h2 0;" + nl + "}");
            StringAssert.Contains(updatedCss, "h1::before {" + nl + "  content: counter(h1) \". \";" + nl + "}");

            // h2 (採番無効なのでcounter-resetのみ)
            StringAssert.Contains(updatedCss, "h2 {" + nl + "}"); // h2ルール自体は残る
            Assert.IsFalse(updatedCss.Contains("h2 {" + nl + "  counter-increment:")); // h2のcounter-incrementはなし
            Assert.IsFalse(updatedCss.Contains("h2::before")); // h2の::beforeはなし

            // h3
            StringAssert.Contains(updatedCss, "h3 {" + nl + "  font-size: 1.2em;" + nl + "  counter-increment: h3;" + nl + "  counter-reset: h4 0;" + nl + "}");
            StringAssert.Contains(updatedCss, "h3::before {" + nl + "  content: counter(h1) \".\" counter(h2) \".\" counter(h3) \". \";" + nl + "}");

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
            var nl = Environment.NewLine;

            // Assert - h1の採番ルールが削除されていることを確認
            Assert.IsFalse(updatedCss.Contains("body {" + nl + "  counter-reset: h1 0;" + nl + "}")); // bodyのcounter-resetもなし
            Assert.IsFalse(updatedCss.Contains("h1 {" + nl + "  counter-increment:"));
            Assert.IsFalse(updatedCss.Contains("h1::before"));

            // Assert - h2の採番ルールは残っていることを確認
            StringAssert.Contains(updatedCss, "h2 {" + nl + "  counter-increment: h2;" + nl + "  counter-reset: h3 0;" + nl + "}");
            StringAssert.Contains(updatedCss, "h2::before {" + nl + "  content: counter(h1) \".\" counter(h2) \". \";" + nl + "}");

            // Assert - h3は採番ルールがないことを確認
            StringAssert.Contains(updatedCss, "h3 {" + nl + "  font-size: 1em;" + nl + "}");
            Assert.IsFalse(updatedCss.Contains("h3 {" + nl + "  counter-increment:"));
            Assert.IsFalse(updatedCss.Contains("h3::before"));
        }
    }
}
