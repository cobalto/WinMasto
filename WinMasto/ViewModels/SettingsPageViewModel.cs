﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace WinMasto.ViewModels
{
    public class SettingsPageViewModel : WinMastoViewModel
    {
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();
    }

    public class SettingsPartViewModel : WinMastoViewModel
    {
        readonly Services.SettingsService _settings;

        public SettingsPartViewModel()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                _settings = Services.SettingsService.Instance;
        }

        public bool UseDarkThemeButton
        {
            get { return _settings.AppTheme.Equals(ApplicationTheme.Dark); }
            set { _settings.AppTheme = value ? ApplicationTheme.Dark : ApplicationTheme.Light; base.RaisePropertyChanged(); }
        }

        public bool UseBackgroundTask
        {
            get { return _settings.BackgroundEnable; }
            set { _settings.BackgroundEnable = value; base.RaisePropertyChanged(); }
        }

        public bool UseBackgroundLiveTile
        {
            get { return _settings.BackgroundTile; }
            set { _settings.BackgroundTile = value; base.RaisePropertyChanged(); }
        }

        public bool AlwaysShowNSFW
        {
            get { return _settings.AlwaysShowNSFW; }
            set { _settings.AlwaysShowNSFW = value; base.RaisePropertyChanged(); }
        }

        public bool UseBackgroundNotify
        {
            get { return _settings.ToastNotifications; }
            set { _settings.ToastNotifications = value; base.RaisePropertyChanged(); }
        }
    }

    public class AboutPartViewModel : WinMastoViewModel
    {
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var ver = Windows.ApplicationModel.Package.Current.Id.Version;
                return ver.Major.ToString() + "." + ver.Minor.ToString() + "." + ver.Build.ToString() + "." + ver.Revision.ToString();
            }
        }

        public Uri RateMe => new Uri("https://github.com/drasticactions/winmasto");
    }
}
