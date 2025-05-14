using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;




/*********************************************************************************************
 *                                                                                           * 
 *  File:        RouteCalculator.cs                                                          *
 *  Copyright:   (c) 2025 Stoian Mario-Daniel, Chiriac Raluca                                *
 *  E-mail:      mario-daniel.stoian@student.tuiasi.ro, raluca.chiriac@student.tuiasi.ro     *
 *                                                                                           *
 *********************************************************************************************/


namespace Proiect
{
    public partial class Form1 : Form
    {
        private GMapOverlay markersOverlay;
        private GMapOverlay routesOverlay;

        public Form1()
        {
            InitializeComponent();
        }
        //---------SMD-----------
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                gmapControl.MapProvider = GMapProviders.OpenStreetMap;
                gmapControl.MinZoom = 1;
                gmapControl.MaxZoom = 18;
                gmapControl.Zoom = 6;
                gmapControl.DragButton = MouseButtons.Left;
                gmapControl.Position = new PointLatLng(45.9432, 24.9668);
                gmapControl.ShowCenter = false;

                if (gmapControl.Overlays.Count == 0)
                {
                    markersOverlay = new GMapOverlay("markers");
                    gmapControl.Overlays.Add(markersOverlay);

                    routesOverlay = new GMapOverlay("routes");
                    gmapControl.Overlays.Add(routesOverlay);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A apărut o eroare la inițializarea hărții: {ex.Message}",
                                "Eroare GMap.NET",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        //---------SMD-----------/


        private async Task<PointLatLng?> GetCoordinatesAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SimulareNavigatieApp/1.0");

                try
                {
                    var response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                        return null;

                    var json = await response.Content.ReadAsStringAsync();
                    var results = JArray.Parse(json);

                    if (results.Count == 0)
                        return null;

                    if (results[0]["lat"] == null || results[0]["lon"] == null)
                        return null;

                    if (!double.TryParse((string)results[0]["lat"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat) ||
                        !double.TryParse((string)results[0]["lon"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lon))
                    {
                        Console.WriteLine($"Eroare la parsarea coordonatelor pentru adresa: {address}. Lat: {results[0]["lat"]}, Lon: {results[0]["lon"]}");
                        return null;
                    }

                    return new PointLatLng(lat, lon);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Excepție în GetCoordinatesAsync pentru '{address}': {ex.Message}");
                    return null;
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var coordPlecare = await GetCoordinatesAsync(textBox2.Text);
            var coordSosire = await GetCoordinatesAsync(textBox1.Text);

            if (coordPlecare == null)
            {
                MessageBox.Show("Locația de plecare nu a putut fi găsită sau nu a fost introdusă.", "Eroare Locație Plecare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //---------SMD-----------
            if (coordSosire == null)
            {
                MessageBox.Show("Locația de sosire nu a putut fi găsită sau nu a fost introdusă.", "Eroare Locație Sosire", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //---------SMD-----------/

            markersOverlay.Markers.Clear();
            routesOverlay.Routes.Clear();

            //---------SMD-----------
            textBox4.Text = "";
            textBox5.Text = "";
            //---------SMD-----------/

            markersOverlay.Markers.Add(new GMarkerGoogle(coordPlecare.Value, GMarkerGoogleType.green));
            markersOverlay.Markers.Add(new GMarkerGoogle(coordSosire.Value, GMarkerGoogleType.red));

            var routingService = new RoutingService();
            var routes = await routingService.GetRoutesAsync(coordPlecare.Value, coordSosire.Value);

            if (routes == null || routes.Count == 0)
            {
                MessageBox.Show("Nu a putut fi găsită nicio rută între locațiile specificate!", "Atenție!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //---------SMD-----------
            double averageSpeedKmH = 0;
            bool canCalculateMetrics = false;
            string originalSpeedInput = textBox3.Text;

            if (string.IsNullOrWhiteSpace(originalSpeedInput))
            {
                averageSpeedKmH = 60.0; 
                canCalculateMetrics = true;
            }
            else if (double.TryParse(originalSpeedInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedSpeed) && parsedSpeed > 0)
            {
                averageSpeedKmH = parsedSpeed; 
                canCalculateMetrics = true;
            }

            if (canCalculateMetrics) 
            {

                textBox3.Text = averageSpeedKmH.ToString("G", System.Globalization.CultureInfo.InvariantCulture);

                if (routes.Count > 0)
                {
                    const double fuelConsumptionRateL100Km = 7.5;
                    var firstRouteInfo = routes[0];

                    try
                    {
                        var metrics = RouteCalculator.CalculateRouteMetrics(firstRouteInfo, averageSpeedKmH, fuelConsumptionRateL100Km);

                        TimeSpan calculatedTime = metrics.CalculatedEstimatedTime;
                        string formattedCalculatedTime = $"{(int)calculatedTime.TotalHours}h {calculatedTime.Minutes:D2}m";
                        textBox4.Text = formattedCalculatedTime;

                        textBox5.Text = metrics.EstimatedConsumptionLiters.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " L";

                        TimeSpan apiTime = metrics.ApiProvidedDuration;
                        string formattedApiTime = $"{(int)apiTime.TotalHours}h {apiTime.Minutes:D2}m {apiTime.Seconds:D2}s";
                        Console.WriteLine($"--- Detalii Rută (Prima) ---");
                        Console.WriteLine($"Distanță: {metrics.DistanceKm.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} km");
                        Console.WriteLine($"Timp estimat (calculat cu {averageSpeedKmH.ToString("G", System.Globalization.CultureInfo.InvariantCulture)} km/h): {formattedCalculatedTime}");
                        Console.WriteLine($"Durată (din API OSRM): {formattedApiTime}");
                        Console.WriteLine($"Consum estimat ({fuelConsumptionRateL100Km} L/100km): {metrics.EstimatedConsumptionLiters.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} L");
                        Console.WriteLine($"-----------------------------");
                    }
                    catch (ArgumentOutOfRangeException exMetrics)
                    {
                        MessageBox.Show($"Eroare la calcularea metricilor: {exMetrics.Message}", "Eroare Calcul", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox4.Text = "";
                        textBox5.Text = "";
                    }
                    catch (Exception exGen)
                    {
                        MessageBox.Show($"O eroare neașteptată la calcularea metricilor: {exGen.Message}", "Eroare Calcul General", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox4.Text = "";
                        textBox5.Text = "";
                    }
                }
            }
            //---------SMD-----------/


            int colorStep = routes.Count > 1 ? 255 / (routes.Count - 1) : 255;
            for (int i = 0; i < routes.Count; i++)
            {
                var route = routes[i];
                var routeLine = new GMapRoute(route.Geometry, $"Ruta {i + 1} ({(route.Distance / 1000.0).ToString("F1")} km)")
                {
                    Stroke = routes.Count == 1 ? new Pen(Color.Blue, 3) :
                             new Pen(Color.FromArgb(255, Math.Min(255, i * colorStep), Math.Max(0, 100 - i * (colorStep / 2)), Math.Max(0, 255 - i * colorStep)), 3)
                };
                routesOverlay.Routes.Add(routeLine);
            }

            if (routes.Count > 0)
            {
                gmapControl.ZoomAndCenterRoutes("routes");
            }
            else
            {
                if (coordPlecare.HasValue) gmapControl.Position = coordPlecare.Value;
                gmapControl.Zoom = 6;
            }
        }
    }
}