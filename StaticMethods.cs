/******************************************************************************\
 * IceChat 9 Internet Relay Chat Client
 *
 * Copyright (C) 2013 Paul Vanderzee <snerf@icechat.net>
 *                                    <www.icechat.net> 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 * Please consult the LICENSE.txt file included with this project for
 * more details
 *
\******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace IceChat
{
    public static class StaticMethods
    {
        /// <summary>
        /// Load in embedded resources from Stream and add it to the sent object's Image Property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="resourceName"></param>
        public static void LoadResourceImage(object sender, string resourceName)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            Stream s;
            //if (IsRunningOnMono())
            //    s = a.GetManifestResourceStream(@"IceChat.Icons\" + resourceName);
            //else
                s = a.GetManifestResourceStream("IceChat.Icons." + resourceName);
            
            if (s == null)
                return;

            Bitmap b;
            if (resourceName.EndsWith("ico"))
                b = new Icon(s).ToBitmap();
            else
                b = new Bitmap(s);

            if (sender.GetType() == typeof(Button))
                ((Button)sender).Image = b;
            else if (sender.GetType() == typeof(ToolStripMenuItem))
                ((ToolStripMenuItem)sender).Image = b;

        }
        
        /// <summary>
        /// Load in embedded resources from Stream
        /// </summary>
        /// <param name="resourceName">The resource name of the Image</param>
        public static Bitmap LoadResourceImage(string resourceName)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            Stream s;
            //if (IsRunningOnMono())
            //    s = a.GetManifestResourceStream(@"IceChat.Icons\" + resourceName);
            //else
                s = a.GetManifestResourceStream("IceChat.Icons." + resourceName);
            
            if (s == null)
                return null;
            
            if (resourceName.EndsWith("ico"))
                return new Icon(s).ToBitmap();

            return new Bitmap(s);
        }
        
        /// <summary>
        /// Check if we are running in the Mono runtime or not
        /// </summary>
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
