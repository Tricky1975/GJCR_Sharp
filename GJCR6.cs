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
        static TGINI Config;

        // GUI: Main
        static MainWindow win;

        // GUI: Comments
        static HBox boxComments;
        static ListBox listComments;

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
            win = new MainWindow();
        }

        public static void Run(){
            win.Show();
            Application.Run();
        }

        public static void Main(string[] args){
            Init(args);
            ComposeGUI();
            Run();
        }

    }
}
