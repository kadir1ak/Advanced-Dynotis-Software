using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using LiveCharts;
using LiveCharts.Wpf;

public class BalancerVibrationLevelsViewModel : INotifyPropertyChanged
{
    private InterfaceVariables _interfaceVariables;
    private DynotisData _dynotisData;

    public BalancerVibrationLevelsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
    {
        _interfaceVariables = interfaceVariables;
        _dynotisData = dynotisData;

        _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

        HighVibrations = new List<double>();
        BalancerIterationStep = 0;

        VibrationSeriesCollection = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Vibration Levels",
                Values = new ChartValues<double>(HighVibrations)
            }
        };

        XFormatter = value => value.ToString("0.00");
        YFormatter = value => value.ToString("0");
    }

    private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InterfaceVariables.HighVibrations))
        {
            HighVibrations = _interfaceVariables.HighVibrations;
            UpdateVibrationChart();
        }
        else if (e.PropertyName == nameof(InterfaceVariables.BalancerIterationStep))
        {
            BalancerIterationStep = _interfaceVariables.BalancerIterationStep;
            UpdateVibrationChart();
        }
    }

    private List<double> _highVibrations;
    public List<double> HighVibrations
    {
        get => _highVibrations;
        set
        {
            if (SetProperty(ref _highVibrations, value))
            {
                UpdateVibrationChart();
            }
        }
    }

    private int _balancerIterationStep;
    public int BalancerIterationStep
    {
        get => _balancerIterationStep;
        set => SetProperty(ref _balancerIterationStep, value);
    }

    public SeriesCollection VibrationSeriesCollection { get; set; }
    public Func<double, string> XFormatter { get; set; }
    public Func<double, string> YFormatter { get; set; }

    private void UpdateVibrationChart()
    {
        if (VibrationSeriesCollection != null && VibrationSeriesCollection.Count > 0)
        {
            var series = VibrationSeriesCollection[0] as LineSeries;
            if (series != null)
            {
                series.Values = new ChartValues<double>(HighVibrations);
            }
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
