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
using System.Drawing;

namespace IceChat
{
    public static class IrcColor
    {
        public static Color[] colors;

        static IrcColor()
        {
            //Color color;
            colors = new Color[72];

            colors[0] = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            colors[1] = System.Drawing.ColorTranslator.FromHtml("#000000");
            colors[2] = System.Drawing.ColorTranslator.FromHtml("#00007F");
            colors[3] = System.Drawing.ColorTranslator.FromHtml("#009300");
            colors[4] = System.Drawing.ColorTranslator.FromHtml("#FF0000");
            colors[5] = System.Drawing.ColorTranslator.FromHtml("#7F0000");
            colors[6] = System.Drawing.ColorTranslator.FromHtml("#9C009C");
            colors[7] = System.Drawing.ColorTranslator.FromHtml("#FC7F00");

            colors[8] = System.Drawing.ColorTranslator.FromHtml("#FFFF00");
            colors[9] = System.Drawing.ColorTranslator.FromHtml("#00FC00");
            colors[10] = System.Drawing.ColorTranslator.FromHtml("#009393");
            colors[11] = System.Drawing.ColorTranslator.FromHtml("#00FFFF");
            colors[12] = System.Drawing.ColorTranslator.FromHtml("#0000FC");
            colors[13] = System.Drawing.ColorTranslator.FromHtml("#FF00FF");
            colors[14] = System.Drawing.ColorTranslator.FromHtml("#7F7F7F");
            colors[15] = System.Drawing.ColorTranslator.FromHtml("#D2D2D2");

            colors[16] = System.Drawing.ColorTranslator.FromHtml("#CCFFCC");
            colors[17] = System.Drawing.ColorTranslator.FromHtml("#0066FF");
            colors[18] = System.Drawing.ColorTranslator.FromHtml("#FAEBD7");
            colors[19] = System.Drawing.ColorTranslator.FromHtml("#FFD700");
            colors[20] = System.Drawing.ColorTranslator.FromHtml("#E6E6E6");
            colors[21] = System.Drawing.ColorTranslator.FromHtml("#4682B4");
            colors[22] = System.Drawing.ColorTranslator.FromHtml("#993333");
            colors[23] = System.Drawing.ColorTranslator.FromHtml("#FF99FF");

            colors[24] = System.Drawing.ColorTranslator.FromHtml("#DDA0DD");
            colors[25] = System.Drawing.ColorTranslator.FromHtml("#8B4513");
            colors[26] = System.Drawing.ColorTranslator.FromHtml("#CC0000");
            colors[27] = System.Drawing.ColorTranslator.FromHtml("#FFFF99");
            colors[28] = System.Drawing.ColorTranslator.FromHtml("#339900");
            colors[29] = System.Drawing.ColorTranslator.FromHtml("#FF9900");

            colors[30] = System.Drawing.ColorTranslator.FromHtml("#FFDAB9");
            colors[31] = System.Drawing.ColorTranslator.FromHtml("#2F4F4F");
            colors[32] = System.Drawing.ColorTranslator.FromHtml("#ECE9D8");
            colors[33] = System.Drawing.ColorTranslator.FromHtml("#5FDAEE");
            colors[34] = System.Drawing.ColorTranslator.FromHtml("#E2FF00");
            colors[35] = System.Drawing.ColorTranslator.FromHtml("#00009E");

            colors[36] = System.Drawing.ColorTranslator.FromHtml("#FFFFCC");
            colors[37] = System.Drawing.ColorTranslator.FromHtml("#FFFF99");
            colors[38] = System.Drawing.ColorTranslator.FromHtml("#FFFF66");
            colors[39] = System.Drawing.ColorTranslator.FromHtml("#FFCC33");
            colors[40] = System.Drawing.ColorTranslator.FromHtml("#FF9933");
            colors[41] = System.Drawing.ColorTranslator.FromHtml("#FF6633");

            colors[42] = System.Drawing.ColorTranslator.FromHtml("#c6ffc6");
            colors[43] = System.Drawing.ColorTranslator.FromHtml("#84ff84");
            colors[44] = System.Drawing.ColorTranslator.FromHtml("#00ff00");
            colors[45] = System.Drawing.ColorTranslator.FromHtml("#00c700");
            colors[46] = System.Drawing.ColorTranslator.FromHtml("#008600");
            colors[47] = System.Drawing.ColorTranslator.FromHtml("#004100");

            //blues
            colors[48] = System.Drawing.ColorTranslator.FromHtml("#C6FFFF");
            colors[49] = System.Drawing.ColorTranslator.FromHtml("#84FFFF");
            colors[50] = System.Drawing.ColorTranslator.FromHtml("#00FFFF");
            colors[51] = System.Drawing.ColorTranslator.FromHtml("#6699FF");
            colors[52] = System.Drawing.ColorTranslator.FromHtml("#6666FF");
            colors[53] = System.Drawing.ColorTranslator.FromHtml("#3300FF");



            //reds
            colors[54] = System.Drawing.ColorTranslator.FromHtml("#FFCC99");
            colors[55] = System.Drawing.ColorTranslator.FromHtml("#FF9966");
            colors[56] = System.Drawing.ColorTranslator.FromHtml("#ff6633");
            colors[57] = System.Drawing.ColorTranslator.FromHtml("#FF0033");
            colors[58] = System.Drawing.ColorTranslator.FromHtml("#CC0000");
            colors[59] = System.Drawing.ColorTranslator.FromHtml("#AA0000");


            //pink / purple
            colors[60] = System.Drawing.ColorTranslator.FromHtml("#ffc7ff");
            colors[61] = System.Drawing.ColorTranslator.FromHtml("#ff86ff");
            colors[62] = System.Drawing.ColorTranslator.FromHtml("#ff00ff");
            colors[63] = System.Drawing.ColorTranslator.FromHtml("#FF00CC");
            colors[64] = System.Drawing.ColorTranslator.FromHtml("#CC0099");
            colors[65] = System.Drawing.ColorTranslator.FromHtml("#660099");


            //gray scale
            colors[66] = System.Drawing.ColorTranslator.FromHtml("#EEEEEE");
            colors[67] = System.Drawing.ColorTranslator.FromHtml("#CCCCCC");
            colors[68] = System.Drawing.ColorTranslator.FromHtml("#AAAAAA");
            colors[69] = System.Drawing.ColorTranslator.FromHtml("#888888");
            colors[70] = System.Drawing.ColorTranslator.FromHtml("#666666");
            colors[71] = System.Drawing.ColorTranslator.FromHtml("#444444");

        }
    }
}
