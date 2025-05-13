using System;
using System.Windows.Forms; 
using GMap.NET;           
using GMap.NET.MapProviders; 
using GMap.NET.WindowsForms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;



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


        private void label3_Click(object sender, EventArgs e)
        {
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
            // Verificăm dacă ambele locații sunt selectate 
            var coordPlecare = await GetCoordinatesAsync(btn_plecare.Text);
            var coordSosire = await GetCoordinatesAsync(btn_sosire.Text);

            if (coordPlecare == null || coordSosire == null)
            {
                MessageBox.Show("Una dintre locații nu a putut fi găsită.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Coordonatele plecare: {coordPlecare.Value.Lat}, {coordPlecare.Value.Lng}\n" +
                            $"Coordonatele sosire: {coordSosire.Value.Lat}, {coordSosire.Value.Lng}");
            // --------------------------------------
        }
    }
}