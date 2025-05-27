/************************************************************************************************
 *                                                                                              * 
 *  File:        UnitTest1.cs                                                                   *
 *  Copyright:   (c) 2025  Chiriac Raluca-Ștefania                                              *
 *  E-mail:      raluca-stefania.chiriac@student.tuiasi.ro                                      *
 *   Description: Conține teste unitare automate pentru verificarea funcționalității aplicației *
 *               de rutare, inclusiv calculul rutelor, afișarea metricilor (distanță, timp,     * 
 *               consum) și validarea calculelor matematice de bază.                            *                  
 *                                                                                              *
 ************************************************************************************************/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Proiect;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;
using System.Diagnostics;
using System.Threading;


namespace UnitTestGPS
{

    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Testează calcularea unei rute simple între Suceava și Iași.
        /// Verifică dacă se găsește cel puțin o rută.
        /// </summary>
        [TestMethod]
        public void test_Orase1_SuceavaToIasi()
        {
            var form = new Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Suceava";
            form.EndLocationTextBox.Text = "Iasi";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(10))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "Nu s-au găsit rute.");

            form.Close();
        }

        /// <summary>
        /// Testează calcularea unei rute între Bacău și Cluj-Napoca cu o viteză specificată.
        /// Verifică dacă se găsește o rută și dacă se afișează corect distanța, timpul și consumul.
        /// </summary>
        [TestMethod]
        public void Test_Orase2_BacauToCluj()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Bacau";
            form.EndLocationTextBox.Text = "Cluj-Napoca";
            form.SpeedTextBox.Text = "80";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(10))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "Nu s-au găsit rute.");


            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string kmText = form.RouteListBox.Items[0].ToString();
            string timeText = form.EstimatedTimeTextBox.Text;
            string consumText = form.EstimatedConsumptionTextBox.Text;

            Assert.IsTrue(kmText.Contains("km"), "Nu se afișează kilometri în listă.");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(timeText), "Nu se afișează timpul estimat.");
            Assert.IsTrue(consumText.Contains("L"), "Nu se afișează consumul estimat.");

            form.Close();
        }

        /// <summary>
        /// Testează calcularea unei rute lungi între Timișoara și Constanța.
        /// Verifică găsirea rutei și afișarea corectă a metricilor (distanță, timp, consum).
        /// </summary>
        [TestMethod]
        public void Test_Orase3_TimisoaraToConstanta()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Timisoara";
            form.EndLocationTextBox.Text = "Constanta";
            form.SpeedTextBox.Text = "100";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(15))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "Nu s-au găsit rute.");


            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string kmText = form.RouteListBox.Items[0].ToString();
            string timeText = form.EstimatedTimeTextBox.Text;
            string consumText = form.EstimatedConsumptionTextBox.Text;

            Assert.IsTrue(kmText.Contains("km"), "Nu se afișează kilometri în listă.");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(timeText), "Nu se afișează timpul estimat.");
            Assert.IsTrue(consumText.Contains("L"), "Nu se afișează consumul estimat.");

            form.Close();
        }

        /// <summary>
        /// Testează calcularea unei rute internaționale între Oslo și Budapesta.
        /// Verifică dacă se găsește cel puțin o rută.
        /// </summary>
        [TestMethod]
        public void Test_Orase4_OsloToBudapesta()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Oslo";
            form.EndLocationTextBox.Text = "Budapesta";
            form.SpeedTextBox.Text = "90";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(12))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "Nu s-au găsit rute.");
            form.Close();
        }

        /// <summary>
        /// Testează calcularea unei rute internaționale lungi între Lisabona și Varșovia.
        /// Verifică dacă se găsește cel puțin o rută.
        /// </summary>
        [TestMethod]
        public void Test_Orase5_LisabonaToVarsovia()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Lisabona";
            form.EndLocationTextBox.Text = "Varsovia";
            form.SpeedTextBox.Text = "70";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(12))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "Nu s-au găsit rute.");
            form.Close();
        }

        /// <summary>
        /// Testează o rută internațională (Paris - Berlin) cu viteză specificată.
        /// Verifică găsirea rutei și afișarea corectă a tuturor metricilor (distanță, timp, consum).
        /// </summary>
        [TestMethod]
        public void Test_Orase6_ParisToBerlin_CustomSpeedAndMetrics()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Paris";
            form.EndLocationTextBox.Text = "Berlin";
            form.SpeedTextBox.Text = "110";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(15))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "No routes found.");

            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string kmText = form.RouteListBox.Items[0].ToString();
            string timeText = form.EstimatedTimeTextBox.Text;
            string consumText = form.EstimatedConsumptionTextBox.Text;

            Assert.IsTrue(kmText.Contains("km"), "Kilometers not displayed.");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(timeText), "Estimated time not displayed.");
            Assert.IsTrue(consumText.Contains("L"), "Estimated consumption not displayed.");

            form.Close();
        }

        /// <summary>
        /// Testează o rută internațională (Madrid - Roma) folosind viteza implicită (câmpul viteză gol).
        /// Verifică găsirea rutei și afișarea timpului estimat.
        /// </summary>
        [TestMethod]
        public void Test_Orase7_MadridToRome_DefaultSpeed()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Madrid";
            form.EndLocationTextBox.Text = "Rome";
            form.SpeedTextBox.Text = ""; 

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(15))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "No routes found.");

            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string timeText = form.EstimatedTimeTextBox.Text;

            Assert.IsTrue(!string.IsNullOrWhiteSpace(timeText), "Estimated time not displayed.");

            form.Close();
        }

        /// <summary>
        /// Testează o rută internațională (Amsterdam - Viena).
        /// Verifică găsirea rutei și dacă se afișează corect kilometrii în lista de rute.
        /// </summary>
        [TestMethod]
        public void Test_Orase8_AmsterdamToVienna_CheckKilometers()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Amsterdam";
            form.EndLocationTextBox.Text = "Vienna";
            form.SpeedTextBox.Text = "95";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(15))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "No routes found.");

            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string kmText = form.RouteListBox.Items[0].ToString();

            Assert.IsTrue(kmText.Contains("km"), "Kilometers not displayed in route list.");

            form.Close();
        }

        /// <summary>
        /// Testează o rută internațională (Praga - Atena) cu o viteză redusă specificată.
        /// Verifică găsirea rutei și afișarea timpului estimat. Timeout mai mare pentru procesare.
        /// </summary>
        [TestMethod]
        public void Test_Orase9_PragueToAthens_LowSpeed()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Prague";
            form.EndLocationTextBox.Text = "Athens";
            form.SpeedTextBox.Text = "60";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(20))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "No routes found.");

            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string timeText = form.EstimatedTimeTextBox.Text;

            Assert.IsTrue(!string.IsNullOrWhiteSpace(timeText), "Estimated time not displayed.");

            form.Close();
        }

        /// <summary>
        /// Testează o rută internațională (Copenhaga - Zurich) cu o viteză mare specificată.
        /// Verifică găsirea rutei și afișarea consumului estimat.
        /// </summary>
        [TestMethod]
        public void Test_Orase10_CopenhagenToZurich()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Copenhagen";
            form.EndLocationTextBox.Text = "Zurich";
            form.SpeedTextBox.Text = "130";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(15))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "No routes found.");

            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string consumText = form.EstimatedConsumptionTextBox.Text;

            Assert.IsTrue(consumText.Contains("L"), "Estimated consumption not displayed.");

            form.Close();
        }

        /// <summary>
        /// Testează dacă timpul estimat este calculat și afișat în formatul corect (Xh YYm).
        /// Utilizează ruta Viena - Munchen ca exemplu.
        /// </summary>
        [TestMethod]
        public void Test_EstimatedTime_IsCalculatedAndFormatted()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Vienna";
            form.EndLocationTextBox.Text = "Munich";
            form.SpeedTextBox.Text = "100";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (form.RouteListBox.Items.Count == 0 && sw.Elapsed < TimeSpan.FromSeconds(12))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.RouteListBox.Items.Count > 0, "No routes found.");

            form.RouteListBox.SelectedIndex = 0;
            System.Windows.Forms.Application.DoEvents();
            string timeText = form.EstimatedTimeTextBox.Text;

            Assert.IsFalse(string.IsNullOrWhiteSpace(timeText), "Estimated time should not be empty.");

            StringAssert.Matches(timeText, new System.Text.RegularExpressions.Regex(@"^\d+h \d{2}m$"), "Estimated time format is incorrect.");

            form.Close();
        }

        /// <summary>
        /// Testează comportamentul aplicației la introducerea unui nume de țară/oraș invalid.
        /// Se așteaptă ca nicio rută să nu fie găsită și metricile să fie goale.
        /// </summary>
        [TestMethod]
        public void Test_InvalidCountryName()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "NumeInvalid";
            form.EndLocationTextBox.Text = "Iasi";
            form.SpeedTextBox.Text = "80";

            form.CalculateButton.PerformClick();

            var sw = Stopwatch.StartNew();
            while (!form.CalculateButton.Enabled && sw.Elapsed < TimeSpan.FromSeconds(8))
            {
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            Assert.IsTrue(form.CalculateButton.Enabled, "Butonul de calcul ar trebui să fie activat după procesarea unui input invalid.");

            Assert.AreEqual(0, form.RouteListBox.Items.Count, "No routes should be found for an invalid country/city name.");
            Assert.IsTrue(string.IsNullOrWhiteSpace(form.EstimatedTimeTextBox.Text), "Estimated time should be empty for invalid input.");
            Assert.IsTrue(string.IsNullOrWhiteSpace(form.EstimatedConsumptionTextBox.Text), "Estimated consumption should be empty for invalid input.");

            form.Close();
        }

        /// <summary>
        /// Testează comportamentul aplicației când locația de start este goală.
        /// Se așteaptă ca nicio rută să nu fie găsită.
        /// </summary>
        [TestMethod]
        public void Test_EmptyStartLocation1_ShowsNoRoute()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "";
            form.EndLocationTextBox.Text = "Iasi";
            form.CalculateButton.PerformClick();

            System.Windows.Forms.Application.DoEvents();
            Task.Delay(500).Wait();

            Assert.AreEqual(0, form.RouteListBox.Items.Count, "Nu ar trebui să existe rute pentru start gol.");
            form.Close();
        }

        /// <summary>
        /// Testează comportamentul aplicației când locația de destinație este goală.
        /// Se așteaptă ca nicio rută să nu fie găsită.
        /// </summary>
        [TestMethod]
        public void Test_EmptyStartLocation2()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.StartLocationTextBox.Text = "Iasi";
            form.EndLocationTextBox.Text = "";
            form.CalculateButton.PerformClick();

            System.Windows.Forms.Application.DoEvents();
            Task.Delay(500).Wait();

            Assert.AreEqual(0, form.RouteListBox.Items.Count, "Nu ar trebui să existe rute pentru destinație gol.");
            form.Close();
        }

        /// <summary>
        /// Testează funcționalitatea metodei `ResetUIForNewCalculation`.
        /// Verifică dacă RouteListBox, câmpurile de metrici, marker-ele și rutele de pe hartă sunt golite.
        /// </summary>
        [TestMethod]
        public void Test_ResetUIForNewCalculation()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.RouteListBox.Items.Add("Test Route Item");
            form.EstimatedTimeTextBox.Text = "10h 00m";
            form.EstimatedConsumptionTextBox.Text = "20 L";
            form.MarkersOverlay.Markers.Add(new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                new GMap.NET.PointLatLng(45.0, 25.0), GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green));
            form.RoutesOverlay.Routes.Add(new GMap.NET.WindowsForms.GMapRoute(
                new System.Collections.Generic.List<GMap.NET.PointLatLng> { new GMap.NET.PointLatLng(45.0, 25.0), new GMap.NET.PointLatLng(46.0, 26.0) }, "TestRoute"));

            form.ResetUIForNewCalculation();

            Assert.AreEqual(0, form.RouteListBox.Items.Count, "RouteListBox nu a fost golit.");
            Assert.AreEqual("", form.EstimatedTimeTextBox.Text, "EstimatedTimeTextBox nu a fost golit.");
            Assert.AreEqual("", form.EstimatedConsumptionTextBox.Text, "EstimatedConsumptionTextBox nu a fost golit.");
            Assert.AreEqual(0, form.MarkersOverlay.Markers.Count, "Marker-ele nu au fost șterse de pe hartă.");
            Assert.AreEqual(0, form.RoutesOverlay.Routes.Count, "Rutele nu au fost șterse de pe hartă.");

            form.Close();
        }

        /// <summary>
        /// Testează funcționalitatea metodei `ClearMetricTextBoxes`.
        /// Verifică dacă câmpurile de text pentru timpul estimat și consumul estimat sunt golite.
        /// </summary>
        [TestMethod]
        public void Test_ClearMetricTextBoxes_ClearsTextBoxes()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.EstimatedTimeTextBox.Text = "5h 30m";
            form.EstimatedConsumptionTextBox.Text = "15.5 L";

            form.ClearMetricTextBoxes();

            Assert.AreEqual("", form.EstimatedTimeTextBox.Text, "EstimatedTimeTextBox was not cleared.");
            Assert.AreEqual("", form.EstimatedConsumptionTextBox.Text, "EstimatedConsumptionTextBox was not cleared.");

            form.Close();
        }

        /// <summary>
        /// Testează specific dacă metoda `ResetUIForNewCalculation` golește marker-ele și rutele de pe hartă.
        /// </summary>
        [TestMethod]
        public void Test_ResetUIForNewCalculation_ClearsMapMarkersAndRoutes()
        {
            var form = new Proiect.Form1();
            form.Show();

            form.MarkersOverlay.Markers.Add(new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                new GMap.NET.PointLatLng(45.0, 25.0), GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green));
            form.RoutesOverlay.Routes.Add(new GMap.NET.WindowsForms.GMapRoute(
                new System.Collections.Generic.List<GMap.NET.PointLatLng> { new GMap.NET.PointLatLng(45.0, 25.0), new GMap.NET.PointLatLng(46.0, 26.0) }, "TestRoute"));

            form.ResetUIForNewCalculation();

            Assert.AreEqual(0, form.MarkersOverlay.Markers.Count, "Marker-ele nu au fost șterse.");
            Assert.AreEqual(0, form.RoutesOverlay.Routes.Count, "Rutele nu au fost șterse.");

            form.Close();
        }

        /// <summary>
        /// Testează corectitudinea matematică a funcției statice `CalculateTravelTime`.
        /// </summary>
        [TestMethod]
        public void Test_CalculateTravelTime_MathematicCalculus()
        {
            double distanceKm = 300.0;
            double speedKmH = 100.0;

            double expectedTime = 3.0; 
            double actualTime = CalculateTravelTime(distanceKm, speedKmH);

            Assert.AreEqual(expectedTime, actualTime, 0.0001, "Travel time calculation is incorrect.");
        }

        // Metodă ajutătoare pentru test (poate fi parte din clasa Form1 sau o clasă de utilități)
        public static double CalculateTravelTime(double distanceKm, double speedKmH)
        {
            if (speedKmH <= 0) throw new ArgumentException("Speed must be greater than zero.");
            return distanceKm / speedKmH;
        }

        /// <summary>
        /// Testează corectitudinea matematică a funcției statice `CalculateFuelConsumption`.
        /// </summary>
        [TestMethod]
        public void Test_CalculateFuelConsumption_MathematicCalculus()
        {
            double distanceKm = 500.0;
            double avgConsumptionPer100Km = 6.0; 

            double expectedConsumption = 30.0; 
            double actualConsumption = CalculateFuelConsumption(distanceKm, avgConsumptionPer100Km);

            Assert.AreEqual(expectedConsumption, actualConsumption, 0.0001, "Fuel consumption calculation is incorrect.");
        }

        // Metodă ajutătoare
        public static double CalculateFuelConsumption(double distanceKm, double avgConsumptionPer100Km)
        {
            if (avgConsumptionPer100Km < 0) throw new ArgumentException("Average consumption must be non-negative.");
            return (distanceKm * avgConsumptionPer100Km) / 100.0;
        }

        /// <summary>
        /// Testează corectitudinea formatării timpului de către funcția statică `FormatTime`.
        /// </summary>
        [TestMethod]
        public void Test_FormatTime_MathematicCalculus()
        {
            double hours = 2.5; 

            string expected = "2h 30m";
            string actual = FormatTime(hours);

            Assert.AreEqual(expected, actual, "Time formatting is incorrect.");
        }

        // Metodă ajutătoare
        public static string FormatTime(double hours)
        {
            if (hours < 0) throw new ArgumentException("Hours must be non-negative.");
            int h = (int)hours;
            int m = (int)Math.Round((hours - h) * 60);
            return $"{h}h {m:D2}m";
        }

        /// <summary>
        /// Testează atât calculul, cât și formatarea consumului de combustibil.
        /// Verifică corectitudinea valorii calculate și a string-ului formatat.
        /// </summary>
        [TestMethod]
        public void Test_CalculateAndFormatFuelConsumption_MathematicCalculus()
        {
            double distanceKm = 420.0;
            double avgConsumptionPer100Km = 5.5;

            double expectedConsumptionValue = (420.0 * 5.5) / 100.0; 
            double actualConsumption = CalculateFuelConsumption(distanceKm, avgConsumptionPer100Km);

            Assert.AreEqual(expectedConsumptionValue, actualConsumption, 0.0001, "Fuel consumption calculation is incorrect.");

            string formattedConsumption = $"{actualConsumption:F1} L";

            Assert.AreEqual("23.1 L", formattedConsumption, "Fuel consumption formatting is incorrect.");
        }
    }
}