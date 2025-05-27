/**************************************************************************
 *                                                                        *
 *  File:        ColorName.cs  (Notă: Numele fișierului ar trebui să      *
 *                              corespundă clasei pe care o conține)      *
 *  Copyright:   (c) 2025 Chilimon Ana-Maria                              *
 *  E-mail:      ana-maria.chilimon@student.tuiasi.ro                     *
 *                                                                        *
 *  Description: Această clasă statică, `ColorName`, furnizează          *
 *               funcționalitatea de a determina cel mai apropiat nume    *
 *               de culoare cunoscut (din enumerarea `KnownColor` a       *
 *               framework-ului .NET) pentru o culoare de intrare dată    *
 *               sub formă de valori RGB.                                 *
 *                                                                        *
 *               Principalul său scop este de a traduce o valoare de      *
 *               culoare numerică (RGB) într-un nume descriptiv, uman-    *
 *               lizibil, prin compararea culorii de intrare cu o listă   *
 *               predefinită de culori cunoscute și identificarea celei   *
 *               mai similare pe baza distanței euclidiene în spațiul     *
 *               de culori RGB.                                           *
 *                                                                        *
 **************************************************************************/


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 



namespace ColorNameDLL
{
    /// <summary>
    /// Clasă statică ce oferă metode pentru a obține numele celei mai apropiate culori cunoscute
    /// pentru o valoare de culoare RGB dată.
    /// </summary>
    public static class ColorName
    {
        /// <summary>
        /// Găsește și returnează numele celei mai apropiate culori cunoscute (din `System.Drawing.KnownColor`)
        /// pentru o culoare de intrare specificată.
        /// Compararea se bazează pe calculul distanței euclidiene între componentele RGB
        /// ale culorii de intrare și ale fiecărei culori cunoscute.
        /// </summary>
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