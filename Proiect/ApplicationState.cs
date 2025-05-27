/*********************************************************************************************
 *                                                                                           * 
 *  File:        ApplicationStates.cs (sau un nume similar, ex: StatePattern.cs)             *
 *  Copyright:   (c) 2025 Stoian Mario-Daniel, Chiriac Raluca-Ștefania, Chilimon Ana-Maria   *
 *  E-mail:      mario-daniel.stoian@student.tuiasi.ro,                                      *
 *               raluca-stefania.chiriac@student.tuiasi.ro,                                  *
 *               ana-maria.chilimon@student.tuiasi.ro                                        *
 *  Description: Acest fișier definește implementarea pattern-ului de design State           *
 *               pentru aplicația de simulare a navigației. Conține interfața                *
 *               `IApplicationState` și clasele concrete care reprezintă diferitele          *
 *               stări ale aplicației:                                                       *
 *               - `IApplicationState`: Definește contractul comun pentru toate stările,     *
 *                 incluzând metode pentru intrarea în stare (`Enter`), gestionarea          *
 *                 evenimentelor declanșate de utilizator (click pe calculare rută,          *
 *                 selecție rută din listă) și ieșirea din stare (`Exit`).                   *
 *               - `InitialState`: Reprezintă starea inițială a aplicației, unde             *
 *                 utilizatorul poate introduce locațiile. La intrare, resetează UI-ul.      *
 *                 La click pe "Calculate", validează input-ul și tranziționează către       *
 *                 `LoadingState`.                                                           *
 *               - `LoadingState`: Starea în care aplicația obține coordonatele, calculează  *
 *                 rutele și afișează marker-ele. Gestionează erorile posibile în acest      *
 *                 proces (locații negăsite, erori de la serviciul de rutare) și             *
 *                 tranziționează fie la `RoutesDisplayedState` (dacă rutele sunt găsite),   *
 *                 fie înapoi la `InitialState` (în caz de eroare).                          *
 *               - `RoutesDisplayedState`: Starea în care rutele sunt afișate pe hartă și    *
 *                 în listă. La intrare, desenează toate rutele, populează lista și          *
 *                 actualizează metricile pentru prima rută. Permite utilizatorului să       *
 *                 selecteze o rută (tranziționând la `RouteSelectedState`) sau să înceapă   *
 *                 o nouă căutare (revenind la `InitialState` și apoi `LoadingState`).       *
 *               - `RouteSelectedState`: Starea activată atunci când utilizatorul selectează *
 *                 o rută specifică din listă. La intrare, evidențiază ruta selectată pe     *
 *                 hartă și actualizează metricile corespunzătoare. Permite schimbarea       *
 *                 selecției sau o nouă căutare.                                             *
 *               Fiecare clasă de stare menține o referință la context (`Form1`) pentru a    *
 *               putea interacționa cu UI-ul și a declanșa tranziții între stări.            *
 *               Comentariile SMD și CAM indică contribuțiile specifice ale membrilor        *
 *               echipei în cadrul acestor stări.                                            *
 *                                                                                           *
 *********************************************************************************************/

using GMap.NET;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoutingServiceDLL; 

namespace Proiect
{
    /// <summary>
    /// Definește contractul pentru toate stările posibile ale aplicației.
    /// Fiecare stare trebuie să implementeze aceste metode pentru a gestiona
    /// logica specifică stării și tranzițiile către alte stări.
    /// </summary>
    public interface IApplicationState
    {
        /// <summary>
        /// Se execută la intrarea în această stare.
        /// Inițializează UI-ul sau logica specifică stării.
        /// </summary>
        void Enter();

        /// <summary>
        /// Gestionează acțiunea declanșată de click-ul pe butonul de calculare a rutei.
        /// </summary>
        Task HandleCalculateRouteClickedAsync();

        /// <summary>
        /// Gestionează acțiunea declanșată de selectarea unei rute din listă.
        /// </summary>
        /// <param name="selectedIndex">Indexul rutei selectate.</param>
        void HandleRouteSelectionChanged(int selectedIndex);

        /// <summary>
        /// Se execută la ieșirea din această stare.
        /// Curăță resursele sau UI-ul specific stării.
        /// </summary>
        void Exit();
    }

    /// <summary>
    /// Reprezintă starea inițială a aplicației.
    /// Utilizatorul poate introduce locațiile de start și destinație.
    /// </summary>
    public class InitialState : IApplicationState
    {
        private readonly Form1 _context;
        public InitialState(Form1 context) { _context = context; }

        /// <summary>
        /// La intrarea în starea inițială, se dezactivează starea de încărcare (loading)
        /// și se resetează interfața grafică la valorile implicite.
        /// </summary>
        public void Enter()
        {
            try
            {
                _context.SetLoadingState(false);
                _context.ResetUIForNewCalculation();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în InitialState.Enter: {ex.Message}");
                _context.ShowUserMessage($"Eroare la inițializarea stării: {ex.Message}", "Eroare Stare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Când se apasă butonul de calculare, se verifică dacă au fost introduse locațiile.
        /// Dacă da, se tranziționează la starea de încărcare (LoadingState).
        /// Altfel, se afișează un mesaj de avertizare.
        /// </summary>
        public async Task HandleCalculateRouteClickedAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_context.StartLocationTextBox.Text) ||
                    string.IsNullOrWhiteSpace(_context.EndLocationTextBox.Text))
                {
                    _context.ShowUserMessage("Te rog introdu locația de plecare și sosire.", "Input Incomplet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _context.TransitionToState(new LoadingState(_context));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în InitialState.HandleCalculateRouteClickedAsync: {ex.Message}");
                _context.ShowUserMessage($"Eroare la procesarea calculului: {ex.Message}", "Eroare Procesare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            await Task.CompletedTask; 
        }

        /// <summary>
        /// În starea inițială, selectarea unei rute din listă nu are niciun efect,
        /// deoarece lista ar trebui să fie goală.
        /// </summary>
        public void HandleRouteSelectionChanged(int selectedIndex)
        {
            // Nicio acțiune specifică în starea inițială
        }

        /// <summary>
        /// Nu sunt necesare acțiuni specifice la ieșirea din starea inițială.
        /// </summary>
        public void Exit() { }
    }

    /// <summary>
    /// Reprezintă starea în care aplicația încarcă datele necesare pentru rutare:
    /// obține coordonate, calculează rute, afișează marker-e.
    /// </summary>
    public class LoadingState : IApplicationState
    {
        private readonly Form1 _context;
        public LoadingState(Form1 context) { _context = context; }

        /// <summary>
        /// La intrarea în starea de încărcare:
        /// 1. Se activează indicatorul vizual de încărcare și se resetează UI-ul.
        /// 2. Se obțin asincron coordonatele pentru locațiile de start și destinație.
        /// 3. Dacă locațiile sunt invalide, se revine la starea inițială cu mesaj de eroare.
        /// 4. Se afișează marker-ele pe hartă.
        /// 5. Se apelează serviciul de rutare pentru a obține rutele.
        /// 6. Dacă se găsesc rute, se tranziționează la RoutesDisplayedState.
        /// 7. În caz de eroare sau dacă nu se găsesc rute, se revine la InitialState.
        /// </summary>
        public async void Enter()
        {
            try
            {
                _context.SetLoadingState(true);
                _context.ResetUIForNewCalculation();

                PointLatLng? coordPlecare = null;
                PointLatLng? coordSosire = null;

                try
                {
                    coordPlecare = await _context.GetCoordinatesForAddressAsync(_context.StartLocationTextBox.Text);
                    coordSosire = await _context.GetCoordinatesForAddressAsync(_context.EndLocationTextBox.Text);
                }
                catch (Exception exCoord)
                {
                    _context.ShowUserMessage($"Eroare la obținerea coordonatelor: {exCoord.Message}", "Eroare Geocodare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _context.TransitionToState(new InitialState(_context));
                    return;
                }

                if (coordPlecare == null)
                {
                    _context.ShowUserMessage("Locația de plecare nu a putut fi găsită sau nu a fost introdusă.", "Eroare Locație Plecare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _context.TransitionToState(new InitialState(_context));
                    return;
                }
                //---------SMD-----------
                if (coordSosire == null)
                {
                    _context.ShowUserMessage("Locația de sosire nu a putut fi găsită sau nu a fost introdusă.", "Eroare Locație Sosire", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _context.TransitionToState(new InitialState(_context));
                    return;
                }
                //---------SMD-----------/

                _context.DisplayMarkers(coordPlecare.Value, coordSosire.Value);

                var routingService = new RoutingService();
                _context.CurrentRoutes = await routingService.GetRoutesAsync(coordPlecare.Value, coordSosire.Value);

                if (_context.CurrentRoutes == null || _context.CurrentRoutes.Count == 0)
                {
                    _context.ShowUserMessage("Nu a putut fi găsită nicio rută între locațiile specificate!", "Atenție!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _context.TransitionToState(new InitialState(_context));
                    return;
                }
                _context.TransitionToState(new RoutesDisplayedState(_context));
            }
            catch (Exception ex) 
            {
                _context.ShowUserMessage($"Eroare generală în timpul încărcării: {ex.Message}", "Eroare Încărcare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _context.TransitionToState(new InitialState(_context));
            }
        }

        /// <summary>
        /// În starea de încărcare, un nou click pe butonul de calculare nu ar trebui să aibă efect
        /// sau ar putea anula operațiunea curentă (neimplementat aici).
        /// Momentan, nu face nimic.
        /// </summary>
        public async Task HandleCalculateRouteClickedAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// În starea de încărcare, selecția unei rute nu are sens.
        /// </summary>
        public void HandleRouteSelectionChanged(int selectedIndex) { }

        /// <summary>
        /// La ieșirea din starea de încărcare, se dezactivează indicatorul vizual de încărcare.
        /// </summary>
        public void Exit()
        {
            try
            {
                _context.SetLoadingState(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în LoadingState.Exit: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Reprezintă starea în care rutele calculate sunt afișate pe hartă și în listă.
    /// </summary>
    public class RoutesDisplayedState : IApplicationState
    {
        private readonly Form1 _context;
        public RoutesDisplayedState(Form1 context) { _context = context; }

        /// <summary>
        /// La intrarea în această stare:
        /// 1. Se dezactivează starea de încărcare.
        /// 2. Se afișează toate rutele pe hartă și se populează lista de rute.
        /// 3. Dacă există rute, se actualizează metricile pentru prima rută din listă
        ///    și se centrează harta pe rute.
        /// 4. Altfel, se afișează o eroare și se revine la starea inițială.
        /// </summary>
        public void Enter()
        {
            try
            {
                _context.SetLoadingState(false);
                _context.DisplayAllRoutesOnMap();
                _context.PopulateRouteListBox();

                if (_context.CurrentRoutes != null && _context.CurrentRoutes.Count > 0)
                {
                    //---------SMD-----------
                    _context.UpdateRouteMetrics(_context.CurrentRoutes[0]); 
                    //---------SMD-----------/
                    _context.GMapControl.ZoomAndCenterRoutes("routes");
                }
                else
                {
                    _context.ShowUserMessage("Eroare internă: Nu sunt rute de afișat în RoutesDisplayedState.", "Eroare Afișare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _context.TransitionToState(new InitialState(_context));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în RoutesDisplayedState.Enter: {ex.Message}");
                _context.ShowUserMessage($"Eroare la afișarea rutelor: {ex.Message}", "Eroare Afișare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _context.TransitionToState(new InitialState(_context));
            }
        }

        /// <summary>
        /// Dacă se apasă butonul de calculare, se consideră o nouă cerere de rutare.
        /// Se tranziționează înapoi la starea inițială, apoi se declanșează din nou calculul
        /// (care va duce la LoadingState).
        /// </summary>
        public async Task HandleCalculateRouteClickedAsync()
        {
            try
            {
                _context.TransitionToState(new InitialState(_context)); 
                await _context.TriggerRouteCalculationAsync();          
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în RoutesDisplayedState.HandleCalculateRouteClickedAsync: {ex.Message}");
                _context.ShowUserMessage($"Eroare la inițierea unui nou calcul: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Când utilizatorul selectează o rută din listă:
        /// - Dacă nu este selectat niciun element (selectedIndex == -1), se curăță metricile
        ///   și se resetează evidențierea rutelor.
        /// - Dacă este selectată o rută validă, se tranziționează la RouteSelectedState.
        /// </summary>
        public void HandleRouteSelectionChanged(int selectedIndex)
        {
            try
            {
                //--------------CAM----------------
                if (selectedIndex == -1) 
                {
                    //---------SMD-----------
                    _context.ClearMetricTextBoxes();
                    //---------SMD-----------/
                    _context.ResetRouteHighlightingAndListBox(); 
                    return;
                }


                if (_context.CurrentRoutes != null && selectedIndex >= 0 && selectedIndex < _context.CurrentRoutes.Count)
                {
                    _context.TransitionToState(new RouteSelectedState(_context, selectedIndex));
                }
                //--------------CAM----------------/
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în RoutesDisplayedState.HandleRouteSelectionChanged: {ex.Message}");
                _context.ShowUserMessage($"Eroare la selectarea rutei: {ex.Message}", "Eroare Selecție", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Nu sunt necesare acțiuni specifice la ieșirea din această stare.
        /// </summary>
        public void Exit() { }
    }

    /// <summary>
    /// Reprezintă starea în care o rută specifică este selectată de utilizator.
    /// </summary>
    public class RouteSelectedState : IApplicationState
    {
        private readonly Form1 _context;
        private readonly int _selectedIndex; 

        public RouteSelectedState(Form1 context, int selectedIndex)
        {
            _context = context;
            _selectedIndex = selectedIndex;
        }

        /// <summary>
        /// La intrarea în această stare:
        /// 1. Se dezactivează starea de încărcare.
        /// 2. Se evidențiază ruta selectată pe hartă și se actualizează lista.
        /// 3. Se actualizează metricile pentru ruta selectată.
        /// 4. Dacă ruta selectată nu mai este validă, se revine la RoutesDisplayedState.
        /// </summary>
        public void Enter()
        {
            try
            {
                _context.SetLoadingState(false);
                //--------------CAM----------------
                _context.HighlightSelectedRouteAndUpdateListBox(_selectedIndex);
                //--------------CAM----------------/

                if (_context.CurrentRoutes != null && _selectedIndex >= 0 && _selectedIndex < _context.CurrentRoutes.Count)
                {
                    //---------SMD-----------
                    _context.UpdateRouteMetrics(_context.CurrentRoutes[_selectedIndex]);
                    //---------SMD-----------/
                }
                else
                {
                    _context.TransitionToState(new RoutesDisplayedState(_context));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în RouteSelectedState.Enter: {ex.Message}");
                _context.ShowUserMessage($"Eroare la afișarea rutei selectate: {ex.Message}", "Eroare Afișare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _context.TransitionToState(new RoutesDisplayedState(_context)); 
            }
        }

        /// <summary>
        /// Similar cu RoutesDisplayedState, un nou click pe calculare inițiază o nouă rutare.
        /// </summary>
        public async Task HandleCalculateRouteClickedAsync()
        {
            try
            {
                _context.TransitionToState(new InitialState(_context));
                await _context.TriggerRouteCalculationAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în RouteSelectedState.HandleCalculateRouteClickedAsync: {ex.Message}");
                _context.ShowUserMessage($"Eroare la inițierea unui nou calcul: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Când utilizatorul schimbă selecția în listă:
        /// - Dacă nu mai este nicio rută selectată, se revine la RoutesDisplayedState și se curăță metricile.
        /// - Dacă este selectată o altă rută validă, se tranziționează la o nouă instanță
        ///   de RouteSelectedState cu noul index.
        /// </summary>
        public void HandleRouteSelectionChanged(int newSelectedIndex)
        {
            try
            {
                //--------------CAM----------------
                if (newSelectedIndex == -1) // Nicio selecție
                {
                    //---------SMD-----------
                    _context.ClearMetricTextBoxes();
                    //---------SMD-----------/
                    _context.TransitionToState(new RoutesDisplayedState(_context)); 
                    return;
                }

                // Dacă s-a selectat o altă rută validă
                if (newSelectedIndex != _selectedIndex &&
                    _context.CurrentRoutes != null &&
                    newSelectedIndex >= 0 &&
                    newSelectedIndex < _context.CurrentRoutes.Count)
                {
                    _context.TransitionToState(new RouteSelectedState(_context, newSelectedIndex));
                }
                //--------------CAM----------------/
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în RouteSelectedState.HandleRouteSelectionChanged: {ex.Message}");
                _context.ShowUserMessage($"Eroare la schimbarea selecției rutei: {ex.Message}", "Eroare Selecție", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Nu sunt necesare acțiuni specifice la ieșirea din această stare,
        /// deoarece noua stare (de obicei o altă RouteSelectedState sau RoutesDisplayedState)
        /// va redesena UI-ul corespunzător.
        /// </summary>
        public void Exit()
        {
        }
    }
}