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
// Version: 18.09.28
// EndLic
﻿using System;
using System.Collections.Generic;
using Gtk;
using TrickyUnits;
using TrickyUnits.GTK;
using UseJCR6;

namespace GJCR
{
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
        static bool hasfile { get { return jcr != null; } }
        static bool hasentry { get { return false; }} // TODO: Make this respond properly to having an entry

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


        // Callback functions
        static void OnOpen(object sender,EventArgs e){
            var file = QuickGTK.RequestFile("Please choose a JCR6 compatible file");
            if (file!="") Load(file);
        }

        static void OnComments(object sender,EventArgs e){
            viewComment.Buffer.Text = $"{listComments.ItemText}\n\n{jcr.Comments[listComments.ItemText]}";
        }


        // Sub functions
        static void Load(string j){
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
                lsEntries.AppendValues(e.Entry, $"{e.Size}", $"{e.CompressedSize}", e.Ratio, e.Storage,e.MainFile, e.Author, e.Notes);
            }
            nvEntries.Model = lsEntries;

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
            MKL.Version("GJCR6 for .NET - GJCR6.cs","18.09.28");
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
            Application.Init();
        }

        static void Done(){
            Console.WriteLine("Saving config: " + configfile);
            Config.SaveSource(configfile);
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
            bExtract = new Button("Extract entry"); EntryRequired.Add(bExtract);
            bExtractAll = new Button("Extract all entries to: "); FileRequired.Add(bExtractAll);
            eExtractAll = new Entry(); FileRequired.Add(eExtractAll);
            bInfo = new Button("Entry Info"); EntryRequired.Add(bInfo);
            cbRecent = new ComboBox();
            nvEntries = new TreeView(); FileRequired.Add(nvEntries);
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
