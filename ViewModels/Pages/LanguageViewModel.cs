﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using Advanced_Dynotis_Software.Properties;
using Advanced_Dynotis_Software.Services.Helpers;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class LanguageViewModel : INotifyPropertyChanged
    {
        public InterfaceVariables InterfaceVariables => InterfaceVariables.Instance;

        public bool IsTurkishChecked
        {
            get => InterfaceVariables.SelectedIsTurkishChecked;
            set
            {
                if (InterfaceVariables.SelectedIsTurkishChecked != value)
                {
                    InterfaceVariables.SelectedIsTurkishChecked = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        IsEnglishChecked = false;
                        App.ChangeLanguage("tr-TR");
                    }
                    else
                    {
                        IsEnglishChecked = true;
                    }
                }
            }
        }

        public bool IsEnglishChecked
        {
            get => InterfaceVariables.SelectedIsEnglishChecked;
            set
            {
                if (InterfaceVariables.SelectedIsEnglishChecked != value)
                {
                    InterfaceVariables.SelectedIsEnglishChecked = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        IsTurkishChecked = false;
                        App.ChangeLanguage("en-EN");
                    }
                    else
                    {
                        IsTurkishChecked = true;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
