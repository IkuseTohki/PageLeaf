using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using System;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class MarkdownFootnoteHelperTests
    {
        [TestMethod]
        public void RenumberFootnotes_ShouldReorderSequentially()
        {
            // テスト観点: バラバラな番号の脚注が登場順に 1, 2, 3... と振り直されることを確認する。
            var input = @"
本文[^5]です。
次の脚注[^2]です。

[^2]: 2番目の注釈
[^5]: 1番目の注釈
";
            var expected = @"
本文[^1]です。
次の脚注[^2]です。

[^1]: 1番目の注釈
[^2]: 2番目の注釈
";
            var result = MarkdownFootnoteHelper.Renumber(input).Replace("\r\n", "\n").Trim();
            Assert.AreEqual(expected.Replace("\r\n", "\n").Trim(), result);
        }

        [TestMethod]
        public void RenumberFootnotes_ShouldIgnoreCodeBlocks()
        {
            // テスト観点: コードブロック内の [^1] は無視され、置換されないことを確認する。
            var input = @"
本文[^2]です。

```
[^2]: これはコード内なので無視
```

[^2]: 本物の注釈
";
            var expected = @"
本文[^1]です。

```
[^2]: これはコード内なので無視
```

[^1]: 本物の注釈
";
            var result = MarkdownFootnoteHelper.Renumber(input).Replace("\r\n", "\n").Trim();
            Assert.AreEqual(expected.Replace("\r\n", "\n").Trim(), result);
        }

        [TestMethod]
        public void RenumberFootnotes_ShouldHandleDuplicates()
        {
            // テスト観点: 同一IDのマーカーが複数ある場合、同じ番号に変換されることを確認する。
            var input = "Note[^abc] and Note[^abc]\n\n[^abc]: Content";
            var expected = "Note[^1] and Note[^1]\n\n[^1]: Content";
            var result = MarkdownFootnoteHelper.Renumber(input).Replace("\r\n", "\n").Trim();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenumberFootnotes_ShouldHandleMissingDefinitions()
        {
            // テスト観点: 定義がないマーカーに対し、空の定義を作成することを確認する。
            var input = "Text[^1]";
            var expected = "Text[^1]\n\n[^1]:";
            var result = MarkdownFootnoteHelper.Renumber(input).Replace("\r\n", "\n").Trim();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenumberFootnotes_ShouldDropUnusedDefinitions()
        {
            // テスト観点: 参照されていない定義は削除されることを確認する。
            var input = "Text[^1]\n\n[^1]: Used\n[^2]: Unused";
            var expected = "Text[^1]\n\n[^1]: Used";
            var result = MarkdownFootnoteHelper.Renumber(input).Replace("\r\n", "\n").Trim();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenumberFootnotes_ShouldIgnoreInlineCode()
        {
            // テスト観点: インラインコード内の [^1] は無視されることを確認する。
            var input = "Note[^1] and `[^2]`\n\n[^1]: Real";
            var expected = "Note[^1] and `[^2]`\n\n[^1]: Real";
            var result = MarkdownFootnoteHelper.Renumber(input).Replace("\r\n", "\n").Trim();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetNextFootnoteNumber_ShouldReturnCorrectValues()
        {
            // テスト観点: 既存の最大番号+1 が正しく計算されることを確認する。
            Assert.AreEqual(1, MarkdownFootnoteHelper.GetNextFootnoteNumber(""));
            Assert.AreEqual(1, MarkdownFootnoteHelper.GetNextFootnoteNumber("No notes"));
            Assert.AreEqual(2, MarkdownFootnoteHelper.GetNextFootnoteNumber("Note[^1]"));
            Assert.AreEqual(6, MarkdownFootnoteHelper.GetNextFootnoteNumber("Note[^1] and [^5]"));
            Assert.AreEqual(1, MarkdownFootnoteHelper.GetNextFootnoteNumber("Note[^abc]"));
        }
    }
}
