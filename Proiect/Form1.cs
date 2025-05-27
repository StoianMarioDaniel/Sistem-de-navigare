using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoutingServiceDLL;
using ColorNameDLL;
using RouteCalculatorDLL;



/*********************************************************************************************
 *                                                                                           * 
 *  File:        RouteCalculator.cs                                                          *
 *  Copyright:   (c) 2025 Stoian Mario-Daniel, Chiriac Raluca-Ștefania, Chilimon Ana-Maria   *
 *  E-mail:      mario-daniel.stoian@student.tuiasi.ro,                                      *
 *               raluca-stefania.chiriac@student.tuiasi.ro,                                  *
 *               ana-maria.chilimon@student.tuiasi.ro                                        *
 *                                                                                           *
 *********************************************************************************************/

namespace Proiect
{
    public partial class Form1 : Form
    {
        private GMapOverlay _markersOverlay;
        private GMapOverlay _routesOverlay;

        private IApplicationState _currentState;

        public GMapControl GMapControl => gmapControl;
        public ListBox RouteListBox => listBox1;
        public TextBox StartLocationTextBox => textBox2;
        public TextBox EndLocationTextBox => textBox1;
        public TextBox SpeedTextBox => textBox3;
        public TextBox EstimatedTimeTextBox => textBox4;
        public TextBox EstimatedConsumptionTextBox => textBox5;
        public Button CalculateButton => button1;

        public GMapOverlay MarkersOverlay => _markersOverlay;
        public GMapOverlay RoutesOverlay => _routesOverlay;
        public List<RouteInfo> CurrentRoutes { get; set; }

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
                    _markersOverlay = new GMapOverlay("markers");
                    gmapControl.Overlays.Add(_markersOverlay);

                    _routesOverlay = new GMapOverlay("routes");
                    gmapControl.Overlays.Add(_routesOverlay);
                }
            }
            catch (Exception ex)
            {
                ShowUserMessage($"A apărut o eroare la inițializarea hărții: {ex.Message}",
                                "Eroare GMap.NET",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            TransitionToState(new InitialState(this));
        }
        //---------SMD-----------/

        public void TransitionToState(IApplicationState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public async Task<PointLatLng?> GetCoordinatesForAddressAsync(string address) 
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
                    if (!response.IsSuccessStatusCode) return null;
                    var json = await response.Content.ReadAsStringAsync();
                    var results = JArray.Parse(json);
                    if (results.Count == 0) return null;
                    if (results[0]["lat"] == null || results[0]["lon"] == null) return null;
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
                    Console.WriteLine($"Excepție în GetCoordinatesForAddressAsync pentru '{address}': {ex.Message}");
                    return null;
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_currentState != null)
            {
                await _currentState.HandleCalculateRouteClickedAsync();
            }
        }

        public async Task TriggerRouteCalculationAsync()
        {
            if (_currentState != null)
            {
                await _currentState.HandleCalculateRouteClickedAsync();
            }
        }

        //--------------CAM----------------
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentState != null)
            {
                _currentState.HandleRouteSelectionChanged(listBox1.SelectedIndex);
            }
        }
        //--------------CAM----------------/

        public void SetLoadingState(bool isLoading)
        {
            CalculateButton.Enabled = !isLoading;
            RouteListBox.Enabled = !isLoading;
            this.Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }

        public void ResetUIForNewCalculation()
        {
            //--------------CAM----------------
            RouteListBox.Items.Clear(); 

            //--------------CAM----------------/

            //---------SMD-----------
            EstimatedTimeTextBox.Text = "";
            EstimatedConsumptionTextBox.Text = "";
            //---------SMD-----------/

            _markersOverlay?.Markers.Clear();
            _routesOverlay?.Routes.Clear();
            CurrentRoutes = null; 
            gmapControl.Refresh();
        }

        public void ClearMetricTextBoxes()
        {
            //---------SMD-----------
            EstimatedTimeTextBox.Text = "";
            EstimatedConsumptionTextBox.Text = "";
            //---------SMD-----------/
        }

        public void DisplayMarkers(PointLatLng start, PointLatLng end)
        {
            _markersOverlay.Markers.Clear();
            _markersOverlay.Markers.Add(new GMarkerGoogle(start, GMarkerGoogleType.green));
            _markersOverlay.Markers.Add(new GMarkerGoogle(end, GMarkerGoogleType.red));
            gmapControl.Refresh();
        }

        public void PopulateRouteListBox()
        {
            RouteListBox.Items.Clear(); 
            if (CurrentRoutes == null || RoutesOverlay.Routes.Count != CurrentRoutes.Count)
            {
                if (CurrentRoutes == null || CurrentRoutes.Count == 0) return;

                for (int i = 0; i < CurrentRoutes.Count; i++)
                {
                    var routeInfo = CurrentRoutes[i];
                    //--------------CAM----------------
                    string routeName = $"Ruta {i + 1} ({(routeInfo.Distance / 1000.0):F1} km)";
                    RouteListBox.Items.Add(routeName);
                    //--------------CAM----------------/
                }
                return;
            }


            for (int i = 0; i < CurrentRoutes.Count; i++)
            {
                var routeInfo = CurrentRoutes[i];
                var routeLine = RoutesOverlay.Routes[i];

                //--------------CAM----------------
                string colorName = ColorName.GetClosestKnownColorName(routeLine.Stroke.Color);
                string routeName = $"Ruta {i + 1} ({colorName}, {(routeInfo.Distance / 1000.0):F1} km)";
                RouteListBox.Items.Add(routeName);
                //--------------CAM----------------/
            }
        }

        public void DisplayAllRoutesOnMap()
        {
            _routesOverlay.Routes.Clear();
            if (CurrentRoutes == null) return;

            int colorStep = CurrentRoutes.Count > 1 ? 255 / (CurrentRoutes.Count - 1) : 255;
            for (int i = 0; i < CurrentRoutes.Count; i++)
            {
                var route = CurrentRoutes[i];
                var routeLine = new GMapRoute(route.Geometry, $"Ruta {i + 1} ({(route.Distance / 1000.0):F1} km)")
                {
                    Stroke = CurrentRoutes.Count == 1 ? new Pen(Color.Blue, 3) :
                             new Pen(Color.FromArgb(255, Math.Min(255, i * colorStep), Math.Max(0, 100 - i * (colorStep / 2)), Math.Max(0, 255 - i * colorStep)), 3)
                };
                _routesOverlay.Routes.Add(routeLine);
            }
            gmapControl.Refresh();
        }

        //--------------CAM----------------
        public void HighlightSelectedRouteAndUpdateListBox(int selectedIndex)
        {
            if (_routesOverlay == null || _routesOverlay.Routes == null || CurrentRoutes == null || selectedIndex < 0 || selectedIndex >= CurrentRoutes.Count)
            {
                ResetRouteHighlightingAndListBox(); 
                return;
            }

            RouteListBox.SelectedIndexChanged -= listBox1_SelectedIndexChanged; 
            RouteListBox.Items.Clear();

            for (int i = 0; i < _routesOverlay.Routes.Count; i++)
            {
                if (i >= CurrentRoutes.Count) break;

                GMapRoute routeLine = _routesOverlay.Routes[i];
                string colorDisplayName;

                if (i == selectedIndex)
                {
                    routeLine.Stroke = new Pen(Color.Red, 3);
                    colorDisplayName = "Red";
                }
                else
                {
                    int colorStep = CurrentRoutes.Count > 1 ? 255 / (CurrentRoutes.Count - 1) : 255;
                    routeLine.Stroke = CurrentRoutes.Count == 1 ? new Pen(Color.Blue, 1) :
                                     new Pen(Color.FromArgb(255, Math.Min(255, i * colorStep), Math.Max(0, 100 - i * (colorStep / 2)), Math.Max(0, 255 - i * colorStep)), 1);
                    colorDisplayName = ColorName.GetClosestKnownColorName(routeLine.Stroke.Color); 
                }
                string routeItemText = $"Ruta {i + 1} ({colorDisplayName}, {(CurrentRoutes[i].Distance / 1000.0):F1} km)";
                RouteListBox.Items.Add(routeItemText);
            }

            if (selectedIndex >= 0 && selectedIndex < RouteListBox.Items.Count)
            {
                RouteListBox.SelectedIndex = selectedIndex; 
            }
            RouteListBox.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            gmapControl.Refresh();
        }

        public void ResetRouteHighlightingAndListBox()
        {
            if (_routesOverlay == null || CurrentRoutes == null) return;

            RouteListBox.SelectedIndexChanged -= listBox1_SelectedIndexChanged;
            RouteListBox.Items.Clear();

            int colorStep = CurrentRoutes.Count > 1 ? 255 / (CurrentRoutes.Count - 1) : 255;
            for (int i = 0; i < _routesOverlay.Routes.Count; i++)
            {
                if (i >= CurrentRoutes.Count) break;
                GMapRoute routeLine = _routesOverlay.Routes[i];
                routeLine.Stroke = CurrentRoutes.Count == 1 ? new Pen(Color.Blue, 3) :
                                  new Pen(Color.FromArgb(255, Math.Min(255, i * colorStep), Math.Max(0, 100 - i * (colorStep / 2)), Math.Max(0, 255 - i * colorStep)), 3);

                string colorName = ColorName.GetClosestKnownColorName(routeLine.Stroke.Color);
                string routeNameText = $"Ruta {i + 1} ({colorName}, {(CurrentRoutes[i].Distance / 1000.0):F1} km)";
                RouteListBox.Items.Add(routeNameText);
            }
            RouteListBox.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            gmapControl.Refresh();
        }
        //--------------CAM----------------/


        //---------SMD-----------

        public void UpdateRouteMetrics(RouteInfo routeInfo)
        {
            double averageSpeedKmH = 60.0;
            bool canCalculateMetrics = false;
            string originalSpeedInput = SpeedTextBox.Text;

            if (string.IsNullOrWhiteSpace(originalSpeedInput))
            {
                canCalculateMetrics = true;
            }
            else if (double.TryParse(originalSpeedInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedSpeed) && parsedSpeed > 0)
            {
                averageSpeedKmH = parsedSpeed;
                canCalculateMetrics = true;
            }

            SpeedTextBox.Text = averageSpeedKmH.ToString("G", System.Globalization.CultureInfo.InvariantCulture);

            if (canCalculateMetrics && routeInfo != null)
            {
                const double fuelConsumptionRateL100Km = 7.5;
                try
                {
                    var metrics = RouteCalculator.CalculateRouteMetrics(routeInfo, averageSpeedKmH, fuelConsumptionRateL100Km);
                    TimeSpan calculatedTime = metrics.CalculatedEstimatedTime;
                    string formattedCalculatedTime = $"{(int)calculatedTime.TotalHours}h {calculatedTime.Minutes:D2}m";
                    EstimatedTimeTextBox.Text = formattedCalculatedTime;
                    EstimatedConsumptionTextBox.Text = metrics.EstimatedConsumptionLiters.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " L";

                    TimeSpan apiTime = metrics.ApiProvidedDuration;
                    string formattedApiTime = $"{(int)apiTime.TotalHours}h {apiTime.Minutes:D2}m {apiTime.Seconds:D2}s";
                    Console.WriteLine($"--- Detalii Rută (Info curent) ---");
                    Console.WriteLine($"Distanță: {metrics.DistanceKm:F2} km");
                    Console.WriteLine($"Timp estimat (calculat cu {averageSpeedKmH:G} km/h): {formattedCalculatedTime}");
                    Console.WriteLine($"Durată (din API OSRM): {formattedApiTime}");
                    Console.WriteLine($"Consum estimat ({fuelConsumptionRateL100Km} L/100km): {metrics.EstimatedConsumptionLiters:F2} L");
                    Console.WriteLine($"-----------------------------");
                }
                catch (ArgumentOutOfRangeException exMetrics)
                {
                    ShowUserMessage($"Eroare la calcularea metricilor: {exMetrics.Message}", "Eroare Calcul", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearMetricTextBoxes(); 
                }
                catch (Exception exGen)
                {
                    ShowUserMessage($"O eroare neașteptată la calcularea metricilor: {exGen.Message}", "Eroare Calcul General", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearMetricTextBoxes(); 
                }
            }
            else
            {
                if (!canCalculateMetrics && !string.IsNullOrWhiteSpace(originalSpeedInput))
                {
                    ShowUserMessage("Viteza introdusă nu este validă. Nu s-au putut calcula metricile.", "Eroare Viteză", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                ClearMetricTextBoxes(); 
            }
        }
        //---------SMD-----------/

        public void ShowUserMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(this, message, caption, buttons, icon);
        }
    }
}