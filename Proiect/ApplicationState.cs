using GMap.NET;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;


/*********************************************************************************************
 *                                                                                           * 
 *  File:        RouteCalculator.cs                                                          *
 *  Copyright:   (c) 2025 Stoian Mario-Daniel, Chiriac Raluca-Ștefania, Chilimon Ana-Maria   *
 *  E-mail:      mario-daniel.stoian@student.tuiasi.ro,                                      *
 *               ana-maria.chilimon@student.tuiasi.ro                                        *
 *                                                                                           *
 *                                                                                           *
 *********************************************************************************************/

namespace Proiect
{
    public interface IApplicationState
    {
        void Enter();
        Task HandleCalculateRouteClickedAsync();
        void HandleRouteSelectionChanged(int selectedIndex);
        void Exit();
    }

    public class InitialState : IApplicationState
    {
        private readonly Form1 _context;
        public InitialState(Form1 context) { _context = context; }

        public void Enter()
        {
            _context.SetLoadingState(false);
            _context.ResetUIForNewCalculation(); 
        }

        public async Task HandleCalculateRouteClickedAsync()
        {
            if (string.IsNullOrWhiteSpace(_context.StartLocationTextBox.Text) ||
                string.IsNullOrWhiteSpace(_context.EndLocationTextBox.Text))
            {
                _context.ShowUserMessage("Te rog introdu locația de plecare și sosire.", "Input Incomplet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _context.TransitionToState(new LoadingState(_context));
        }

        public void HandleRouteSelectionChanged(int selectedIndex)
        {
        }

        public void Exit() { }
    }

    public class LoadingState : IApplicationState
    {
        private readonly Form1 _context;
        public LoadingState(Form1 context) { _context = context; }

        public async void Enter() 
        {
            _context.SetLoadingState(true);
            _context.ResetUIForNewCalculation(); 

            var coordPlecare = await _context.GetCoordinatesForAddressAsync(_context.StartLocationTextBox.Text);
            var coordSosire = await _context.GetCoordinatesForAddressAsync(_context.EndLocationTextBox.Text);

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
            try
            {
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
                _context.ShowUserMessage($"Eroare la obținerea rutelor: {ex.Message}", "Eroare Serviciu Rutare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _context.TransitionToState(new InitialState(_context));
            }
        }

        public async Task HandleCalculateRouteClickedAsync()
        {
            await Task.CompletedTask;
        }

        public void HandleRouteSelectionChanged(int selectedIndex) { }
        public void Exit()
        {
            _context.SetLoadingState(false);
        }
    }

    public class RoutesDisplayedState : IApplicationState
    {
        private readonly Form1 _context;
        public RoutesDisplayedState(Form1 context) { _context = context; }

        public void Enter()
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
                _context.ShowUserMessage("Eroare internă: Nu sunt rute de afișat.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _context.TransitionToState(new InitialState(_context));
            }
        }

        public async Task HandleCalculateRouteClickedAsync()
        {
            _context.TransitionToState(new InitialState(_context));
            await _context.TriggerRouteCalculationAsync();
        }

        public void HandleRouteSelectionChanged(int selectedIndex)
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
        public void Exit() { }
    }

    public class RouteSelectedState : IApplicationState
    {
        private readonly Form1 _context;
        private readonly int _selectedIndex;

        public RouteSelectedState(Form1 context, int selectedIndex)
        {
            _context = context;
            _selectedIndex = selectedIndex;
        }

        public void Enter()
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

        public async Task HandleCalculateRouteClickedAsync()
        {
            _context.TransitionToState(new InitialState(_context));
            await _context.TriggerRouteCalculationAsync();
        }

        public void HandleRouteSelectionChanged(int newSelectedIndex)
        {
            //--------------CAM----------------
            if (newSelectedIndex == -1)
            {
                //---------SMD-----------
                _context.ClearMetricTextBoxes();
                //---------SMD-----------/
                _context.TransitionToState(new RoutesDisplayedState(_context));
                return;
            }

            if (newSelectedIndex != _selectedIndex && _context.CurrentRoutes != null && newSelectedIndex >= 0 && newSelectedIndex < _context.CurrentRoutes.Count)
            {
                _context.TransitionToState(new RouteSelectedState(_context, newSelectedIndex));
            }
            //--------------CAM----------------/
        }
        public void Exit()
        {
        }
    }
}