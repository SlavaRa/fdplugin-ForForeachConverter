using System.Collections.Generic;
using FlashDevelop;
using ForForeachConverter.Tests.TestUtils;
using NSubstitute;
using NUnit.Framework;
using PluginCore;
using PluginCore.Helpers;
using ScintillaNet;
using ScintillaNet.Enums;

namespace ForForeachConverter.Completion
{
    [TestFixture]
    public class CompleteTests
    {
        MainForm mainForm;
        ISettings settings;
        ITabbedDocument doc;
        protected ScintillaControl Sci;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            mainForm = new MainForm();
            settings = Substitute.For<ISettings>();
            settings.UseTabs = true;
            settings.IndentSize = 4;
            settings.SmartIndentType = SmartIndent.CPP;
            settings.TabIndents = true;
            settings.TabWidth = 4;
            doc = Substitute.For<ITabbedDocument>();
            mainForm.Settings = settings;
            mainForm.CurrentDocument = doc;
            mainForm.StandaloneMode = true;
            PluginBase.Initialize(mainForm);
            FlashDevelop.Managers.ScintillaManager.LoadConfiguration();
            Sci = GetBaseScintillaControl();
            doc.SciControl.Returns(Sci);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            settings = null;
            doc = null;
            mainForm.Dispose();
            mainForm = null;
        }

        ScintillaControl GetBaseScintillaControl()
        {
            return new ScintillaControl
            {
                Encoding = System.Text.Encoding.UTF8,
                CodePage = 65001,
                Indent = settings.IndentSize,
                Lexer = 3,
                StyleBits = 7,
                IsTabIndents = settings.TabIndents,
                IsUseTabs = settings.UseTabs,
                TabWidth = settings.TabWidth
            };
        }

        [TestFixture]
        public class GetStartOfStatementTests : CompleteTests
        {
            public IEnumerable<TestCaseData> AS3TestCases
            {
                get
                {
                    yield return new TestCaseData(" for each$(EntryPoint)(var it:* in {}) {}").Returns(1);
                    yield return new TestCaseData(" for$(EntryPoint) each(var it:* in {}) {}").Returns(1);
                    yield return new TestCaseData(" for $(EntryPoint)each(var it:* in {}) {}").Returns(1);
                    yield return new TestCaseData(" fo$(EntryPoint)r each(var it:* in {}) {}").Returns(1);
                    yield return new TestCaseData(" for eac$(EntryPoint)h(var it:* in {}) {}").Returns(1);
                    yield return new TestCaseData("$(EntryPoint) for each(var it:* in {}) {}").Returns(-1);
                    yield return new TestCaseData(" for$(EntryPoint)(var i:int = 0; i < 1; i++) {}").Returns(-1);
                    yield return new TestCaseData(" $(EntryPoint)for(var it:* in {}) {}").Returns(-1);
                }
            }

            [Test, TestCaseSource(nameof(AS3TestCases))]
            public int AS3(string sourceText) => ImplAS3(sourceText, Sci);

            static int ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                return Common(sourceText, sci);
            }

            static int Common(string sourceText, ScintillaControl sci)
            {
                sci.Text = sourceText;
                SnippetHelper.PostProcessSnippets(sci, 0);
                return Complete.GetStartOfStatement(sci, sci.CurrentPos);
            }
        }

        [TestFixture]
        public class GetExpressionTests : CompleteTests
        {
            
        }

        internal static string ReadAllTextAS3(string fileName)
        {
            return TestFile.ReadAllText($"{nameof(ForForeachConverter)}.Test_Files.completion.as3.{fileName}");
        }

        internal static string ReadAllTextHaxe(string fileName)
        {
            return TestFile.ReadAllText($"{nameof(ForForeachConverter)}.Test_Files.completion.haxe.{fileName}");
        }
    }
}
