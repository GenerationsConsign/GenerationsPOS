using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using GenerationsPOS.Data;
using GenerationsPOS.PointOfSale;
using GenerationsPOS.ViewModels;
using GenerationsPOS.Views;
using System;

namespace GenerationsPOS;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        IGenerationsCore? generations;
        UniversalCore universal = new UniversalCore(this);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            generations = new GenerationsDesktopCore(universal);
            // Set initial application theme
            universal.Theme = (CompanySettings.Theme)generations!.Configuration.UserTheme;

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(generations)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            generations = new GenerationsRemoteCore(universal);
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(generations)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
