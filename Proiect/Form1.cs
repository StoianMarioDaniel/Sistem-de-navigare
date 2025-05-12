using System;
using System.Windows.Forms; 
using GMap.NET;           
using GMap.NET.MapProviders; 
using GMap.NET.WindowsForms; 


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

    }
}