using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/**************************************************************************
 *                                                                        *
 *  File:        RouteCalculator.cs                                       *
 *  Copyright:   (c) 2025 Chilimon Ana-Maria                              *
 *  E-mail:      ana-maria.chilimon@student.tuiasi.ro                     *
 *                                                                        *
 *  Această clasă statică, `ColorName`, este folosita pentru a gasi cel   *
 * cel mai apropiat nume de culoare, pentru o culoare transmita ca RGB.   *
 *                                                                        *
 **************************************************************************/


namespace Proiect
{
    public static class ColorName
    {
        public static string GetClosestKnownColorName(Color inputColor)
        {
            string closestName = "";
            double minDistance = double.MaxValue;

            foreach (KnownColor known in Enum.GetValues(typeof(KnownColor)))
            {
                Color knownColor = Color.FromKnownColor(known);
                int rDiff = inputColor.R - knownColor.R;
                int gDiff = inputColor.G - knownColor.G;
                int bDiff = inputColor.B - knownColor.B;
                double distance = Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestName = knownColor.Name;
                }
            }

            return closestName;
        }

    }
}
