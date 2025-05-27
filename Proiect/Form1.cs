/*********************************************************************************************
 *                                                                                           * 
 *  File:        Form1.cs (anterior denumit RouteCalculator.cs în comentariu)                *
 *  Copyright:   (c) 2025 Stoian Mario-Daniel, Chiriac Raluca-Ștefania,                      *
 *                      Chilimon Ana-Maria, Galusca Mihnea-Ioan                              *
 *  E-mail:      mario-daniel.stoian@student.tuiasi.ro,                                      *
 *               raluca-stefania.chiriac@student.tuiasi.ro,                                  *
 *               ana-maria.chilimon@student.tuiasi.ro,                                       *
 *               mihnea-ioan.galusca@student.tuiasi.ro                                       *
 *  Description: Acest fișier conține clasa principală `Form1` a aplicației de               *
 *               simulare a navigației. Aceasta gestionează interfața grafică (UI)           *
 *               și logica de bază a aplicației, incluzând:                                  *
 *               - Inițializarea și configurarea hărții (GMap.NET) cu OpenStreetMap.         *
 *               - Gestionarea input-ului utilizatorului pentru locațiile de start/destinație*
 *                 și viteza medie.                                                          *
 *               - Obținerea coordonatelor geografice pentru adresele introduse folosind     *
 *                 serviciul Nominatim.                                                      *
 *               - Interacțiunea cu biblioteci externe (DLL-uri) pentru calculul rutelor     *
 *                 (`RoutingServiceDLL`), determinarea numelui culorilor (`ColorNameDLL`) și *
 *                 calculul metricilor rutei (`RouteCalculatorDLL`).                         *
 *               - Afișarea markerelor de start/destinație și a rutelor multiple pe hartă,   *
 *                 cu diferențiere vizuală (culori).                                         *
 *               - Popularea unei liste cu detaliile rutelor găsite (distanță, nume culoare).*
 *               - Evidențierea rutei selectate din listă pe hartă și actualizarea UI-ului.  *
 *               - Calcularea și afișarea timpului estimat de călătorie și a consumului      *
 *                 estimat de combustibil, pe baza vitezei medii și a unui consum standard.  *
 *               - Implementarea unui sistem de stări (`IApplicationState`) pentru a         *
 *                 gestiona diferitele faze ale aplicației (ex. inițial, calculare rută).    *
 *               - Funcționalități de resetare a UI-ului și de afișare a mesajelor către     *
 *                 utilizator.                                                               *
 *               - Furnizarea accesului la fișierul de ajutor (help) al aplicației.          *
 *               Integrează funcționalități dezvoltate de mai mulți membri ai echipei        *
 *               (SMD, GMI, CAM).                                                            *
 *                                                                                           *
 *********************************************************************************************/



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

        /// <summary>
        /// Constructorul clasei Form1.
        /// Inițializează componentele vizuale ale formularului.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        //---------SMD-----------
        /// <summary>
        /// Handler pentru evenimentul de încărcare a formularului.
        /// Inițializează controlul GMapControl (hartă), setează provider-ul, zoom-ul, poziția inițială.
        /// Creează și adaugă overlay-urile pentru markere și rute.
        /// Tranziționează aplicația la starea inițială.
        /// </summary>
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

        /// <summary>
        /// Realizează tranziția aplicației către o nouă stare.
        /// Apelează metoda Exit() a stării curente (dacă există) și metoda Enter() a noii stări.
        /// </summary>
        /// <param name="newState">Noua stare în care va tranziționa aplicația.</param>
        public void TransitionToState(IApplicationState newState)
        {
            _currentState?.Exit(); 
            _currentState = newState; 
            _currentState.Enter(); 
        }

        //---------GMI-----------/
        /// <summary>
        /// Obține asincron coordonatele geografice (latitudine, longitudine) pentru o adresă dată.
        /// Utilizează serviciul Nominatim (OpenStreetMap) pentru geocodare.
        /// </summary>
        /// <param name="address">Adresa (text) pentru care se caută coordonatele.</param>
        /// <returns>Un obiect PointLatLng conținând coordonatele sau null dacă adresa nu este găsită sau apare o eroare.</returns>
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

        /// <summary>
        /// Handler pentru evenimentul de click pe butonul de calculare a rutei (button1).
        /// Deleagă acțiunea stării curente a aplicației.
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            if (_currentState != null)
            {
                await _currentState.HandleCalculateRouteClickedAsync();
            }
        }
        //---------GMI-----------/

        /// <summary>
        /// Metodă publică pentru a declanșa manual calcularea rutei.
        /// Poate fi apelată din alte părți ale aplicației dacă este necesar.
        /// Deleagă acțiunea stării curente a aplicației.
        /// </summary>
        public async Task TriggerRouteCalculationAsync()
        {
            if (_currentState != null)
            {
                await _currentState.HandleCalculateRouteClickedAsync();
            }
        }

        //--------------CAM----------------
        /// <summary>
        /// Handler pentru evenimentul de schimbare a selecției în lista de rute (listBox1).
        /// Deleagă acțiunea stării curente a aplicației, transmițând indexul elementului selectat.
        /// </summary>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentState != null)
            {
                _currentState.HandleRouteSelectionChanged(listBox1.SelectedIndex);
            }
        }
        //--------------CAM----------------/

        /// <summary>
        /// Setează starea vizuală de încărcare a aplicației.
        /// Activează/dezactivează butonul de calcul și lista de rute.
        /// Modifică cursorul mouse-ului pentru a indica așteptarea.
        /// </summary>
        /// <param name="isLoading">True dacă aplicația încarcă date, false altfel.</param>
        public void SetLoadingState(bool isLoading)
        {
            CalculateButton.Enabled = !isLoading;
            RouteListBox.Enabled = !isLoading;
            this.Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }

        /// <summary>
        /// Resetează interfața grafică pentru o nouă calculare de rută.
        /// Golește lista de rute, câmpurile de text pentru metrici (timp, consum),
        /// marker-ele și rutele de pe hartă. Resetează lista `CurrentRoutes`.
        /// </summary>
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

        /// <summary>
        /// Golește câmpurile de text care afișează metricile rutei (timpul estimat și consumul estimat).
        /// </summary>
        public void ClearMetricTextBoxes()
        {
            //---------SMD-----------
            EstimatedTimeTextBox.Text = "";
            EstimatedConsumptionTextBox.Text = "";
            //---------SMD-----------/
        }

        //---------GMI-----------/
        /// <summary>
        /// Afișează marker-ele de start (verde) și destinație (roșu) pe hartă la coordonatele specificate.
        /// </summary>
        /// <param name="start">Coordonatele punctului de start.</param>
        /// <param name="end">Coordonatele punctului de destinație.</param>
        public void DisplayMarkers(PointLatLng start, PointLatLng end)
        {
            _markersOverlay.Markers.Clear(); 
            _markersOverlay.Markers.Add(new GMarkerGoogle(start, GMarkerGoogleType.green)); 
            _markersOverlay.Markers.Add(new GMarkerGoogle(end, GMarkerGoogleType.red));     
            gmapControl.Refresh();
        }
        //---------GMI-----------/

        /// <summary>
        /// Populează lista de rute (ListBox) cu informații despre rutele găsite.
        /// Afișează numărul rutei, numele culorii asociate (dacă este disponibilă) și distanța.
        /// </summary>
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

        /// <summary>
        /// Desenează toate rutele din `CurrentRoutes` pe hartă.
        /// Fiecărei rute i se atribuie o culoare distinctă pentru diferențiere vizuală.
        /// Dacă există o singură rută, aceasta este desenată cu albastru.
        /// </summary>
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
                             new Pen(Color.FromArgb(255,
                                                    Math.Min(255, i * colorStep), 
                                                    Math.Max(0, 100 - i * (colorStep / 2)), 
                                                    Math.Max(0, 255 - i * colorStep)),
                                   3) 
                };
                _routesOverlay.Routes.Add(routeLine); 
            }
            gmapControl.Refresh(); 
        }

        //--------------CAM----------------
        /// <summary>
        /// Evidențiază ruta selectată în ListBox pe hartă (o colorează cu roșu) și
        /// actualizează textul elementelor din ListBox pentru a reflecta culorile curente ale rutelor de pe hartă.
        /// Celelalte rute sunt desenate cu culorile lor standard (sau albastru subțire dacă e singura).
        /// </summary>
        /// <param name="selectedIndex">Indexul rutei selectate în ListBox.</param>
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
                                     new Pen(Color.FromArgb(255,
                                                            Math.Min(255, i * colorStep),
                                                            Math.Max(0, 100 - i * (colorStep / 2)),
                                                            Math.Max(0, 255 - i * colorStep)),
                                           1); 
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

        /// <summary>
        /// Resetează evidențierea rutelor pe hartă la culorile lor standard (definite în DisplayAllRoutesOnMap)
        /// și repopulează ListBox-ul cu descrierile corespunzătoare acestor culori standard.
        /// </summary>
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
                                  new Pen(Color.FromArgb(255,
                                                         Math.Min(255, i * colorStep),
                                                         Math.Max(0, 100 - i * (colorStep / 2)),
                                                         Math.Max(0, 255 - i * colorStep)),
                                        3);

                string colorName = ColorName.GetClosestKnownColorName(routeLine.Stroke.Color);
                string routeNameText = $"Ruta {i + 1} ({colorName}, {(CurrentRoutes[i].Distance / 1000.0):F1} km)";
                RouteListBox.Items.Add(routeNameText);
            }
            RouteListBox.SelectedIndexChanged += listBox1_SelectedIndexChanged; 
            gmapControl.Refresh(); 
        }
        //--------------CAM----------------/


        //---------SMD-----------
        /// <summary>
        /// Actualizează și afișează metricile pentru o rută specificată (timp estimat, consum estimat).
        /// Se bazează pe viteza medie introdusă de utilizator (sau o valoare implicită) și un consum standard.
        /// Utilizează `RouteCalculatorDLL` pentru calcule.
        /// </summary>
        /// <param name="routeInfo">Informațiile despre ruta pentru care se calculează metricile.</param>
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

        /// <summary>
        /// Afișează un mesaj utilizatorului folosind un MessageBox standard.
        /// </summary>
        /// <param name="message">Mesajul de afișat.</param>
        /// <param name="caption">Titlul ferestrei de mesaj.</param>
        /// <param name="buttons">Butoanele de afișat în fereastra de mesaj.</param>
        /// <param name="icon">Iconița de afișat în fereastra de mesaj.</param>
        public void ShowUserMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(this, message, caption, buttons, icon);
        }

        //---------GMI-----------/
        /// <summary>
        /// Handler pentru evenimentul de click pe opțiunea de meniu "Help".
        /// Încearcă să deschidă fișierul de ajutor (.chm) al aplicației.
        /// Afișează un mesaj de eroare dacă fișierul nu este găsit sau apare o altă problemă.
        /// </summary>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string helpFilePath = System.IO.Path.Combine(Application.StartupPath, "Sistem-Simulare-de-Navigare_Help.chm");

                if (System.IO.File.Exists(helpFilePath))
                {
                    Help.ShowHelp(this, helpFilePath); 
                }
                else
                {
                    MessageBox.Show("Fișierul de ajutor nu a fost găsit la locația:\n" + helpFilePath,
                                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ShowUserMessage($"A apărut o eroare la deschiderea fișierului de ajutor: {ex.Message}",
                                "Eroare Help",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        //---------GMI-----------/
    }
}