using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using PDFco.Security.Cryptography;
using System.Security.Cryptography;


namespace BaldrSkyZero_TransProj
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Encoding jis = Encoding.GetEncoding("shift-jis");
        Encoding jisEx = Encoding.GetEncoding("shift-jis", new EncoderExceptionFallback(), new DecoderExceptionFallback());
        Encoding gb = Encoding.GetEncoding("GB2312");
        Encoding gbEx = Encoding.GetEncoding("GB2312", new EncoderExceptionFallback(), new DecoderExceptionFallback());
        static Encoding utf8 = new UTF8Encoding(true);
        Encoding unicode = new UnicodeEncoding();

        private void iL脚本提取ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = new OpenFileDialog();
            op.Filter = "il file (*.il)|*.il";
            op.FileName = "";
            op.CheckFileExists = true;
            if (op.ShowDialog() == DialogResult.OK && op.FileName != "")
            {
                var fs = File.OpenRead(op.FileName);
                var sr = new StreamReader(fs, gb);
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms, utf8);
                var fcnt = 0;
                var strCnt = 0;
                //var outdir = Path.Combine(Path.GetDirectoryName(op.FileName), "extract");
                var outfn = Path.ChangeExtension(op.FileName, ".txt");
                while (fs.Position < fs.Length || !sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var l = line.TrimStart();
                    if (l.StartsWith("{"))
                    {

                    }
                    else if (l.StartsWith("}"))
                    {
                        if (ms.Length > 0 && strCnt > 0)
                        {
                            //var outfn = Path.Combine(outdir, fcnt + ".txt");
                            //if(!Directory.Exists(Path.GetDirectoryName(outfn)))
                            //{
                            //    Directory.CreateDirectory(Path.GetDirectoryName(outfn));
                            //}
                            //sw.Flush();
                            //File.WriteAllBytes(outfn, ms.ToArray());
                            //ms.SetLength(3);    //utf8 header
                            sw.WriteLine("#MethodEnd " + fcnt);
                            fcnt++;
                            strCnt = 0;
                        }
                    }
                    else if (l.StartsWith("IL_"))
                    {
                        var indOperand = l.IndexOf(":");
                        if (indOperand == -1)
                            continue;
                        l = l.Substring(indOperand + 1).TrimStart();
                        if (l.StartsWith("ldstr"))
                        {
                            l = l.Substring(5).TrimStart();
                            if (l.StartsWith("\""))
                            {
                                var indEnd = l.IndexOf("\"", 1);
                                if (indEnd == -1)
                                    throw new Exception("\"\" not matched");
                                var str = l.Substring(1, indEnd - 1);
                                sw.WriteLine(str);
                                strCnt++;
                            }
                            else if (l.StartsWith("bytearray"))
                            {
                                l = l.Substring(9).TrimStart();
                                if (!l.StartsWith("("))
                                    throw new Exception("bytearray not followed with (");
                                int arrayEnd = -1;
                                var byteArray = new List<byte>();
                                l = deleteComments(l.Substring(1).TrimStart());
                                while ((arrayEnd = l.IndexOf(")")) == -1)
                                {
                                    var elements0 = l.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    int einLine = 0;
                                    foreach (var ec in elements0)
                                    {
                                        if (einLine++ > 15)
                                            break;
                                        byteArray.Add(Convert.ToByte(ec, 16));
                                    }
                                    l = deleteComments(sr.ReadLine().TrimStart());
                                }
                                l = l.Substring(0, arrayEnd);
                                var elements = l.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var ec in elements)
                                {
                                    byteArray.Add(Convert.ToByte(ec, 16));
                                }
                                var str = unicode.GetString(byteArray.ToArray());
                                sw.WriteLine(str);
                                strCnt++;
                            }
                        }
                        else if (l.StartsWith("call"))
                        {
                            l = l.Substring(4).TrimStart();
                            if (l.StartsWith("class INeXAS_Script NeXAS_Script::SetSelectMenu"))
                            {
                                sw.WriteLine("#SetSelectMenu");
                            }
                            else if (l.StartsWith("class INeXAS_Script NeXAS_Script::SetMessage"))
                            {
                                sw.WriteLine("#SetMessage");
                            }
                            else if (l.StartsWith("class INeXAS_Script NeXAS_Script::AddMessage"))
                            {
                                sw.WriteLine("#AddMessage");
                            }
                            else if (l.StartsWith("class [mscorlib]System.Collections.IEnumerator BattleScript::SetIngameMessage"))
                            {
                                sw.WriteLine("#SetIngameMessage");
                            }
                        }
                    }
                }
                sw.Flush();
                File.WriteAllBytes(outfn, ms.ToArray());
                sw.Close();
                ms.Close();
                sr.Close();
                fs.Close();
            }
        }

        static string deleteComments(string s)
        {
            if (s == null) return s;
            int pos = s.IndexOf("//");
            if (pos < 0) return s;
            return s.Substring(0, pos);
        }

        private void iL脚本导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = new OpenFileDialog();
            op.Filter = "il file (*.il)|*.il";
            op.FileName = "origin il file";
            op.CheckFileExists = true;
            if (op.ShowDialog() == DialogResult.OK && op.FileName != "")
            {
                var fs = File.OpenRead(op.FileName);
                var sr = new StreamReader(fs, gb);
                var strs = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(op.FileName), "Assembly-CSharp_out.txt"));
                var ptStrs = 0;
                var fcnt = 0;
                var strCnt = 0;
                var outfn = Path.ChangeExtension(op.FileName, null);
                outfn += "_out.il";
                var outfs = File.Open(outfn, FileMode.Create);
                var ms = new MemoryStream();
                var swDummy = new StreamWriter(ms, utf8);
                var sw = new StreamWriter(outfs, gb);
                while (fs.Position < fs.Length || !sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var l = line.TrimStart();
                    if (l.StartsWith("{"))
                    {
                        sw.WriteLine(line);
                    }
                    else if (l.StartsWith("}"))
                    {
                        if (ms.Length > 0 && strCnt > 0)
                        {
                            ptStrs++;
                            sw.WriteLine(line);
                            swDummy.WriteLine("#MethodEnd " + fcnt);
                            fcnt++;
                            strCnt = 0;
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                    else if (l.StartsWith("IL_"))
                    {
                        var indOperand = l.IndexOf(":");
                        if (indOperand == -1)
                        {
                            sw.WriteLine(line);
                            continue;
                        }
                        l = l.Substring(indOperand + 1).TrimStart();
                        if (l.StartsWith("ldstr"))
                        {
                            l = l.Substring(5).TrimStart();
                            if (l.StartsWith("\""))
                            {
                                var indEnd = l.IndexOf("\"", 1);
                                if (indEnd == -1)
                                    throw new Exception("\"\" not matched");
                                var str = l.Substring(1, indEnd - 1);
                                var ind = line.IndexOf($"\"{str}\"");
                                sw.WriteLine(line.Substring(0, ind) + $"\"{strs[ptStrs++]}\"" + (ind + str.Length + 2 == line.Length ? "" : line.Substring(ind + str.Length + 2)));
                                swDummy.WriteLine(str);
                                strCnt++;
                            }
                            else if (l.StartsWith("bytearray"))
                            {
                                l = l.Substring(9).TrimStart();
                                if (!l.StartsWith("("))
                                    throw new Exception("bytearray not followed with (");
                                int arrayEnd = -1;
                                var byteArray = new List<byte>();
                                l = deleteComments(l.Substring(1).TrimStart());
                                while ((arrayEnd = l.IndexOf(")")) == -1)
                                {
                                    var elements0 = l.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    int einLine = 0;
                                    foreach (var ec in elements0)
                                    {
                                        if (einLine++ > 15)
                                            break;
                                        byteArray.Add(Convert.ToByte(ec, 16));
                                    }
                                    l = deleteComments(sr.ReadLine().TrimStart());
                                }
                                l = l.Substring(0, arrayEnd);
                                var elements = l.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var ec in elements)
                                {
                                    byteArray.Add(Convert.ToByte(ec, 16));
                                }
                                var str = unicode.GetString(byteArray.ToArray());
                                var newline = new StringBuilder();
                                var ind = line.IndexOf("(");
                                newline.Append(line.Substring(0, ind + 1));
                                var newbytes = unicode.GetBytes(strs[ptStrs++]);
                                int curInd = 0;
                                while (curInd < newbytes.Length)
                                {
                                    newline.Append(newbytes[curInd].ToString("X2"));
                                    newline.Append(" ");
                                    if (curInd % 16 == 15 && curInd != newbytes.Length - 1)
                                    {
                                        sw.WriteLine(newline.ToString());
                                        newline.Clear();
                                    }
                                    curInd++;
                                }
                                newline.Append(")");
                                sw.WriteLine(newline.ToString());
                                swDummy.WriteLine(str);
                                strCnt++;
                            }
                            else
                            {
                                sw.WriteLine(line);
                            }
                        }
                        else if (l.StartsWith("call"))
                        {
                            l = l.Substring(4).TrimStart();
                            if (l.StartsWith("class INeXAS_Script NeXAS_Script::SetSelectMenu"))
                            {
                                swDummy.WriteLine("#SetSelectMenu");
                                ptStrs++;
                            }
                            else if (l.StartsWith("class INeXAS_Script NeXAS_Script::SetMessage"))
                            {
                                swDummy.WriteLine("#SetMessage");
                                ptStrs++;
                            }
                            else if (l.StartsWith("class INeXAS_Script NeXAS_Script::AddMessage"))
                            {
                                swDummy.WriteLine("#AddMessage");
                                ptStrs++;
                            }
                            else if (l.StartsWith("class [mscorlib]System.Collections.IEnumerator BattleScript::SetIngameMessage"))
                            {
                                swDummy.WriteLine("#SetIngameMessage");
                                ptStrs++;
                            }
                            sw.WriteLine(line);
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                    else
                    {
                        sw.WriteLine(line);
                    }
                }
                swDummy.Close();
                sw.Flush();
                sw.Close();
                outfs.Close();
                sr.Close();
                fs.Close();
            }
        }

        private void 剧本字符串导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = new OpenFileDialog();
            op.Filter = "txt file (*.txt)|*.txt";
            op.FileName = "";
            if (op.ShowDialog() == DialogResult.OK)
            {
                var dir = Path.GetDirectoryName(op.FileName);
                var outdir = dir + "\\msg";
                var strfn = dir + "\\string.txt";
                if (Directory.Exists(outdir) == false)
                {
                    Directory.CreateDirectory(outdir);
                }
                else
                {
                    Directory.Delete(outdir, true);
                    Directory.CreateDirectory(outdir);
                }

                var lines = File.ReadAllLines(op.FileName);
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms, utf8);
                var strMs = new MemoryStream();
                var strSw = new StreamWriter(strMs, utf8);
                var strcnt = 0;
                var msgcnt = 0;
                var indLine = 0;
                while (indLine < lines.Length)
                {
                    var l = lines[indLine];
                    if (l.StartsWith("#MethodEnd") && msgcnt > 0)
                    {
                        l = l.Substring(10).TrimStart();
                        var outfn = Path.Combine(outdir, l + ".txt");
                        if (!Directory.Exists(Path.GetDirectoryName(outfn)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(outfn));
                        }
                        sw.Flush();
                        File.WriteAllBytes(outfn, ms.ToArray());
                        ms.SetLength(3);    //utf8 header
                        msgcnt = 0;
                    }
                    else if (l.StartsWith("#SetMessage") || l.StartsWith("#SetIngameMessage"))
                    {
                        var msg = lines[indLine - 1];
                        var name = lines[indLine - 2];
                        if (name.StartsWith("#"))
                            name = "";
                        if (name != "" && msg.StartsWith("@v"))
                            strSw.WriteLine("m " + strcnt++ + " " + name);
                        sw.WriteLine("#" + name);
                        sw.WriteLine("m " + msgcnt + " " + msg);
                        msgcnt++;
                    }
                    else if (l.StartsWith("#AddMessage"))
                    {
                        var msg = lines[indLine - 1];
                        if (!msg.StartsWith("#"))
                        {
                            sw.WriteLine("m " + msgcnt + " " + msg);
                            msgcnt++;
                        }
                    }
                    else if (l.StartsWith("#SelectMessage"))
                    {
                        var msg = lines[indLine - 1];
                        sw.WriteLine("#Choice");
                        sw.WriteLine("m " + msgcnt + " " + msg);
                        msgcnt++;
                    }
                    indLine++;
                }
                strSw.Flush();
                File.WriteAllBytes(strfn, strMs.ToArray());
                strSw.Close();
            }
        }

        private void 剧本字符串导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = new OpenFileDialog();
            op.Filter = "txt file (*.txt)|*.txt";
            op.FileName = "";
            if (op.ShowDialog() == DialogResult.OK)
            {
                var dir = Path.GetDirectoryName(op.FileName);
                var outdir = dir + "\\msg";
                var strfn = dir + "\\string.txt";
                var outputFn = Path.ChangeExtension(op.FileName, null) + "_out.txt";
                var outlines = new List<string>();

                var lines = File.ReadAllLines(op.FileName);
                var strLines = File.ReadAllLines(strfn);
                var strs = new List<string>();
                var msgs = new List<string>();
                foreach (var s in strLines)
                {
                    if (s.StartsWith("m "))
                    {
                        var ind0 = s.IndexOf(' ', 0);
                        var ind1 = s.IndexOf(' ', ind0 + 1);
                        var inde = s.Substring(ind0 + 1, ind1 - ind0 - 1);
                        var ind = Convert.ToInt32(inde);
                        var msgTemp = s.Substring(ind1 + 1, s.Length - ind1 - 1);
                        strs.Add(msgTemp);
                    }
                }
                var strcnt = 0;
                var msgcnt = 0;
                var indLine = 0;
                var methodInd = 0;
                while (indLine < lines.Length)
                {
                    var l = lines[indLine];
                    if (l.StartsWith("#MethodEnd") || methodInd == 0)
                    {
                        msgs.Clear();
                        var ltrim = l.StartsWith("#MethodEnd") ? l.Substring(10).TrimStart() : "";
                        var outfn = Path.Combine(outdir, methodInd + ".txt");
                        if (File.Exists(outfn))
                        {
                            var msglines = File.ReadAllLines(outfn);
                            msgs.Clear();
                            foreach (var s in msglines)
                            {
                                if (s.StartsWith("m "))
                                {
                                    var ind0 = s.IndexOf(' ', 0);
                                    var ind1 = s.IndexOf(' ', ind0 + 1);
                                    var inde = s.Substring(ind0 + 1, ind1 - ind0 - 1);
                                    var ind = Convert.ToInt32(inde);
                                    var msgTemp = s.Substring(ind1 + 1, s.Length - ind1 - 1);
                                    msgs.Add(msgTemp);
                                }
                            }
                        }
                        methodInd++;
                        msgcnt = 0;
                    }
                    else if (l.StartsWith("#SetMessage") || l.StartsWith("#SetIngameMessage"))
                    {
                        outlines.RemoveAt(outlines.Count - 1);
                        var msg = lines[indLine - 1];
                        var name = lines[indLine - 2];
                        if (name.StartsWith("#"))
                            name = "";
                        if (name != "" && msg.StartsWith("@v"))
                        {
                            outlines.RemoveAt(outlines.Count - 1);
                            var str = strcnt >= strs.Count ? "" : strs[strcnt++];
                            outlines.Add(str);
                        }
                        outlines.Add(msgs[msgcnt]);
                        msgcnt++;
                    }
                    else if (l.StartsWith("#AddMessage"))
                    {
                        var msg = lines[indLine - 1];
                        if (!msg.StartsWith("#"))
                        {
                            outlines.RemoveAt(outlines.Count - 1);
                            outlines.Add(msgs[msgcnt]);
                            msgcnt++;
                        }
                    }
                    else if (l.StartsWith("#SelectMessage"))
                    {
                        var msg = lines[indLine - 1];
                        outlines.RemoveAt(outlines.Count - 1);
                        outlines.Add(msgs[msgcnt]);
                        msgcnt++;
                    }
                    else
                    {

                    }
                    outlines.Add(l);
                    indLine++;
                }
                File.WriteAllLines(outputFn, outlines, utf8);
            }
        }

        private void 字符串合并提取ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = new OpenFileDialog();
            op.Filter = "all file (*.*)|*.*";
            op.FileName = "";
            if (op.ShowDialog() == DialogResult.OK)
            {
                if (op.FileName != null)
                {
                    var dir = Path.GetDirectoryName(op.FileName);
                    afaTool.afaTool.mergeTextInDir(dir);
                }
            }
        }

        private void 合并字符串恢复ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = new OpenFileDialog();
            op.Filter = "all file (*.*)|*.*";
            op.FileName = "";
            if (op.ShowDialog() == DialogResult.OK)
            {
                if (op.FileName != null)
                {
                    var dir = Path.GetDirectoryName(op.FileName);
                    afaTool.afaTool.disperseTextIntoDir(dir);
                }
            }
        }

        private void 解密资源文本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op1 = new OpenFileDialog();
            op1.CheckFileExists = false;
            op1.Filter = "all file (*.*)|*.*";
            op1.FileName = "all txt here";
            if (op1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(op1.FileName))
            {
                var dir = Path.GetDirectoryName(op1.FileName);
                var outdir = dir + "_dec";
                var fls = Directory.GetFiles(dir, "*.txt", SearchOption.TopDirectoryOnly);
                foreach (var fl in fls)
                {
                    var str = File.ReadAllText(fl);
                    var bytes = Convert.FromBase64String(str);
                    var decoder = new RijndaelCryptography();
                    GenerateKeyFromPassword("BSZ.TGL.XML", decoder.KeySize, out byte[] key, decoder.BlockSize, out byte[] iv);
                    decoder.Key = key;
                    decoder.IV = iv;
                    decoder.Decrypt(str, out string outstr);
                    var outfn = Path.Combine(outdir, afaTool.afaTool.getPathWithoutDir(fl, dir));
                    if (!Directory.Exists(Path.GetDirectoryName(outfn)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(outfn));
                    }
                    File.WriteAllText(outfn, outstr);
                }

            }
        }

        private static void GenerateKeyFromPassword(string password, int keySize, out byte[] key, int blockSize, out byte[] iv)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("saltは必ず8バイト以上");
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, bytes);
            rfc2898DeriveBytes.IterationCount = 1000;
            key = rfc2898DeriveBytes.GetBytes(keySize / 8);
            iv = rfc2898DeriveBytes.GetBytes(blockSize / 8);
        }

        private void 加密资源文本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //to be implemented
        }

        private void 内嵌文本提取ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = new OpenFileDialog();
            op.Filter = "all file (*.*)|*.*";
            op.FileName = "ALL HERE";
            op.CheckFileExists = false;
            if (op.ShowDialog() == DialogResult.OK)
            {
                var dir = Path.GetDirectoryName(op.FileName);
                var files = Directory.GetFiles(dir);
                var strlist = new List<afaTool.afaTool.notedString>();
                foreach (var fl in files)
                {
                    var lines = File.ReadAllLines(fl);
                    foreach (var line in lines)
                    {
                        var l = line.Trim();
                        if (l.StartsWith("\"mText\":"))
                        {
                            var text = l.Substring(l.IndexOf("\"", 8) + 1, l.Length - 3 - l.IndexOf("\"", 8));
                            afaTool.afaTool.addStrToList(text, strlist);
                        }
                    }
                }
                var outfn = dir + "Str.txt";
                var fs = File.Open(outfn, FileMode.Create);
                var sw = new StreamWriter(fs);
                var strCnt = 0;
                foreach (var str in strlist)
                {
                    sw.WriteLine("m " + strCnt++ + " " + str.s);
                }
                sw.Close();
                fs.Close();
            }
        }

        private void 内嵌文本导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //to be implemented
        }
    }
}
