using System;
using System.Collections.Generic;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;


namespace ConsoleApplication3
{
    class Program
    {
        static string CreatePath = @"TestFile";

        static void Main(string[] args)
        {
            //List<MemberInfo> all = new List<MemberInfo>();
            //all.Add(new MemberInfo("short", "testShort"));
            //all.Add(new MemberInfo("int", "testint"));
            //all.Add(new MemberInfo("float", "testfloat"));
            //all.Add(new MemberInfo("string", "teststring"));
            //all.Add(new MemberInfo("arrayfloat", "testflaotlist"));
            //all.Add(new MemberInfo("arrayint", "testintlist"));

            if (!Directory.Exists(CreatePath))
            {
                Directory.CreateDirectory(CreatePath);
            }

            string path = @"config.txt";
            if(!File.Exists(path))
            {
                return;
            }
            string[] allLines = File.ReadAllLines(path);
            for (int i = 0; i < allLines.Length; i++)
            {
                // 如果该行是 [ 开头
                string tmpLine = allLines[i];
                if (tmpLine.Length == 0)
                {
                    continue;
                }
                else if (tmpLine[0] == '[')
                {
                    // 得到类名和ID
                    string[] tmp = tmpLine.Split('=');
                    string classname = tmp[0].Trim('[', ']');
                    short id;
                    if (tmp[1].IndexOf('/') == -1)
                    {
                        // 没注释
                        id = short.Parse(tmp[1].Trim(' '));
                    }
                    else
                    {
                        // 有注释
                        id = short.Parse(tmp[1].Substring(0, tmp[1].IndexOf('/')).Trim(' '));
                    }

                    i++; // 下一行

                    List<MemberInfo> all = new List<MemberInfo>();
                    // 获取所有字段 - 直到空行为止 - 暂不考虑最后没有留白
                    while (allLines[i].Length != 0)
                    {
                        string[] tmpMember = allLines[i].Split('=');
                        string fieldtype = tmpMember[0].Trim(' ');
                        string fieldname = tmpMember[1].Trim(' ');
                        all.Add(new MemberInfo(fieldtype, fieldname));
                        i++;
                    }
                    Create(classname, id, all);
                }
            }

            Console.WriteLine("创建在" + CreatePath + "文件夹中...");
            System.Diagnostics.Process.Start("explorer.exe", CreatePath);
        }

        static void Create(string scriptName, short id,  List<MemberInfo> allMember)
        {
            // 编译器单元 - 一个文件就是一个编辑器单元
            CodeCompileUnit unit = new CodeCompileUnit();

            // 命名空间 - 
            CodeNamespace ns = new CodeNamespace();

            // Add -> 引用必要的命名空间
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

            // 定义一个类 - 类名
            CodeTypeDeclaration needClass = new CodeTypeDeclaration(scriptName);
            needClass.IsClass = true;
            needClass.TypeAttributes = TypeAttributes.Public;
            needClass.BaseTypes.Add("PackageBase");  // 类的继承

            // 把类放到该该命名空间下
            ns.Types.Add(needClass);
            // 把命名空间放在编辑器单元中
            unit.Namespaces.Add(ns);

            // 创建 ID 字段
            CodeMemberField field_id = new CodeMemberField(typeof(short), "ID");
            field_id.Attributes = MemberAttributes.Public;
            field_id.InitExpression = new CodePrimitiveExpression(id);
            needClass.Members.Add(field_id);

            // 创建n个字段
            foreach (var item in allMember)
            {
                //CodeMemberField field = new CodeMemberField(item.mType, item.mName);
                CodeMemberField field = new CodeMemberField(GetTypeByString(item.mType), item.mName);
                field.Attributes = MemberAttributes.Public;
                needClass.Members.Add(field);
            }

            //添加方法， 
            CodeMemberMethod mothod = new CodeMemberMethod();
            // 方法 override - public
            mothod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            mothod.Name = "Create"; // 方法名
            mothod.ReturnType = new CodeTypeReference(typeof(byte[])); // 返回值byte[]
            // 方法中的各种语句见 CodeStatement 的各个派生类, 嫌麻烦就用字符串拼语句了
            // 固定的 - id
            mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)(ID & (short)filter))"));
            mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((ID>> 8) & (short)filter))"));
            mothod.Statements.Add(new CodeSnippetStatement());

            mothod.Statements.Add(new CodeVariableReferenceExpression("int len = 0"));
            // 找出所有的 string
            List<MemberInfo> tmpList = allMember.FindAll((x)=>x.mType == "string");
            foreach (var item in tmpList)
            {
                string text = string.Format("byte[] {0}ByteArray = Encoding.UTF8.GetBytes({0})", item.mName);
                mothod.Statements.Add(new CodeVariableReferenceExpression(text));
            }

            // 计算 len 的大小
            if (allMember.Count != 0)
            {
                string lentext = "len += ";
                foreach (var item in allMember)
                {
                    // int=4, short=2, float=4, string=2+lenght, List<int/float>=2+count*4
                    switch (item.mType)
                    {
                        case "short": lentext += " 2 +"; break;
                        case "int": lentext += "4 +"; break;
                        case "float": lentext += " 4 +"; break;
                        case "string": lentext += " 2 + " + item.mName + "ByteArray.Lenght +"; break;
                        case "arrayfloat":
                        case "arrayint": lentext += " 2 + " + item.mName + ".Count * 4 +"; break;
                        default:
                            break;
                    }
                }
                lentext = lentext.Trim('+');
                mothod.Statements.Add(new CodeVariableReferenceExpression(lentext)); // 会自己加分号
                mothod.Statements.Add(new CodeSnippetStatement());
            }

            // 固定的 - len
            mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)(len & (short)filter))"));
            mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((len >> 8) & (short)filter))"));
            mothod.Statements.Add(new CodeSnippetStatement());
            // 添加参数
            foreach (var item in allMember)
            {
                switch (item.mType)
                {
                    case "short":
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)("+item.mName+" & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((" + item.mName + " >> 8) & (short)filter))"));
                        break;
                    case "int":
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)(" + item.mName + " & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((" + item.mName + " >> 8) & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((" + item.mName + " >> 16) & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((" + item.mName + " >> 24) & (short)filter))"));
                        break;
                    case "float":
                        //mothod.Statements.Add(new CodeVariableReferenceExpression("byte[] by" + item.mName + " = BitConverter.GetBytes(" + item.mName + ")"));
                        // for 循环可以用 CodeIterationStatement 
                        //CodeIterationStatement tmpfor = new CodeIterationStatement();
                        //// 初始化  int m = 0
                        //tmpfor.InitStatement = new CodeVariableDeclarationStatement(
                        //    typeof(int), "m", new CodePrimitiveExpression(0));
                        //// 递增条件 m++
                        //tmpfor.IncrementStatement = new CodeAssignStatement(
                        //    new CodeVariableReferenceExpression("m"),
                        //    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("m"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
                        //// 循环条件 m < 10
                        //tmpfor.TestExpression = new CodeBinaryOperatorExpression(
                        //    new CodeVariableReferenceExpression("m"), CodeBinaryOperatorType.LessThan,
                        //    new CodePrimitiveExpression(10));
                        //// 循环语句内部  
                        //tmpfor.Statements.Add(new CodeVariableReferenceExpression("   buf.Add(byx[m])"));
                        //mothod.Statements.Add(tmpfor);

                        mothod.Statements.Add(new CodeVariableReferenceExpression("byte[] by" + item.mName + " = BitConverter.GetBytes(" + item.mName + ")"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\tfor (int m = 0; m < by" + item.mName + ".Length; m++) "));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t{"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("   buf.Add(by"+ item.mName + "[m])"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t}"));
                        break;
                    case "string":
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)("+item.mName+"ByteArray.Length & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)(("+item.mName+"ByteArray.Length >> 8) & (short)filter))"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\tfor (int i = 0; i < " + item.mName + "ByteArray.Length; i++) "));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t{"));
                        // 循环内
                        mothod.Statements.Add(new CodeVariableReferenceExpression("   buf.Add("+item.mName+"ByteArray[i])"));

                        mothod.Statements.Add(new CodeSnippetStatement("\t\t}"));
                        break;
                    case "arrayfloat":
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)(" + item.mName + ".Count & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((" + item.mName + ".Count >> 8) & (short)filter))"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\tfor (int i = 0; i < " + item.mName + ".Count; i++) "));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t{"));
                        // 循环内
                        mothod.Statements.Add(new CodeVariableReferenceExpression("byte[] by" + item.mName + " = BitConverter.GetBytes(" + item.mName + "[i])"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\tfor (int m = 0; m < by" + item.mName + ".Length; m++) "));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t{"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("   buf.Add(by"+ item.mName + "[m])"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t}"));

                        mothod.Statements.Add(new CodeSnippetStatement("\t\t}"));
                        //buf.Add((byte)(talkList.Count & (short)filter));
                        //buf.Add((byte)((talkList.Count >> 8) & (short)filter));
                        //for (int i = 0; i < talkList.Count; i++)
                        //{
                        //    byte[] bytalkList = BitConverter.GetBytes(talkList[i]);
                        //    for (int m = 0; m < bytalkList.Length; m++)
                        //    {
                        //        buf.Add(bytalkList[m]);
                        //    }
                        //}
                        break;
                    case "arrayint":
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)(" + item.mName + ".Count & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("buf.Add((byte)((" + item.mName + ".Count >> 8) & (short)filter))"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\tfor (int i = 0; i < " + item.mName + ".Count; i++) "));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t{"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("   buf.Add((byte)("+item.mName+"[i] & (short)filter));"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("   buf.Add((byte)((" + item.mName + "[i] >> 8) & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("   buf.Add((byte)((" + item.mName + "[i] >> 16) & (short)filter))"));
                        mothod.Statements.Add(new CodeVariableReferenceExpression("   buf.Add((byte)((" + item.mName + "[i] >> 24) & (short)filter))"));
                        mothod.Statements.Add(new CodeSnippetStatement("\t\t}"));
                        break;
                    default:
                        break;
                }
                mothod.Statements.Add(new CodeSnippetStatement());
            }

            // 方法最后的返回值
            mothod.Statements.Add(new CodeVariableReferenceExpression("return buf.ToArray()"));
            needClass.Members.Add(mothod); // 添加方法到类中

            // 生成脚本
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions option = new CodeGeneratorOptions(); // 成员间有空格
            option.BracingStyle = "C";  // C 风格脚本
            option.BlankLinesBetweenMembers = true;  // 成员之间插入空行

            string outputFile = CreatePath +"\\"+ scriptName +".cs"; // 脚本文件名
            StreamWriter sw = new StreamWriter(outputFile);

            provider.GenerateCodeFromCompileUnit(unit, sw, option);  // 将编辑器中的命名空间和类都写入具体的文件中
            sw.Close();
        }

        public static Type GetTypeByString(string type)
        {
            switch (type.ToLower())
            {
                case "bool":
                    return Type.GetType("System.Boolean", true, true);
                case "byte":
                    return Type.GetType("System.Byte", true, true);
                case "sbyte":
                    return Type.GetType("System.SByte", true, true);
                case "char":
                    return Type.GetType("System.Char", true, true);
                case "decimal":
                    return Type.GetType("System.Decimal", true, true);
                case "double":
                    return Type.GetType("System.Double", true, true);
                case "float":
                    return Type.GetType("System.Single", true, true);
                case "int":
                    return Type.GetType("System.Int32", true, true);
                case "uint":
                    return Type.GetType("System.UInt32", true, true);
                case "long":
                    return Type.GetType("System.Int64", true, true);
                case "ulong":
                    return Type.GetType("System.UInt64", true, true);
                case "object":
                    return Type.GetType("System.Object", true, true);
                case "short":
                    return Type.GetType("System.Int16", true, true);
                case "ushort":
                    return Type.GetType("System.UInt16", true, true);
                case "string":
                    return Type.GetType("System.String", true, true);
                case "date":
                case "datetime":
                    return Type.GetType("System.DateTime", true, true);
                case "guid":
                    return Type.GetType("System.Guid", true, true);
                case "arrayfloat":
                    return new List<float>().GetType();
                case "arrayint":
                    return new List<int>().GetType();
                default:
                    return Type.GetType(type, true, true);
            }
        }
    }

    public struct MemberInfo
    {
        public string mType;
        public string mName;

        public MemberInfo(string type, string name)
        {
            mType = type;
            mName = name;
        }
    }
}
