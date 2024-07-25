using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancingRoutingStepsViewModel : INotifyPropertyChanged
    {
        private int _balancingIterationStep;
        private InterfaceVariables _interfaceVariables;
        private int _currentStepIndex;
        private readonly List<string> _steps;

        public BalancingRoutingStepsViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            RunCommand = new RelayCommand(param => Run());
            SaveCommand = new RelayCommand(param => Save());
            StepCommand = new RelayCommand(param => Step());

            _steps = new List<string>
            {
                "Cihazın Hazırlanması ve Başlangıç Titreşiminin Ölçülmesi",
                "Pervane Titreşim Ölçümü (Ağırlıksız)",
                "Balans Yönünün Belirlenmesi (0 Derece Pozisyonu)",
                "Balans Yönünün Belirlenmesi (180 Derece Pozisyonu)",
                "Balanslama için Değerlerin Hesaplanması",
                "Test ve Değerlendirme",
                "Balanslama İyileşme Oranının Hesaplanması"
            };

            _currentStepIndex = 0;
        }

        public ICommand RunCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand StepCommand { get; }

        public List<string> Steps => _steps;

        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    OnPropertyChanged(nameof(CurrentStepIndex));
                }
            }
        }

        public int BalancingIterationStep
        {
            get => _balancingIterationStep;
            set
            {
                if (SetProperty(ref _balancingIterationStep, value))
                {
                    _interfaceVariables.BalancingIterationStep = value;
                    OnPropertyChanged(nameof(BalancingIterationStep));
                }
            }
        }

        private void Run()
        {
            // Çalıştırma sistemi yazılacak
            MessageBox.Show("Run");
        }

        private void Save()
        {
            // Kayıt sistemi yazılacak
            MessageBox.Show("Save");
        }

        private void Step()
        {
            if (_currentStepIndex < _steps.Count - 1)
            {
                CurrentStepIndex++;
                BalancingIterationStep = CurrentStepIndex;
                InterfaceVariables.Instance.BalancingIterationStep = BalancingIterationStep;
                //MessageBox.Show($"Step: {CurrentStepIndex + 1} - {_steps[CurrentStepIndex]}");
            }
            else
            {
                CurrentStepIndex =0;
                BalancingIterationStep = CurrentStepIndex;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
