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
// Version: 18.09.27
// EndLic
﻿using System;
using Gtk;
using TrickyUnits;
using TrickyUnits.GTK;

namespace GJCR
{
    class MainClass
    {
        // Variables

        // Config
        const int ww = 1500;
        const int wh = 1000;
        static TGINI Config;

        // GUI: Main
        static MainWindow win;
        static VBox whole;

        // GUI: Comments
        static HBox boxComments;
        static ListBox listComments;
        static TextView viewComment;

        // GUI: Entry view
        static HBox boxEntries;
        static Button bView;
        static Button bExtract;
        static Button bExtractAll;
        static Button bInfo;
        static Entry eExtractAll;

        // Functions

        public static void Init(string[] args) // Args are copied in order to allow direct JCR6 loading from the command line
        {
            MKL.Lic    ("GJCR6 for .NET - GJCR6.cs","GNU General Public License 3");
            MKL.Version("GJCR6 for .NET - GJCR6.cs","18.09.27");
            Application.Init();
        }

        public static void ComposeGUI()
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
            boxComments.SetSizeRequest(ww, 250);
            listComments = new ListBox("Comments");
            listComments.SetSizeRequest(300, 250);
            boxComments.Add(QuickGTK.Scroll(listComments.Gadget));
            viewComment = new TextView();
            viewComment.ModifyBase(StateType.Normal,new Gdk.Color( 0, 18, 25));
            viewComment.ModifyText(StateType.Normal, new Gdk.Color(0, 180, 255));
            viewComment.Editable = false;
            viewComment.SetSizeRequest(ww - 300, 250);
            boxComments.Add(QuickGTK.Scroll(viewComment));
            whole.Add(boxComments);
            // Entries
            boxEntries = new HBox();
            boxEntries.SetSizeRequest(ww, wh - 250);
            whole.Add(boxEntries);
        }

        public static void Run(){
            win.ShowAll();
            Application.Run();
        }

        public static void Main(string[] args){
            Init(args);
            ComposeGUI();
            Run();
        }

    }
}
