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
 *  Această clasă statică, `RouteCalculator`, oferă funcționalități       *
 *  pentru calcularea diverselor metrici asociate unei rute de            *
 *  navigație. Principala sa metodă, `CalculateRouteMetrics`, primește    *
 *  informații despre rută (distanță și durată furnizate de un API),      *
 *  o viteză medie de deplasare și o rată de consum de combustibil.       *
 *  Pe baza acestor date, metoda calculează și returnează:                *
 *    - Distanța totală a rutei în kilometri.                             *
 *    - Timpul estimat de parcurgere, calculat pe baza distanței și       *
 *      a vitezei medii.                                                  *
 *    - Durata călătoriei furnizată de API (convertită în TimeSpan).      *
 *    - Consumul estimat de combustibil pentru întreaga rută.             *
 *  Rezultatele sunt încapsulate într-un obiect de tip `CalculationResult`*
 *  Clasa include, de asemenea, validări de bază pentru viteza medie și   *
 *  rata de consum introduse.                                             *
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
