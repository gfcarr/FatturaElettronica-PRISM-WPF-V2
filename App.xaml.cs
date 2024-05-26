using System.Windows;
using Prism.Ioc;
using FatturaElettronica_PRISM_WPF_V2.Views;
using FatturaElettronica_PRISM_WPF_V2.Services;
using Prism.Regions;



namespace FatturaElettronica_PRISM_WPF_V2;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // register needed SINGLETON SERVICES here
        //
        containerRegistry.RegisterSingleton<IDbManager, DbManager>();
        containerRegistry.RegisterSingleton<ISharedDataStore, SharedDataStore>();
        // register other needed services here
        //
        containerRegistry.RegisterForNavigation<ContentView>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var regionManager = Container.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<ContentView>("ContentRegion");
    }





}
