// Lic:
// 	GJCR6 for .NET
// 	
// 	
// 	
// 	
// 	(c) Jeroen P. Broks, 2018, All rights reserved
// 	
// 		This program is free software: you can redistribute it and/or modify
// 		it under the terms of the GNU General Public License as published by
// 		the Free Software Foundation, either version 3 of the License, or
// 		(at your option) any later version.
// 		
// 		This program is distributed in the hope that it will be useful,
// 		but WITHOUT ANY WARRANTY; without even the implied warranty of
// 		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// 		GNU General Public License for more details.
// 		You should have received a copy of the GNU General Public License
// 		along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 		
// 	Exceptions to the standard GNU license are available with Jeroen's written permission given prior 
// 	to the project the exceptions are needed for.
// Version: 18.09.30
// EndLic
﻿using System;
using System.Media;
using System.Collections.Generic;
using Gtk;
using TrickyUnits;
using TrickyUnits.GTK;
using UseJCR6;

namespace GJCR
{

    class GJCR_View{
        Window vWin;

        void OnClose(object sender, EventArgs e) { vWin.Destroy(); }

        public GJCR_View(Widget view,string Caption,bool vp=false){
            vWin = new Window(Caption);
            vWin.SetSizeRequest(900, 800);
            var box = new VBox();
            var scroll = new ScrolledWindow();
            if (vp) scroll.AddWithViewport(view);  else            scroll.Add(view);
            scroll.SetSizeRequest(900, 775);
            box.Add(scroll);
            var close = new Button("Close");
            box.Add(close);
            close.SetSizeRequest(900, 25);
            close.Clicked += OnClose;
            vWin.Add(box);
            vWin.ShowAll();
        }
    }

    delegate void CreateView (string filename);

    class GJCR_Main
    {
        // Variables

        // Config
        const int ww = 1500;
        const int wh = 1000;
        static readonly string configdir;
        static string configfile { get { return $"{configdir}Config.GINI"; }}
        static TGINI Config;
        static TJCRDIR jcr;
        static readonly List<Widget> FileRequired = new List<Widget>();
        static readonly List<Widget> EntryRequired = new List<Widget>();
        static string filename = "";
        static string file_extension { get => System.IO.Path.GetExtension(filename); }
        static string entrykey { get => filename.ToUpper();  }
        static TJCREntry entry { get {
                if (!jcr.Entries.ContainsKey(entrykey)) return null;
                return jcr.Entries[entrykey];
            }}
        static bool hasfile { get { return jcr != null; } }
        static bool hasentry { get { return hasfile && filename != ""; }} // TODO: Make this respond properly to having an entry
        static bool knownstorage {
            get {
                var b = JCR6.CompDrivers.ContainsKey(entry.Storage);
                if (!b) QuickGTK.Error($"ERROR!\n\nThere is no driver loaded for storage method {entry.Storage}");
                return b;
            }
        }

        // GUI: Main
        static MainWindow win;
        static VBox whole;

        // GUI: Comments
        static HBox boxComments;
        static ListBox listComments;
        static TextView viewComment;

        // GUI: Entry view
        static VBox boxEntries;
        static HBox boxButtons;
        static Button bOpen;
        static Button bView;
        static Button bExtract;
        static Button bExtractAll;
        static Button bInfo;
        static ComboBox cbRecent;
        static Entry eExtractAll;
        static ScrolledWindow sEntries;
        static TreeView nvEntries;
        static ListStore lsEntries;

        // Audio "view"
        //static SoundPlayer playsound;
        static List<string> TempAudio = new List<string>();


        // View functions
        static Dictionary<string, CreateView> CV = new Dictionary<string, CreateView>();

        static void ViewText(string filename){
            var tv = new TextView();
            tv.Buffer.Text=jcr.LoadString(filename);
            var font = new Pango.FontDescription();
            font.Family = "Courier";
            tv.ModifyFont(font);
            tv.ModifyBase(StateType.Normal, new Gdk.Color(18, 0, 25));
            tv.ModifyText(StateType.Normal, new Gdk.Color(0, 180, 255));
            tv.Editable = false;
            new GJCR_View(tv,$"Showing {filename}");
        }

        static void ViewImage(string filename){
            var s = jcr.ReadFile(filename);
            var b = s.ReadBytes((int)s.Size); s.Close();
            var t = configdir + "temp" + System.IO.Path.GetExtension(filename);
            s = QOpen.WriteFile(t);
            s.WriteBytes(b);
            s.Close();
            var img = new Image(t);
            img.Visible = true;
            //var bx = new VBox(); bx.Add(img);
            //var img = new Image(s.GetStream());
            new GJCR_View(img, $"Showing image: {filename}",true);
            System.IO.File.Delete(t);
        }

        static void ViewHEX(string filename){
            var s = jcr.ReadFile(filename);
            var b = s.ReadBytes((int)s.Size);
            var py = -1;
            var px = 16;
            var dump = "........   00 01 02 03   04 05 06 07   08 09 0A 0B   0C 0D 0E 0F\n";
            var tdmp = "";
            for (int i = 0; i < b.Length;i++){
                px++;
                if (px > 0xf) { 
                    px = 0; 
                    py++;
                    dump += $" {tdmp}\n";
                    tdmp = "";
                    var pos = py * 0x10;
                    dump += $"{pos.ToString("X8")} ";
                }
                var cb = b[i];
                if (px % 4 == 0) dump += "  ";
                dump += $"{cb.ToString("X2")} ";
                if (cb > 31 && cb < 127) tdmp += qstr.Chr(cb); else tdmp += ".";
            }
            while (px<0xf){
                px++;
                if (px % 4 == 0) dump += "  ";
                dump += ".. ";
            }
            dump += $" {tdmp}";
            var tv = new TextView();
            tv.Buffer.Text = dump;
            var font = new Pango.FontDescription();
            font.Family = "Courier";
            tv.ModifyFont(font);
            tv.ModifyBase(StateType.Normal, new Gdk.Color(25, 18, 0));
            tv.ModifyText(StateType.Normal, new Gdk.Color(255, 180, 0));
            tv.Editable = false;
            new GJCR_View(tv, $"Showing HEX {filename}");

        }

        static void ViewAudio(string filename){
            /*
            var s = jcr.ReadFile(filename);
            if (playsound!=null) playsound.Stop(); 
            if (s == null) { QuickGTK.Error($"Reading {filename} failed\n\n{JCR6.JERROR}"); }
            playsound = new SoundPlayer(s.GetStream());
            s.Close();
            playsound.Play();
            */
            var i = -1;
            var s = "";
            var e = System.IO.Path.GetExtension(filename);
            do
            {
                i++;
                s = i.ToString("X8") + e;
            } while (System.IO.File.Exists(s));
            var bi = jcr.ReadFile(filename);
            var b = bi.ReadBytes((int)bi.Size); bi.Close();
            var bo = QOpen.WriteFile(s);
            bo.WriteBytes(b);
            bo.Close();
            // Only works in unix based systems
            // A windows solution will be implemented later.
            System.Diagnostics.Process.Start("mplayer", s);
            TempAudio.Add(s);
        }


        // Callback functions
        static void OnOpen(object sender,EventArgs e){
            var file = QuickGTK.RequestFile("Please choose a JCR6 compatible file");
            if (file!="") Load(file);
        }

        static void OnComments(object sender,EventArgs e){
            viewComment.Buffer.Text = $"{listComments.ItemText}\n\n{jcr.Comments[listComments.ItemText]}";
        }

        static void OnView(object sender, EventArgs e)
        {
            string ex;
            if (!knownstorage) return;
            ex = System.IO.Path.GetExtension(filename).ToUpper();
            if (CV.ContainsKey(ex)) CV[ex](filename); else {
                //QuickGTK.Error("There is no support YET to view that kind of file");
                ViewHEX(filename);
            }

        }

        static void OnEntrySelect(object sender,EventArgs e){
            TreeSelection selection = nvEntries.Selection;
            TreeModel model;
            TreeIter iter;
            if (selection.GetSelected(out model, out iter))
            {
                filename = (model.GetValue(iter, 0) as string);
            }
            AutoEnable();
        }

        static void OnExtract(object sender,EventArgs e){
            if (!knownstorage) return;
            var fn = QuickGTK.RequestFileSave($"Extract {filename} to:");
            var fe = System.IO.Path.GetExtension(fn);
            if (fe!=file_extension) {
                if (!QuickGTK.Confirm($"WARNING!\nThe file extension of the entry inside this JCR resource ({filename}) differs from the one in the file name you are extracting the data to({fn}).\n\nAlthough the data will just be handled normally, this can lead some programs to get confused.\n\nDo you wish to continue?", MessageType.Warning)) return;
            }
            var bo = QOpen.WriteFile(fn);
            //var bytes = jcr.JCR_B(filename);
            bo.WriteBytes(jcr.JCR_B(filename));
            bo.Close();
        }

        static void OnRecent(object sender,EventArgs e){
            Load(cbRecent.ActiveText);
        }

        static void OnExtractAll(object sender, EventArgs a){
            var xto = eExtractAll.Text.Replace("\\","/");
            if (xto == "/") { QuickGTK.Error("I need a folder to extract everything to!"); return; }
            if (qstr.Right(xto, 1) != "/") xto += "/";
            if (!QuickGTK.Confirm($"Extract all entries to {xto}?")) return;
            win.Hide();
            var i = 0;
            var c = jcr.Entries.Count;
            var r = System.IO.Directory.CreateDirectory(xto);
            if (!r.Exists) { QuickGTK.Error("Creating dir failed");  return; }
            var twin = new Window("Extracting");
            twin.SetSizeRequest(400, 30);
            var tlab = new Label("...");
            twin.Add(tlab);
            twin.ShowAll();
            var success = 0;
            var failed = 0;
            foreach(TJCREntry e in jcr.Entries.Values ){
                i++;
                tlab.Text = $"{i}/{c}: Extracting {e.Entry}";
                var outfile = $"{xto}{e.Entry}";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outfile));

                var b = jcr.JCR_B(e.Entry);
                if (b==null){
                    QuickGTK.Error($"Extracting {e.Entry} failed!\n\n{JCR6.JERROR}");
                    failed++;
                } else {
                    var bt = QOpen.WriteFile(outfile);
                    bt.WriteBytes(b);
                    bt.Close();
                    success++;
                }
            }
            twin.Destroy();
            QuickGTK.Info($"Extraction complete\nSuccess: {success}\nFailed {failed}");
            win.ShowAll();
        }


        // Sub functions
        static void Load(string j){
            string rPrefix = System.IO.Path.GetDirectoryName(j).Replace(@"\","/");
            if (!qstr.Suffixed(rPrefix, "/") && (rPrefix.Length != 2 && qstr.Mid(rPrefix, 2, 1) != ":")) rPrefix += "/";
            if (JCR6.Recognize(j).ToUpper()=="NONE"){
                QuickGTK.Error($"JCR6 ERROR!\n\nNone of the loaded file drivers recognized the file:\n{j}");
                return;
            }
            var tempjcr = JCR6.Dir(j);
            if ( JCR6.JERROR!=""){
                QuickGTK.Error($"JCR6 ERROR!\n\nLoading \"{j}\" failed:\n\n{JCR6.JERROR}");
                return;
            }
            jcr = tempjcr;
            listComments.Clear();
            foreach (string ck in jcr.Comments.Keys) listComments.AddItem(ck);
            AutoEnable();
            lsEntries.Clear();
            foreach(TJCREntry e in jcr.Entries.Values){
                var tMainFile = e.MainFile.Replace(@"\", "/");
                //Console.WriteLine(rPrefix + "\n" + tMainFile);
                if (qstr.Prefixed(tMainFile, rPrefix)) tMainFile = qstr.Right(tMainFile, tMainFile.Length - rPrefix.Length);
                lsEntries.AppendValues(e.Entry, $"{e.Size}", $"{e.CompressedSize}", e.Ratio, e.Storage,tMainFile, e.Author, e.Notes);
            }
            nvEntries.Model = lsEntries;

            var l = Config.List("Recent_JCR");
            l.Remove(j);
            l.Insert(0, j);
            /*
            var rls = new ListStore(typeof(string));
            foreach (string rs in l) rls.AppendValues(l);
            cbRecent.Clear();
            cbRecent.Model = rls;
            */

        }

        static void AutoEnable(){
            foreach (Widget w in FileRequired) w.Sensitive = hasfile;
            foreach (Widget w in EntryRequired) w.Sensitive = hasentry;
        }


        // Functions for main stuff

        static GJCR_Main(){
            Dirry.Add("___APPLICATIONNAME___", "GJCR6");
            configdir = Dirry.C("$AppSupport$/GJCR6CS/"); //Config.GINI");
            Console.WriteLine("Config file: " + configfile);
            if (!System.IO.Directory.Exists(configdir)) {
                Console.WriteLine("Creating dir: " + configdir);
                System.IO.Directory.CreateDirectory(configdir);
            }
        }

        public static void Init(string[] args) // Args are copied in order to allow direct JCR6 loading from the command line
        {
            MKL.Lic    ("GJCR6 for .NET - GJCR6.cs","GNU General Public License 3");
            MKL.Version("GJCR6 for .NET - GJCR6.cs","18.09.30");
            if (System.IO.File.Exists(configfile))
            {
                Console.WriteLine($"Loading: {configfile}");
                Config = GINI.ReadFromFile(configfile);
            }
            else
                Config = new TGINI();
            JCR6_zlib.Init();
            JCR6_lzma.Init();
            new JCR6_WAD();
            new JCR6_RealDir();
            new JCR_QuakePack();
            string[] txt = { "MD", "TXT", "LUA", "BLP", "SH", "BAT", "PHP","GINI" };
            string[] img = { "PNG", "GIF", "BMP", "JPG", "ICNS", "ICO", "JPEG", "PCX", "TGA", "TIFF" };
            string[] aud = { "WAV", "FLAC","OGG","MP3" };
            foreach (string k in txt) CV[$".{k}"] = ViewText;
            foreach (string k in img) CV[$".{k}"] = ViewImage;
            foreach (string k in aud) CV[$".{k}"] = ViewAudio;
            // foreach (Gdk.PixbufFormat f in Gdk.Pixbuf.Formats) Console.WriteLine($"Image format: {f.Name}"); // debug
            Application.Init();
        }

        static void Done(){
            Console.WriteLine("Saving config: " + configfile);
            Config.SaveSource(configfile);
            foreach (string s in TempAudio) { System.IO.File.Delete(s); Console.WriteLine("Deleted: " + s); }
        }

        static void InitEntryTreeView()
        {
            string[] nodes = { "Entry", "Size", "Compressed Size", "Ratio", "Storage", "Main File", "Author", "Notes" };
            bool[] nright = { false, true, true, true, false, false, false };
            lsEntries = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),typeof(string),typeof(string), typeof(string));
            for (int i = 0; i < nodes.Length; i++)
            {
                var tvc = new TreeViewColumn();
                var NameCell = new CellRendererText();
                tvc.Title = nodes[i];
                tvc.PackStart(NameCell, true);
                tvc.AddAttribute(NameCell, "text", i);
                nvEntries.AppendColumn(tvc);
            }

        }

        static void ComposeGUI()
        {
            // Window
            win = new MainWindow();
            whole = new VBox();
            win.Add(whole);
            win.Title = $"GJCR6 - Version {MKL.Newest} - Coded by: Tricky";
            win.SetSizeRequest(ww, wh);
            win.ModifyBg(StateType.Normal,new Gdk.Color(0, 0, 0));
            // Comments
            boxComments = new HBox();
            FileRequired.Add(boxComments);
            boxComments.SetSizeRequest(ww, 250);
            listComments = new ListBox("Comments");
            listComments.SetSizeRequest(300, 250);
            listComments.Gadget.CursorChanged += OnComments;
            boxComments.Add(QuickGTK.Scroll(listComments.Gadget));
            viewComment = new TextView();
            viewComment.ModifyBase(StateType.Normal,new Gdk.Color( 0, 18, 25));
            viewComment.ModifyText(StateType.Normal, new Gdk.Color(0, 180, 255));
            viewComment.Editable = false;
            viewComment.SetSizeRequest(ww - 300, 250);
            boxComments.Add(QuickGTK.Scroll(viewComment));
            whole.Add(boxComments);
            // Entries
            boxEntries = new VBox();
            boxEntries.SetSizeRequest(ww, wh - 250);
            whole.Add(boxEntries);
            boxButtons = new HBox();
            boxButtons.SetSizeRequest(ww, 25);
            bOpen = new Button("Open JCR");
            bOpen.Clicked += OnOpen;
            bView = new Button("View"); EntryRequired.Add(bView);
            bView.Clicked += OnView;
            bExtract = new Button("Extract entry"); EntryRequired.Add(bExtract);
            bExtract.Clicked += OnExtract;
            bExtractAll = new Button("Extract all entries to: "); FileRequired.Add(bExtractAll);
            eExtractAll = new Entry(); FileRequired.Add(eExtractAll);
            bInfo = new Button("Entry Info"); EntryRequired.Add(bInfo);
            bExtractAll.Clicked += OnExtractAll;
            cbRecent = new ComboBox(); //Config.List("Recent_JCR").ToArray());
            CellRendererText text = new CellRendererText();
            cbRecent.PackStart(text, false);
            cbRecent.AddAttribute(text, "text", 0);
            cbRecent.Changed += OnRecent;
            var ls = new ListStore(typeof(string));
            foreach (string rf in Config.List("Recent_JCR")) ls.AppendValues(rf);
            cbRecent.Model = ls;
            nvEntries = new TreeView(); FileRequired.Add(nvEntries);
            nvEntries.CursorChanged += OnEntrySelect;
            nvEntries.RulesHint = true;
            sEntries = QuickGTK.Scroll(nvEntries);
            sEntries.SetSizeRequest(ww, (wh - 250) - 25);
            InitEntryTreeView();
            boxButtons.Add(bOpen);
            boxButtons.Add(bView);
            boxButtons.Add(bExtract);
            boxButtons.Add(bExtractAll); boxButtons.Add(eExtractAll);
            boxButtons.Add(cbRecent);
            boxEntries.Add(boxButtons);
            boxEntries.Add(sEntries);
            AutoEnable();
        }

        public static void Run(){
            win.ShowAll();
            Application.Run();
        }

        public static void Main(string[] args){
            Init(args);
            ComposeGUI();
            Run();
            Done();
        }

    }
}
