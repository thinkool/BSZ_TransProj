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
using System.Threading;
using System.Text.RegularExpressions;


namespace afaTool
{
    public partial class afaTool
    {
        public afaTool()
        {
            
        }

        public static Encoding shiftJis = Encoding.GetEncoding("shift-jis");
        public static Encoding gb = Encoding.GetEncoding("GB2312");

        public static Encoding jis = Encoding.GetEncoding("shift-jis"); //can be changed by checkbox
        public static Encoding tjis = Encoding.GetEncoding("shift-jis");
        public static Encoding fjis = Encoding.GetEncoding("shift-jis", new EncoderExceptionFallback(), new DecoderExceptionFallback());
        public static Encoding utf8 = new UTF8Encoding(true);

        public static string trim(string line)
        {
            var noteInd = line.IndexOf("//");
            if (noteInd >= 0 && noteInd < line.Length && line.Substring(noteInd - 1, 1) != ":")
            {
                line = line.Substring(0, noteInd);
            }
            if (line != null)
            {
                var chars = line.ToArray();
                int ftrim = 0;
                int etrim = chars.Length - 1;
                while (ftrim < chars.Length && (chars[ftrim] == '\t' || chars[ftrim] == ' '))
                {
                    ftrim++;
                }
                while (etrim >= 0 && (chars[etrim] == '\t' || chars[etrim] == ' '))
                {
                    etrim--;
                }
                if (etrim < ftrim)
                    etrim = ftrim - 1;
                line = line.Substring(ftrim, etrim - ftrim + 1);
            }
            return line;
        }

        public class notedString
        {
            public string s;
            public string n;
            //public notedString() { s = null; n = null; }
            //public notedString(string sf) { s = sf; n = null; }
            public notedString(string sf = null, string nf = null) { s = sf; n = nf; }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (obj.GetType() != typeof(notedString))
                    return false;
                notedString c = obj as notedString;
                return (this.s == c.s);
            }
            public override int GetHashCode()
            {
                int hashCode = 0;
                unchecked
                {
                    hashCode += 1000000007 * s.GetHashCode();
                }
                return hashCode;
            }

            public bool addNote(string note)
            {
                if (note == null)
                {
                    return true;
                }
                else
                {
                    if (n == null)
                    {
                        n = note;
                    }
                    else if (n.Contains(note))
                    {

                    }
                    else
                    {
                        if (note == "NoChange&&")
                        {
                            n = note + n;
                        }
                        else if (note.Contains("]&&"))
                        {
                            if (n.Contains("NoChange&&"))
                            {
                                n = "NoChange&&" + note + n.Substring(10, n.Length - 10);
                            }
                            else
                            {
                                n = note + n;
                            }
                        }
                        else
                        {
                            n = n + note;
                        }
                    }
                    return true;
                }

            }

        }

        public static bool addStrToList(string str, List<notedString> strlist)
        {
            var chars = str.ToArray();
            var isDB = false;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] > 0x80)
                {
                    isDB = true;
                    break;
                }
            }
            if (isDB)
            {
                var toAdd = new notedString(str);
                if (!strlist.Contains(toAdd))
                {
                    strlist.Add(toAdd);
                }
                else
                {
                    //change note
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool addStrToList(notedString str, List<notedString> strlist)
        {
            var chars = str.s.ToArray();
            var isDB = false;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] > 0x80)
                {
                    isDB = true;
                    break;
                }
            }
            if (isDB)
            {
                if (str.n != null && str.n.Contains("]&&"))
                {
                    str.n = "[" + shiftJis.GetByteCount(str.s) / 2 + "]&&";
                }

                if (!strlist.Contains(str))
                {
                    strlist.Add(str);
                }
                else
                {
                    //change note
                    var find = strlist.IndexOf(str);
                    strlist[find].addNote(str.n);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string getStrFromList(string note, List<notedString> strlist)
        {
            for (int i = 0; i < strlist.Count; i++)
            {
                if (strlist[i].n == note)
                {
                    return strlist[i].s;
                }
            }
            return null;
        }
        public static string getNoteFromList(string str, List<notedString> strlist)
        {
            for (int i = 0; i < strlist.Count; i++)
            {
                if (strlist[i].s == str)
                {
                    return strlist[i].n;
                }
            }
            return null;
        }
        public static int getIndexFromList(string str, List<notedString> strlist)
        {
            for (int i = 0; i < strlist.Count; i++)
            {
                if (strlist[i].s == str)
                {
                    return i;
                }
            }
            return -1;
        }
        public static List<notedString> readStringFile(string fn, string orifn)
        {
            var strlist = new List<notedString>();
            var fs = File.OpenRead(orifn);
            var sr = new StreamReader(fs, utf8);
            var noteList = new List<notedString>();
            var line = "";
            var lastline = "";
            while (fs.Position < fs.Length || !sr.EndOfStream)
            {
                lastline = line;
                line = sr.ReadLine();
                string note = null;
                if (line.Length > 0 && line.Substring(0, 1) == "m")
                {
                    if (lastline.Length > 1 && lastline.Substring(0, 1) == "#")
                    {
                        note = lastline.Substring(1, lastline.Length - 1);
                    }
                    var ind = line.IndexOf(' ', 2);
                    var index = Convert.ToInt32(line.Substring(2, ind - 2));
                    var str = line.Substring(ind + 1, line.Length - ind - 1);
                    addStrToList(str, strlist);
                    noteList.Add(new notedString(Convert.ToString(index), note));
                }
            }
            sr.Close();
            fs.Close();
            fs = File.OpenRead(fn);
            sr = new StreamReader(fs, utf8);
            while (fs.Position < fs.Length || !sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line.Length > 0 && line.Substring(0, 1) == "m")
                {
                    var ind = line.IndexOf(' ', 2);
                    var index = Convert.ToInt32(line.Substring(2, ind - 2));
                    var str = line.Substring(ind + 1, line.Length - ind - 1);
                    var indInList = getIndexFromList(Convert.ToString(index), noteList);
                    if (str.Length > 0)
                    {
                        if (noteList[indInList].n != null && noteList[indInList].n.Contains("]&&"))
                        {
                            var jisC = 0;
                            try
                            {
                                jisC = fjis.GetByteCount(strlist[indInList].s);
                            }
                            catch (Exception)
                            {
                                jisC = -1;
                                str = strlist[indInList].s;
                            }
                            while (gb.GetByteCount(str) > jisC && jisC != -1)
                            {
                                var diff = gb.GetByteCount(str) - jis.GetByteCount(strlist[indInList].s);
                                diff = (diff + 1) / 2;
                                str = str.Substring(0, str.Length - diff);
                            }
                        }
                        strlist[indInList].n = str;
                    }
                    else
                    {
                        //strlist[indInList].n = strlist[indInList].s;
                    }
                }
            }
            sr.Close();
            fs.Close();
            return strlist;
        }

        static private List<notedString> readStringFilePlain(string fn, string orifn)
        {
            var strlist = new List<notedString>();
            var strlines = File.ReadAllLines(orifn, utf8);
            var notelines = File.ReadAllLines(fn, utf8);
            for (int i = 0; i < strlines.Length; i++)
            {
                var toadd = new notedString(strlines[i], notelines[i]);
                addStrToList(toadd, strlist);
            }
            return strlist;
        }

        static public string getPathWithoutDir(string filename, string originalDir)
        {
            return filename.Substring(originalDir.Length + 1, filename.Length - originalDir.Length - 1);
        }
        static private string wrap(string s)
        {
            return s.Replace('・', '·').Replace('♪', '～');
        }

        static public void mergeTextInDir(string dir)
        {
            var outfn = Path.GetDirectoryName(dir) + "\\mergedString.txt";
            var outfnFull = Path.GetDirectoryName(dir) + "\\mergedStringFull.txt";
            var outfnClean = Path.GetDirectoryName(dir) + "\\mergedStringClean.txt";

            var strlist = new List<notedString>();
            var files = Directory.GetFiles(dir, "*.txt", SearchOption.TopDirectoryOnly);
            var splitC = "／＼：＿．/\\:_. @<>[]='\"t&|";
            foreach (var fl in files)
            {
                var fs = File.OpenRead(fl);
                var sr = new StreamReader(fs);
                var line = "";
                var lastline = "";
                while (sr.EndOfStream == false || fs.Position < fs.Length)
                {
                    lastline = line;
                    line = sr.ReadLine();
                    string note = null;
                    if (line.Length > 1 && line.Substring(0, 1) == "m")
                    {
                        if (lastline.Length > 1 && lastline.Substring(0, 1) == "#")
                        {
                            note = lastline.Substring(1, lastline.Length - 1);
                        }

                        var ind0 = line.IndexOf(' ', 0);
                        var ind1 = line.IndexOf(' ', ind0 + 1);
                        var inde = line.Substring(ind0 + 1, ind1 - ind0 - 1);
                        var ind = Convert.ToInt32(inde);
                        var msgTemp = line.Substring(ind1 + 1, line.Length - ind1 - 1);
                        var note2 = msgTemp + "&&";

                        var chars = msgTemp.ToArray();
                        int pt = 0;
                        int lastP = 0;
                        while (pt < msgTemp.Length)
                        {
                            if (splitC.Contains(chars[pt]))
                            {
                                var str = msgTemp.Substring(lastP, pt - lastP);
                                if (str != null && str != "")
                                {
                                    addStrToList(new notedString(str, note), strlist);
                                    addStrToList(new notedString(str, note2), strlist);
                                }
                                pt++;
                                lastP = pt;
                            }
                            else if (pt == msgTemp.Length - 1)
                            {
                                var str = msgTemp.Substring(lastP, pt + 1 - lastP);
                                if (str != null && str != "")
                                {
                                    addStrToList(new notedString(str, note), strlist);
                                    addStrToList(new notedString(str, note2), strlist);
                                }
                                pt++;
                                lastP = pt;
                            }
                            else
                            {
                                pt++;
                            }
                        }

                    }

                }
                sr.Close();
                fs.Close();
            }
            if (outfnFull != null)
            {
                var outfs = File.Open(outfnFull, FileMode.Create);
                var sw = new StreamWriter(outfs, utf8);
                for (int i = 0; i < strlist.Count; i++)
                {
                    if (strlist[i].n != null)
                    {
                        sw.WriteLine("#" + strlist[i].n);
                    }
                    sw.WriteLine("m " + i + " " + strlist[i].s);
                }
                sw.Close();
                outfs.Close();
            }
            if (outfn != null)
            {
                var outfs = File.Open(outfn, FileMode.Create);
                var sw = new StreamWriter(outfs, utf8);
                var outfs2 = File.Open(outfnClean, FileMode.Create);
                var sw2 = new StreamWriter(outfs2, utf8);
                var regex1 = new Regex(@"[＃〓■▼▲％？＠★＄][a-z][0-9][0-9]");
                var regex2 = new Regex(@"\[[0-9]+\]");
                for (int i = 0; i < strlist.Count; i++)
                {
                    if (strlist[i].n == null)
                    {
                        sw.WriteLine("m " + i + " " + strlist[i].s);
                        sw2.WriteLine("m " + i + " " + strlist[i].s);
                    }
                    else
                    {
                        var spistr = new string[] { "&&" };
                        var notes = strlist[i].n.Split(spistr, StringSplitOptions.RemoveEmptyEntries);
                        var needMod = false;
                        string noteImp = null;
                        for (int ij = 0; ij < notes.Length; ij++)
                        {
                            if (notes[ij] == null || notes[ij] == "")
                            {
                                continue;
                            }
                            if (notes[ij] == "NoChange")
                            {
                                needMod = false;
                                noteImp = "NoChange&&";
                                break;
                            }
                            else if (notes[ij].Contains("／") || notes[ij].Contains("SYS_") || notes[ij].Contains(".png") || notes[ij].Contains(".qnt"))
                            {
                                notes[ij] = null;
                            }
                            else if (notes[ij].Contains(".dcf") || notes[ij].Contains(".bmp") || notes[ij].Contains(".pactex") || notes[ij].Contains(".qnt"))
                            {
                                notes[ij] = null;
                            }
                            else if (notes[ij].Contains(".ogg") || regex1.IsMatch(notes[ij]) || notes[ij].Contains(@"\\") || notes[ij].Contains(".jam"))
                            {
                                notes[ij] = null;
                            }
                            else if (regex2.IsMatch(notes[ij]))
                            {
                                noteImp = notes[ij] + "&&";
                            }
                            else
                            {
                                needMod = true;
                            }
                        }
                        if (needMod)
                        {
                            string note = "";
                            for (int ir = 0; ir < notes.Length; ir++)
                            {
                                if (notes[ir] != null && notes[ir] != "")
                                {
                                    note += notes[ir] + "&&";
                                }
                            }
                            if (note != strlist[i].s + "&&")
                            {
                                //sw.WriteLine("#" + note);
                            }
                            if (noteImp != null)
                            {
                                sw.WriteLine("#" + noteImp);
                                sw2.WriteLine("#" + noteImp);
                            }
                            sw.WriteLine("m " + i + " " + strlist[i].s);
                            sw2.WriteLine("m " + i + " " + strlist[i].s);
                        }
                        else
                        {
                            if (noteImp != null)
                            {
                                sw.WriteLine("#" + noteImp);
                            }
                            sw.WriteLine("m " + i + " " + strlist[i].s);
                        }
                    }
                }
                sw.Close();
                outfs.Close();
                sw2.Close();
                outfs2.Close();
            }
        }

        static public void mergeTextInDirPlain(string dir)
        {
            var outfnFull = Path.GetDirectoryName(dir) + "\\mergedString.txt";

            var strlist = new List<notedString>();
            var files = Directory.GetFiles(dir, "*.txt", SearchOption.AllDirectories);
            var splitC = "／＼：＿．/\\:_. @<>[]='\"t&|";
            foreach (var fl in files)
            {
                var fs = File.OpenRead(fl);
                var sr = new StreamReader(fs);
                var line = "";
                var lastline = "";
                while (sr.EndOfStream == false || fs.Position < fs.Length)
                {
                    lastline = line;
                    line = sr.ReadLine();
                    string note = null;
                    if (line.StartsWith("#"))
                        continue;

                    if (lastline.Length > 1 && lastline.Substring(0, 1) == "#")
                    {
                        note = lastline.Substring(1, lastline.Length - 1);
                    }

                    var msgTemp = line;
                    var note2 = msgTemp + "&&";

                    addStrToList(new notedString(msgTemp, note), strlist);

                    //var chars = msgTemp.ToArray();
                    //int pt = 0;
                    //int lastP = 0;
                    //while (pt < msgTemp.Length)
                    //{
                    //    if (splitC.Contains(chars[pt]))
                    //    {
                    //        var str = msgTemp.Substring(lastP, pt - lastP);
                    //        if (str != null && str != "")
                    //        {
                    //            addStrToList(new notedString(str, note), strlist);
                    //            addStrToList(new notedString(str, note2), strlist);
                    //        }
                    //        pt++;
                    //        lastP = pt;
                    //    }
                    //    else if (pt == msgTemp.Length - 1)
                    //    {
                    //        var str = msgTemp.Substring(lastP, pt + 1 - lastP);
                    //        if (str != null && str != "")
                    //        {
                    //            addStrToList(new notedString(str, note), strlist);
                    //            addStrToList(new notedString(str, note2), strlist);
                    //        }
                    //        pt++;
                    //        lastP = pt;
                    //    }
                    //    else
                    //    {
                    //        pt++;
                    //    }
                    //}

                }
                sr.Close();
                fs.Close();
            }
            if (outfnFull != null)
            {
                var outfs = File.Open(outfnFull, FileMode.Create);
                var sw = new StreamWriter(outfs, utf8);
                for (int i = 0; i < strlist.Count; i++)
                {
                    sw.WriteLine(strlist[i].s);
                }
                sw.Close();
                outfs.Close();
            }
        }

        static public void disperseTextIntoDir(string dir)
        {
            var fn = Path.GetDirectoryName(dir) + "\\mergedString.txt";
            var orifn = Path.GetDirectoryName(dir) + "\\mergedStringOri.txt";
            var outdir = dir + "Out";
            if (!Directory.Exists(outdir))
            {
                Directory.CreateDirectory(outdir);
            }

            var strlist = readStringFile(fn, orifn);

            var files = Directory.GetFiles(dir, "*.txt", SearchOption.TopDirectoryOnly);
            var splitC = "／＼：＿．/\\:_. @<>[]='\"t&|";
            foreach (var fl in files)
            {
                var fs = File.OpenRead(fl);
                var sr = new StreamReader(fs, utf8);
                var outfn = Path.Combine(outdir, Path.GetFileName(fl));
                var outfs = File.Open(outfn, FileMode.Create);
                var sw = new StreamWriter(outfs, utf8);
                var line = "";
                var lastline = "";
                while (sr.EndOfStream == false || fs.Position < fs.Length)
                {
                    lastline = line;
                    line = sr.ReadLine();
                    string note = null;
                    if (line.Length > 1 && line.Substring(0, 1) == "m")
                    {
                        if (lastline.Length > 1 && lastline.Substring(0, 1) == "#")
                        {
                            note = lastline.Substring(1, lastline.Length - 1);
                        }

                        var ind0 = line.IndexOf(' ', 0);
                        var ind1 = line.IndexOf(' ', ind0 + 1);
                        var inde = line.Substring(ind0 + 1, ind1 - ind0 - 1);
                        var ind = Convert.ToInt32(inde);
                        var msgTemp = line.Substring(ind1 + 1, line.Length - ind1 - 1);

                        var chars = msgTemp.ToArray();
                        int pt = 0;
                        int lastP = 0;
                        var newLine = line.Substring(0, ind1 + 1);
                        while (pt < msgTemp.Length)
                        {
                            if (splitC.Contains(chars[pt]))
                            {
                                var str = msgTemp.Substring(lastP, pt - lastP);
                                if (str != null && str != "")
                                {
                                    var nStr = getNoteFromList(str, strlist);
                                    if (nStr == null)
                                    {
                                        newLine += wrap(str);
                                    }
                                    else if (nStr == "*-")
                                    {
                                        pt++;
                                        lastP = pt;
                                        continue;
                                    }
                                    else
                                    {
                                        newLine += nStr;
                                    }
                                }
                                newLine += chars[pt];
                                pt++;
                                lastP = pt;
                            }
                            else if (pt == msgTemp.Length - 1)
                            {
                                var str = msgTemp.Substring(lastP, pt + 1 - lastP);
                                if (str != null && str != "")
                                {
                                    var nStr = getNoteFromList(str, strlist);
                                    if (nStr == null)
                                    {
                                        newLine += wrap(str);
                                    }
                                    else
                                    {
                                        newLine += nStr;
                                    }
                                }
                                pt++;
                                lastP = pt;
                            }
                            else
                            {
                                pt++;
                            }
                        }
                        sw.WriteLine(newLine);

                    }
                    else
                    {
                        sw.WriteLine(line);
                    }

                }
                sr.Close();
                fs.Close();
                sw.Close();
                outfs.Close();
            }

        }

        static public void disperseTextIntoDirPlain(string dir)
        {
            var fn = Path.GetDirectoryName(dir) + "\\mergedString.txt";
            var orifn = Path.GetDirectoryName(dir) + "\\mergedStringOri.txt";
            var outdir = dir + "Out";

            var strlist = readStringFilePlain(fn, orifn);

            var files = Directory.GetFiles(dir, "*.txt", SearchOption.AllDirectories);
            var splitC = "／＼：＿．/\\:_. @<>[]='\"t&|";
            foreach (var fl in files)
            {
                var fs = File.OpenRead(fl);
                var sr = new StreamReader(fs, utf8);
                var outfn = Path.Combine(outdir, getPathWithoutDir(fl, dir));
                if (!Directory.Exists(Path.GetDirectoryName(outfn)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outfn));
                }
                var outfs = File.Open(outfn, FileMode.Create);
                var sw = new StreamWriter(outfs, utf8);
                var line = "";
                var lastline = "";
                while (sr.EndOfStream == false || fs.Position < fs.Length)
                {
                    lastline = line;
                    line = sr.ReadLine();
                    string note = null;
                    if (line.StartsWith("#"))
                    {
                        sw.WriteLine(line);
                        continue;
                    }

                    if (lastline.Length > 1 && lastline.Substring(0, 1) == "#")
                    {
                        note = lastline.Substring(1, lastline.Length - 1);
                    }

                    var msgTemp = line;

                    var newLine = msgTemp;
                    var nStr = getNoteFromList(msgTemp, strlist);
                    if (nStr == null)
                    {
                        newLine = msgTemp;
                    }
                    else
                    {
                        newLine = nStr;
                    }

                    //var chars = msgTemp.ToArray();
                    //int pt = 0;
                    //int lastP = 0;
                    //var newLine = line.Substring(0, ind1 + 1);
                    //while (pt < msgTemp.Length)
                    //{
                    //    if (splitC.Contains(chars[pt]))
                    //    {
                    //        var str = msgTemp.Substring(lastP, pt - lastP);
                    //        if (str != null && str != "")
                    //        {
                    //            var nStr = getNoteFromList(str, strlist);
                    //            if (nStr == null)
                    //            {
                    //                newLine += wrap(str);
                    //            }
                    //            else if (nStr == "*-")
                    //            {
                    //                pt++;
                    //                lastP = pt;
                    //                continue;
                    //            }
                    //            else
                    //            {
                    //                newLine += nStr;
                    //            }
                    //        }
                    //        newLine += chars[pt];
                    //        pt++;
                    //        lastP = pt;
                    //    }
                    //    else if (pt == msgTemp.Length - 1)
                    //    {
                    //        var str = msgTemp.Substring(lastP, pt + 1 - lastP);
                    //        if (str != null && str != "")
                    //        {
                    //            var nStr = getNoteFromList(str, strlist);
                    //            if (nStr == null)
                    //            {
                    //                newLine += wrap(str);
                    //            }
                    //            else
                    //            {
                    //                newLine += nStr;
                    //            }
                    //        }
                    //        pt++;
                    //        lastP = pt;
                    //    }
                    //    else
                    //    {
                    //        pt++;
                    //    }
                    //}
                    sw.WriteLine(newLine);
                }
                sr.Close();
                fs.Close();
                sw.Close();
                outfs.Close();
            }

        }


    }

}
