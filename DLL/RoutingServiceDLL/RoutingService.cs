/**************************************************************************
 *                                                                        *
 *  File:        RoutingService.cs                                        *
 *  Copyright:   (c) 2025 Mihnea-Ioan Galusca                             *
 *  E-mail:      mihnea-ioan.galusca@student.tuiasi.ro                    *
 *  Description: Acest fișier definește clasa `RoutingService` și clasa   *
 *               de date `RouteInfo`, responsabile pentru interacțiunea   *
 *               cu serviciul de rutare Open Source Routing Machine (OSRM)*
 *               și procesarea datelor de rută.                           *
 *               - `RouteInfo`: O clasă simplă de tip container (POCO)    *
 *                 pentru a stoca informațiile despre o rută individuală, *
 *                 incluzând geometria (o listă de puncte `PointLatLng`), *
 *                 distanța totală (în metri) și durata estimată          *
 *                 (în secunde).                                          *
 *               - `RoutingService`: Clasa principală care gestionează    *
 *                 comunicarea cu API-ul OSRM.                            *
 *                 - `GetRoutesAsync`: Metodă asincronă care construiește *
 *                   și trimite o cerere HTTP GET către serverul public   *
 *                   OSRM pentru a obține rute (inclusiv alternative)     *
 *                   între două coordonate geografice (start și end).     *
 *                   Parsează răspunsul JSON primit de la OSRM, extrage   *
 *                   informațiile relevante pentru fiecare rută (distanță,*
 *                   durată, geometrie codificată) și le transformă într-o*
 *                   listă de obiecte `RouteInfo`.                        *
 *                   Aruncă excepții în caz de erori de comunicare cu     *
 *                   OSRM sau dacă răspunsul API-ului nu este valid.      *
 *                 - `DecodePolyline`: Metodă privată utilizată pentru a  *
 *                   decoda șirul de caractere al geometriei rutei        *
 *                   (format polyline encodat Google) într-o listă de     *
 *                   coordonate `PointLatLng`, care pot fi apoi folosite  *
 *                   pentru a desena ruta pe hartă.                       *
 *               Clasa utilizează `HttpClient` pentru cererile HTTP și    *
 *               `System.Text.Json` pentru parsarea răspunsurilor JSON.   *
 *                                                                        *
 **************************************************************************/

using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServiceDLL
{
    /// <summary>
    /// Reprezintă datele specifice despre o ruta precum geometria, distanta si durata.
    /// </summary>
    public class RouteInfo
    {
        public List<PointLatLng> Geometry { get; set; }
        public double Distance { get; set; }
        public double Duration { get; set; }
    }

    /// <summary>
    /// Aceasta este clasa ce se ocupa cu apelarea serviciului OSRM pentru a obtine ruta intre 2 locatii
    /// </summary>
    public class RoutingService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        /// <summary>
        /// Trimite o cerere către OSRM pentru a obține una sau mai multe rute între două puncte.
        /// </summary>
        public async Task<List<RouteInfo>> GetRoutesAsync(PointLatLng start, PointLatLng end)
        {
            var url = $"http://router.project-osrm.org/route/v1/driving/{start.Lng},{start.Lat};{end.Lng},{end.Lat}?overview=full&alternatives=true&geometries=polyline";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Eroare la apelarea OSRM.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.GetProperty("code").GetString() != "Ok")
            {
                throw new Exception("Raspuns invalid de la OSRM.");
            }

            var routes = new List<RouteInfo>();
            foreach (var routeElement in root.GetProperty("routes").EnumerateArray())
            {
                var distance = routeElement.GetProperty("distance").GetDouble();
                var duration = routeElement.GetProperty("duration").GetDouble();
                var geometry = DecodePolyline(routeElement.GetProperty("geometry").GetString());

                routes.Add(new RouteInfo
                {
                    Distance = distance,
                    Duration = duration,
                    Geometry = geometry
                });
            }
            return routes;
        }

        /// <summary>
        /// Decodează un string de tip polyline (Google Encoded Polyline Algorithm Format) într-o listă de coordonate.
        /// </summary>
        private List<PointLatLng> DecodePolyline(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return new List<PointLatLng>();

            var poly = new List<PointLatLng>();
            int index = 0, len = encoded.Length;
            int lat = 0, lng = 0;

            while (index < len)
            {
                int b, shift = 0, result = 0;
                do
                {
                    if (index >= len)
                        return poly;

                    b = encoded[index++] - 63;
                    result |= (b & 0x1F) << shift;
                    shift += 5;
                }
                while (b >= 0x20);
                int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lat += dlat;

                shift = 0;
                result = 0;
                do
                {
                    if (index >= len)
                        return poly; 

                    b = encoded[index++] - 63;
                    result |= (b & 0x1F) << shift;
                    shift += 5;
                }
                while (b >= 0x20);
                int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lng += dlng;

                var point = new PointLatLng(
                    lat / 1E5,
                    lng / 1E5
                );
                poly.Add(point);
            }

            return poly;
        }
    }
}
