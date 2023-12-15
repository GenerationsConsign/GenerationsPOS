using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using GenerationsPOS.Data;
using System;

namespace GenerationsPOS.PointOfSale
{
    /// <summary>
    /// Additional functionality that will be handled identically regardless of platform
    /// </summary>
    public class UniversalCore
    {
        private readonly App? App;

        public UniversalCore(App? app)
        {
            // Store Application to be used for changing theme
            App = app;
        }

        public CompanySettings.Theme Theme
        {
            get => App.RequestedThemeVariant == ThemeVariant.Dark ? CompanySettings.Theme.Dark : CompanySettings.Theme.Light;
            set
            {
                var requestedTheme = value switch
                {
                    CompanySettings.Theme.Light => ThemeVariant.Light,
                    CompanySettings.Theme.Dark => ThemeVariant.Dark
                };
                App.RequestedThemeVariant = requestedTheme;
            }
        }

        public void AddShutdownHook(Action onShutdown)
        {
            if (App.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app)
            {
                app.Exit += (_, _) => onShutdown();
            }
        }
    }
}
