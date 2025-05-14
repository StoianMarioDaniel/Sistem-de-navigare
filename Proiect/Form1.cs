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

        //functia care preia coordonatele de la adresa 
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

                    double lat = double.Parse((string)results[0]["lat"]);
                    double lon = double.Parse((string)results[0]["lon"]);

                    return new PointLatLng(lat, lon);
                }
                catch
                {
                    return null;
                }
            }
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            var coordPlecare = await GetCoordinatesAsync(textBox1.Text);
            var coordSosire = await GetCoordinatesAsync(textBox2.Text);

            if (coordPlecare == null || coordSosire == null)
            {
                MessageBox.Show("Una dintre locații nu a putut fi găsită.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            markersOverlay.Markers.Clear();
            routesOverlay.Routes.Clear();

            markersOverlay.Markers.Add(new GMarkerGoogle(coordPlecare.Value, GMarkerGoogleType.green));
            markersOverlay.Markers.Add(new GMarkerGoogle(coordSosire.Value, GMarkerGoogleType.red));

            var routingService = new RoutingService();
            var routes = await routingService.GetRoutesAsync(coordPlecare.Value, coordSosire.Value);

            if (routes.Count == 0)
            {
                MessageBox.Show("Nu a putut fi găsită nicio rută!", "Atenție!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int colorStep = 255 / routes.Count;
            for (int i = 0; i < routes.Count; i++)
            {
                var route = routes[i];
                var routeLine = new GMapRoute(route.Geometry, $"Ruta {i + 1}")
                {
                    Stroke = new Pen(Color.FromArgb(255, i * colorStep, 0, 255 - i * colorStep), 3)
                };
                routesOverlay.Routes.Add(routeLine);
            }

            gmapControl.ZoomAndCenterRoutes("routes");
        }
    }
}