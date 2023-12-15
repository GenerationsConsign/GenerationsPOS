using CommunityToolkit.Mvvm.ComponentModel;
using GenerationsPOS.PointOfSale;
using GenerationsPOS.ViewModels.StatusPane;
using System.IO;

namespace GenerationsPOS.ViewModels;

/// <summary>
/// ViewModel for main container: navigation stack on left side and active POS operation in remainder
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    // Instances of all main POS operation pages
    // Some operations, such as the invoice manager page, persist throughout the application so they can be resumed by the user
    // Welcome page persists as it is static and there is no benefit to regeneration.
    private readonly WelcomeViewModel WelcomePage;
    private readonly ActiveInvoiceViewModel InvoicePage;
    private readonly GenerateSalesViewModel ReportsPage;
    public StatusPaneViewModel StatusPane { get; private set; }

    // Some other operations, such as consignor sales reports and company settings, are generated when required instead. If they are closed, they should be opened clean when they are required by the store.
    private GenerateConsignorViewModel ConsignorPage => new(Generations);
    private CompanySettingsViewModel SettingsPage => new(this, Generations);
    
    /// <summary>
    /// Initalize the program to the welcome page
    /// </summary>
    public MainViewModel(IGenerationsCore core) : base(core)
    {
        // Initalize main POS operation pages which persist throughout application
        WelcomePage = new(Generations);
        InvoicePage = new(Generations);
        ReportsPage = new(Generations);
        StatusPane = new(Generations);

        CurrentOperation = WelcomePage;
    }


    /// <summary>
    /// If no core is specified, a GenerationsRemoteCore will be generated.
    /// This is only intended for use to allow the designer to function.
    /// </summary>
    public MainViewModel() : this(new GenerationsRemoteCore(new UniversalCore(null)))
    {
    }


    /// <summary>
    ///  The current operational view to be displayed within the main window, defaults to welcome page
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SalesActive), nameof(ReportsActive), nameof(ConsignorReportsActive), nameof(SettingsActive))]
    private ViewModelBase _CurrentOperation;

    // Properties for allowing indication of which menu is active
    public bool SalesActive
    {
        get => CurrentOperation == InvoicePage;
    }

    public bool ReportsActive
    {
        get => CurrentOperation == ReportsPage;
    }
    
    public bool ConsignorReportsActive
    {
        get => CurrentOperation is GenerateConsignorViewModel;
    }

    public bool SettingsActive
    {
        get => CurrentOperation is CompanySettingsViewModel;
    }

    /// <summary>
    /// Switches the active scene to the WelcomeViewModel instrance
    /// </summary>
    public void WelcomeWindow() => CurrentOperation = WelcomePage;

    /// <summary>
    /// Switches the active scene to the ActiveInvoiceViewModel instance 
    /// </summary>
    public void SalesWindow() => CurrentOperation = InvoicePage;

    /// <summary>
    /// Switches the active scene to the GenerateSalesViewModel instance
    /// </summary>
    public void ReportWindow() => CurrentOperation = ReportsPage;

    /// <summary>
    /// Instanciates a new GenerateConsignorViewModel and switches the active scene to it
    /// </summary>
    public void ConsignorWindow() => CurrentOperation = ConsignorPage;

    /// <summary>
    /// Instanciates a new CompanySettingsViewModel and switches the active scene to it
    /// </summary>
    public void CompanySettings() => CurrentOperation = SettingsPage;

    /// <summary>
    /// Requests the physical cash drawer to be opened
    /// Then, a modal will be opened to notify the user of this operation
    /// </summary>
    public void OpenCashDrawer()
    {
        // TODO: open popup/modal 
        try
        {
            Generations.CashDrawer.OpenDrawer();
        } 
        catch (IOException ex)
        {
            Generations.Status.Notify($"Failed to open cash drawer: {ex.Message}");
            logger.Error(ex, "Unable to open physical cash drawer");
        }
    }
}
