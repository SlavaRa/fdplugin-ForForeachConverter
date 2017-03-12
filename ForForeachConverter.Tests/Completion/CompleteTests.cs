// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Completion;
using ASCompletion.Context;
using ASCompletion.Model;
using ASCompletion.Settings;
using FlashDevelop;
using FlashDevelop.Managers;
using ForForeachConverter.TestUtils;
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
            ScintillaManager.LoadConfiguration();
            var pluginMain = Substitute.For<ASCompletion.PluginMain>();
            var pluginUI = new PluginUI(pluginMain);
            pluginMain.MenuItems.Returns(new List<ToolStripItem>());
            pluginMain.Settings.Returns(new GeneralSettings());
            pluginMain.Panel.Returns(pluginUI);
            #region ASContext.GlobalInit(pluginMain);
            var method = typeof(ASContext).GetMethod("GlobalInit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new[] { pluginMain });
            #endregion
            ASContext.Context = Substitute.For<IASContext>();
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
                Encoding = Encoding.UTF8,
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

            public IEnumerable<TestCaseData> HaxeTestCases
            {
                get
                {
                    yield return new TestCaseData(" for$(EntryPoint)(it in [1,2,3,4]) {}").Returns(1);
                    yield return new TestCaseData(" fo$(EntryPoint)r(it in [1,2,3,4]) {}").Returns(1);
                    yield return new TestCaseData("$(EntryPoint) for(it in [1,2,3,4]) {}").Returns(-1);
                    yield return new TestCaseData(" for$(EntryPoint)(i in 0...10) {}").Returns(-1);
                }
            }

            [Test, TestCaseSource(nameof(HaxeTestCases))]
            public int Haxe(string sourceText) => ImplHaxe(sourceText, Sci);

            static int ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                return Common(sourceText, sci);
            }

            static int ImplHaxe(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "haxe";
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
        public class GetStartOfBodyTests : CompleteTests
        {
            public IEnumerable<TestCaseData> AS3TestCases
            {
                get
                {
                    yield return new TestCaseData("$(EntryPoint)for each(var it:* in {}) {}").Returns("for each(var it:* in {})".Length);
                }
            }

            [Test, TestCaseSource(nameof(AS3TestCases))]
            public int AS3(string sourceText) => ImplAS3(sourceText, Sci);

            public IEnumerable<TestCaseData> HaxeTestCases
            {
                get
                {
                    yield return new TestCaseData("$(EntryPoint)for(it in [1,2,3,4]) {}").Returns("for(it in [1,2,3,4])".Length);
                }
            }

            [Test, TestCaseSource(nameof(HaxeTestCases))]
            public int Haxe(string sourceText) => ImplHaxe(sourceText, Sci);

            static int ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                return Common(sourceText, sci);
            }

            static int ImplHaxe(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "haxe";
                return Common(sourceText, sci);
            }

            static int Common(string sourceText, ScintillaControl sci)
            {
                sci.Text = sourceText;
                SnippetHelper.PostProcessSnippets(sci, 0);
                return Complete.GetStartOfBody(sci, sci.CurrentPos);
            }
        }

        [TestFixture]
        public class GetEndOfStatementTests : CompleteTests
        {
            public IEnumerable<TestCaseData> AS3TestCases
            {
                get
                {
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {}) {}")
                            .Returns("for each(var it:* in {}) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {}) trace(it);")
                            .Returns("for each(var it:* in {}) trace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\tif(it != null) trace(it);")
                            .Returns("for each(var it:* in {})\n\tif(it != null) trace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\tif(it != null) trace(it);\n\telse trace(it);")
                            .Returns("for each(var it:* in {})\n\tif(it != null) trace(it);\n\telse trace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\tfor(var field:* in it)\n\ttrace(field);")
                            .Returns("for each(var it:* in {})\n\tfor(var field:* in it)\n\ttrace(field);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\tfor(var i:int = 0; i < 10; i++)\n\ttrace(field + i);")
                            .Returns("for each(var it:* in {})\n\tfor(var i:int = 0; i < 10; i++)\n\ttrace(field + i);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\tfor each(var v:* in it)\n\ttrace(v);")
                            .Returns("for each(var it:* in {})\n\tfor each(var v:* in it)\n\ttrace(v);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\twhile(it != null)\n\ttrace(it);")
                            .Returns("for each(var it:* in {})\n\twhile(it != null)\n\ttrace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\tswitch(it) {\n\t\tcase null: break;\n\t\tdefault: trace(it);\n\t}")
                            .Returns("for each(var it:* in {})\n\tswitch(it) {\n\t\tcase null: break;\n\t\tdefault: trace(it);\n\t}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\tdo {trace(it);}\n\twhile(it != null);")
                            .Returns("for each(var it:* in {})\n\tdo {trace(it);}\n\twhile(it != null);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t} catch(e:*) {}")
                            .Returns("for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t} catch(e:*) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:*) {}")
                            .Returns("for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:*) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:String) {\n\t} catch(e:*) {}")
                            .Returns("for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:String) {\n\t} catch(e:*) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:*) {}\n\t finally {\n\t\t//some comment...\n\t}")
                            .Returns("for each(var it:* in {})\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:*) {}\n\t finally {\n\t\t//some comment...\n\t}".Length);
                }
            }

            [Test, TestCaseSource(nameof(AS3TestCases))]
            public int AS3(string sourceText) => ImplAS3(sourceText, Sci);

            public IEnumerable<TestCaseData> HaxeTestCases
            {
                get
                {
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4]) {}")
                            .Returns("for(it in [1,2,3,4]) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4]) trace(it);")
                            .Returns("for(it in [1,2,3,4]) trace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\tif(it != null) trace(it);")
                            .Returns("for(it in [1,2,3,4])\n\tif(it != null) trace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\tif(it != null) trace(it);\n\telse trace(it);")
                            .Returns("for(it in [1,2,3,4])\n\tif(it != null) trace(it);\n\telse trace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\tfor(field in it)\n\ttrace(field);")
                            .Returns("for(it in [1,2,3,4])\n\tfor(field in it)\n\ttrace(field);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\tfor(i in 0...10)\n\ttrace(field + i);")
                            .Returns("for(it in [1,2,3,4])\n\tfor(i in 0...10)\n\ttrace(field + i);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\twhile(it != null)\n\ttrace(it);")
                            .Returns("for(it in [1,2,3,4])\n\twhile(it != null)\n\ttrace(it);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\tswitch(it) {\n\t\tcase null: break;\n\t\tdefault: trace(it);\n\t}")
                            .Returns("for(it in [1,2,3,4])\n\tswitch(it) {\n\t\tcase null: break;\n\t\tdefault: trace(it);\n\t}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\tdo {trace(it);}\n\twhile(it != null);")
                            .Returns("for(it in [1,2,3,4])\n\tdo {trace(it);}\n\twhile(it != null);".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\ttry {\n\t\ttrace(it);\n\t} catch(e:Dynamic) {}")
                            .Returns("for(it in [1,2,3,4])\n\ttry {\n\t\ttrace(it);\n\t} catch(e:Dynamic) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:Dynamic) {}")
                            .Returns("for(it in [1,2,3,4])\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:Dynamic) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)for(it in [1,2,3,4])\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:String) {\n\t} catch(e:Dynamic) {}")
                            .Returns("for(it in [1,2,3,4])\n\ttry {\n\t\ttrace(it);\n\t}\n\t catch(e:String) {\n\t} catch(e:Dynamic) {}".Length);
                }
            }

            [Test, TestCaseSource(nameof(HaxeTestCases))]
            public int Haxe(string sourceText) => ImplHaxe(sourceText, Sci);

            static int ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                return Common(sourceText, sci);
            }

            static int ImplHaxe(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "haxe";
                return Common(sourceText, sci);
            }

            static int Common(string sourceText, ScintillaControl sci)
            {
                sci.Text = sourceText;
                SnippetHelper.PostProcessSnippets(sci, 0);
                return Complete.GetEndOfStatement(sci, sci.CurrentPos);
            }
        }

        [TestFixture]
        public class GetStartOfIFStatementTests : CompleteTests
        {
            public IEnumerable<TestCaseData> AS3TestCases
            {
                get
                {
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) {}")
                            .Returns("if(true) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) {}\n\telse {}")
                            .Returns("if(true) {}\n\telse {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) trace('')\n\telse {}")
                            .Returns("if(true) trace('')\n\telse {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) trace('')\n\telse trace('')")
                            .Returns("if(true) trace('')\n\telse trace('')".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) trace('')\n\telse if(false) trace('')")
                            .Returns("if(true) trace('')\n\telse if(false) trace('')".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(a == 1) trace('')\n\telse if(a == 2) trace('')\n\t else if(a == 3) {}\n\t else {}\n\t//some code here...")
                            .Returns("if(a == 1) trace('')\n\telse if(a == 2) trace('')\n\t else if(a == 3) {}\n\t else {}\n".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true)\n\tfor(var i:int = 0; i < 10; i++) trace(i)")
                            .Returns("if(true)\n\tfor(var i:int = 0; i < 10; i++) trace(i)".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true)\n\tif(true) trace(i)")
                            .Returns("if(true)\n\tif(true) trace(i)".Length);
                }
            }

            [Test, TestCaseSource(nameof(AS3TestCases))]
            public int AS3(string sourceText) => ImplAS3(sourceText, Sci);

            public IEnumerable<TestCaseData> HaxeTestCases
            {
                get
                {
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) {}")
                            .Returns("if(true) {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) {}\n\telse {}")
                            .Returns("if(true) {}\n\telse {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) trace('')\n\telse {}")
                            .Returns("if(true) trace('')\n\telse {}".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) trace('')\n\telse trace('')")
                            .Returns("if(true) trace('')\n\telse trace('')".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true) trace('')\n\telse if(false) trace('')")
                            .Returns("if(true) trace('')\n\telse if(false) trace('')".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(a == 1) trace('')\n\telse if(a == 2) trace('')\n\t else if(a == 3) {}\n\t else {}\n\t//some code here...")
                            .Returns("if(a == 1) trace('')\n\telse if(a == 2) trace('')\n\t else if(a == 3) {}\n\t else {}\n".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true)\n\tfor(var i:int = 0; i < 10; i++) trace(i)")
                            .Returns("if(true)\n\tfor(var i:int = 0; i < 10; i++) trace(i)".Length);
                    yield return
                        new TestCaseData("$(EntryPoint)if(true)\n\tif(true) trace(i)")
                            .Returns("if(true)\n\tif(true) trace(i)".Length);
                }
            }

            [Test, TestCaseSource(nameof(HaxeTestCases))]
            public int Haxe(string sourceText) => ImplHaxe(sourceText, Sci);

            static int ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                return Common(sourceText, sci);
            }

            static int ImplHaxe(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "haxe";
                return Common(sourceText, sci);
            }

            static int Common(string sourceText, ScintillaControl sci)
            {
                sci.Text = sourceText;
                SnippetHelper.PostProcessSnippets(sci, 0);
                return Complete.GetStartOfIFStatement(sci, sci.CurrentPos);
            }
        }

        [TestFixture]
        public class GetVarOfForeachStatementTests : CompleteTests
        {
            public IEnumerable<TestCaseData> AS3TestCases
            {
                get
                {
                    yield return
                        new TestCaseData("$(EntryPoint)for each(var it:Number in [1,2,3]){}").Returns(new MemberModel
                        {
                            Name = "it",
                            Flags = FlagType.Dynamic | FlagType.Variable,
                            Type = "Number"
                        });
                }
            }

            [Test, TestCaseSource(nameof(AS3TestCases))]
            public MemberModel AS3(string sourceText) => ImplAS3(sourceText, Sci);

            public IEnumerable<TestCaseData> HaxeTestCases
            {
                get
                {
                    yield return new TestCaseData("$(EntryPoint)for(it in [1,2,3]){}").Returns(null);
                }
            }

            [Test, TestCaseSource(nameof(HaxeTestCases))]
            public MemberModel Haxe(string sourceText) => ImplHaxe(sourceText, Sci);

            static MemberModel ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                ASContext.Context.SetAS3Features();
                return Common(sourceText, sci);
            }

            static MemberModel ImplHaxe(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "haxe";
                ASContext.Context.SetHaxeFeatures();
                return Common(sourceText, sci);
            }

            static MemberModel Common(string sourceText, ScintillaControl sci)
            {
                sci.Text = sourceText;
                SnippetHelper.PostProcessSnippets(sci, 0);
                var currentModel = ASContext.Context.CurrentModel;
                new ASFileParser().ParseSrc(currentModel, sci.Text);
                var currentClass = currentModel.Classes.FirstOrDefault() ?? ClassModel.VoidClass;
                ASContext.Context.CurrentClass.Returns(currentClass);
                ASContext.Context.CurrentMember.Returns(currentClass.Members.Items.FirstOrDefault());
                var result = Complete.GetVarOfForeachStatement(sci, sci.CurrentPos);
                return result.Member;
            }
        }

        [TestFixture]
        public class GetCollectionOfForeachStatementTests : CompleteTests
        {
            public IEnumerable<TestCaseData> AS3TestCases
            {
                get
                {
                    yield return
                        new TestCaseData("var a:Array = [1,2,3];\n$(EntryPoint)for each(var it:Number in a){}").Returns(new MemberModel
                        {
                            Name = "a",
                            Flags = FlagType.Dynamic | FlagType.Variable,
                            Type = "Array"
                        });
                }
            }

            [Test, TestCaseSource(nameof(AS3TestCases))]
            public MemberModel AS3(string sourceText) => ImplAS3(sourceText, Sci);

            static MemberModel ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                ASContext.Context.SetAS3Features();
                return Common(sourceText, sci);
            }

            public IEnumerable<TestCaseData> HaxeTestCases
            {
                get
                {
                    yield return
                        new TestCaseData("var a:Array<Int> = [1,2,3];\n$(EntryPoint)for(it in a){}").Returns(new MemberModel
                        {
                            Name = "a",
                            Flags = FlagType.Dynamic | FlagType.Variable,
                            Type = "Array<Int>"
                        });
                }
            }

            [Test, TestCaseSource(nameof(HaxeTestCases))]
            public MemberModel Haxe(string sourceText) => ImplHaxe(sourceText, Sci);

            static MemberModel ImplHaxe(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "haxe";
                ASContext.Context.SetHaxeFeatures();
                return Common(sourceText, sci);
            }

            static MemberModel Common(string sourceText, ScintillaControl sci)
            {
                sci.Text = sourceText;
                SnippetHelper.PostProcessSnippets(sci, 0);
                var currentModel = ASContext.Context.CurrentModel;
                new ASFileParser().ParseSrc(currentModel, sci.Text);
                var currentClass = currentModel.Classes.FirstOrDefault() ?? ClassModel.VoidClass;
                ASContext.Context.CurrentClass.Returns(currentClass);
                ASContext.Context.CurrentMember.Returns(currentClass.Members.Items.FirstOrDefault());
                var result = Complete.GetCollectionOfForeachStatement(sci, sci.CurrentPos);
                return result.Member;
            }
        }

        [TestFixture]
        public class GetExpressionTests : CompleteTests
        {
            public IEnumerable<TestCaseData> AS3TestCases
            {
                get
                {
                    yield return
                        new TestCaseData("var a:Array = [1,2,3];\n$(EntryPoint)for each(var it:Number in a){}")
                            .Returns(new EForeach
                            {
                                StartPosition = "var a:Array = [1,2,3];\n".Length,
                                EndPosition = "var a:Array = [1,2,3];\nfor each(var it:Number in a){}".Length,
                                BodyPosition = "var a:Array = [1,2,3];\nfor each(var it:Number in a)".Length,
                                Variable = new ASResult
                                {
                                    Member = new MemberModel
                                    {
                                        Name = "it",
                                        Flags = FlagType.Dynamic | FlagType.Variable,
                                        Type = "Number"
                                    }
                                },
                                Collection = new ASResult
                                {
                                    Member = new MemberModel
                                    {
                                        Name = "a",
                                        Flags = FlagType.Dynamic | FlagType.Variable,
                                        Type = "Array"
                                    }
                                }
                            });
                }
            }

            [Test, TestCaseSource(nameof(AS3TestCases))]
            public EForeach AS3(string sourceText) => ImplAS3(sourceText, Sci);

            static EForeach ImplAS3(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "as3";
                ASContext.Context.SetAS3Features();
                return Common(sourceText, sci);
            }

            public IEnumerable<TestCaseData> HaxeTestCases
            {
                get
                {
                    yield return
                        new TestCaseData("var a:Array<Int> = [1,2,3];\n$(EntryPoint)for(it in a){}")
                            .Returns(new EForeach
                            {
                                StartPosition = "var a:Array<Int> = [1,2,3];\n".Length,
                                EndPosition = "var a:Array<Int> = [1,2,3];\nfor(it in a){}".Length,
                                BodyPosition = "var a:Array<Int> = [1,2,3];\nfor(it in a)".Length,
                                Variable = new ASResult(),
                                Collection = new ASResult
                                {
                                    Member = new MemberModel
                                    {
                                        Name = "a",
                                        Flags = FlagType.Dynamic | FlagType.Variable,
                                        Type = "Array<Int>"
                                    }
                                }
                            });
                }
            }

            [Test, TestCaseSource(nameof(HaxeTestCases))]
            public EForeach Haxe(string sourceText) => ImplHaxe(sourceText, Sci);

            static EForeach ImplHaxe(string sourceText, ScintillaControl sci)
            {
                sci.ConfigurationLanguage = "haxe";
                ASContext.Context.SetHaxeFeatures();
                return Common(sourceText, sci);
            }

            static EForeach Common(string sourceText, ScintillaControl sci)
            {
                sci.Text = sourceText;
                SnippetHelper.PostProcessSnippets(sci, 0);
                var currentModel = ASContext.Context.CurrentModel;
                new ASFileParser().ParseSrc(currentModel, sci.Text);
                var currentClass = currentModel.Classes.FirstOrDefault() ?? ClassModel.VoidClass;
                ASContext.Context.CurrentClass.Returns(currentClass);
                ASContext.Context.CurrentMember.Returns(currentClass.Members.Items.FirstOrDefault());
                var result = Complete.GetExpression(sci, sci.CurrentPos);
                return result;
            }
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
