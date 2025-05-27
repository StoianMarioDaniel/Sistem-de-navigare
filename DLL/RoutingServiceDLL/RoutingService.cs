using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServiceDLL
{
    public class RouteInfo
    {
        public List<PointLatLng> Geometry { get; set; }
        public double Distance { get; set; }
        public double Duration { get; set; }
    }

    public class RoutingService
    {
        private static readonly HttpClient httpClient = new HttpClient();

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
                        return poly; // 🔒 protecție la a doua buclă

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
