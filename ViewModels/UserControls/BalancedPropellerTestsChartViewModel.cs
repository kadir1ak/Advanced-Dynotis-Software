using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancedPropellerTestsChartViewModel : INotifyPropertyChanged
    {
        private string _balancedPropellerID; // şuan arayüzümde gözlemlediğim pervane  ıd
        private double _balancedPropellerArea; // şuan arayüzümde gözlemlediğim pervane  boyutu
        private ObservableCollection<DateTime> _balancingTestDates; // şuan arayüzümde gözlemlediğim pervaneye ait test tarihleri
        private ObservableCollection<double> _vibrationLevels;// şuan arayüzümde gözlemlediğim pervane testlerine ait titreşim verileri
        private InterfaceVariables _interfaceVariables;
        private DispatcherTimer _interfaceVariablesTimer;
        public BalancedPropellerTestsChartViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _interfaceVariablesTimer = new DispatcherTimer();
            _interfaceVariablesTimer.Interval = TimeSpan.FromMicroseconds(1);
            _interfaceVariablesTimer.Tick += InterfaceVariablesTimer_Tick;
            _interfaceVariablesTimer.Start();

        }
        private void InterfaceVariablesTimer_Tick(object sender, EventArgs e)
        {
            if (_interfaceVariables != null)
            {

                BalancedPropellerID = _interfaceVariables.BalancedPropellersID;
                BalancedPropellerArea = _interfaceVariables.BalancedPropellersArea;
                BalancingTestDates = _interfaceVariables.BalancedPropellersTestDates;
                VibrationLevels = _interfaceVariables.BalancedPropellersVibrations;

                // Tare değerlerini InterfaceVariables.Instance'da saklayın
                InterfaceVariables.Instance.BalancedPropellersID = BalancedPropellerID;
                InterfaceVariables.Instance.BalancedPropellersArea = BalancedPropellerArea;
                InterfaceVariables.Instance.BalancedPropellersTestDates = BalancingTestDates;
                InterfaceVariables.Instance.BalancedPropellersVibrations = VibrationLevels;
            }
        }
        public string BalancedPropellerID
        {
            get => _balancedPropellerID;
            set
            {
                if (SetProperty(ref _balancedPropellerID, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public double BalancedPropellerArea
        {
            get => _balancedPropellerArea;
            set
            {
                if (SetProperty(ref _balancedPropellerArea, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<DateTime> BalancingTestDates
        {
            get => _balancingTestDates;
            set
            {
                if (SetProperty(ref _balancingTestDates, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<double> VibrationLevels
        {
            get => _vibrationLevels;
            set
            {
                if (SetProperty(ref _vibrationLevels, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
