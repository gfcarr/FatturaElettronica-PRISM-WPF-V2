using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Data;
using System.Windows.Automation;
using System.Collections.ObjectModel;
using FatturaElettronica_PRISM_WPF_V2.Models;
using FatturaElettronica_PRISM_WPF_V2.ExtensionMethods;
using FatturaElettronica_PRISM_WPF_V2.Services;
using Prism.Events;
using Prism.Regions;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;



namespace FatturaElettronica_PRISM_WPF_V2.ViewModels;
public class ContentViewModel : BindableBase
{

    private IRegionManager _regionManager;
    private IDbManager _dbManager;
    private IEventAggregator _eventAggregator;
    private ISharedDataStore _sharedData;
    private DbFattureContext _dbcontext;
        
   
    private ObservableCollection<Fattura> elencoFattureOC;
    private ObservableCollection<SoggettoFiltro> elencoClientiOC;
    private ObservableCollection<SoggettoFiltro> elencoFornitoriOC;
    private ObservableCollection<string> elencoProvinceOC;
    private ObservableCollection<string> elencoComuniOC;
    private ObservableCollection<string> elencoAnniOC;
    private ObservableCollection<string> elencoMesiOC;

    // COMANDI
    public DelegateCommand AggiornaDBCommand { get; private set; }
    public DelegateCommand ExitCommand { get; private set; }
    public DelegateCommand<string> RimuoviFiltroCommand { get; private set;  }
    //******************************************************************
    //
    // WATERMARKS per il Controllo Filtro delle ListBox di Selezione Filtri
    // Proprietà:
    private string _watermarkTextProvincia;
    public string WatermarkTextProvincia
    {
        get => _watermarkTextProvincia;
        set => SetProperty(ref _watermarkTextProvincia, value);
    }
    // Proprietà:
    private string _watermarkTextComune;
    public string WatermarkTextComune
    {
        get => _watermarkTextComune;
        set => SetProperty(ref _watermarkTextComune, value);
    }
    // Proprietà:
    private string _watermarkTextCliente;
    public string WatermarkTextCliente
    {
        get => _watermarkTextCliente;
        set => SetProperty(ref _watermarkTextCliente, value);
    }
    // Proprietà:
    private string _watermarkTextFornitore;
    public string WatermarkTextFornitore
    {
        get => _watermarkTextFornitore;
        set => SetProperty(ref _watermarkTextFornitore, value);
    }
    //******************************************************************
    //
    // COLLECTIONVIEWSOURCE per le ListBox di Selezione Filtri
    // Proprietà:
    private CollectionViewSource _elencoClientiVS;
    public CollectionViewSource ElencoClientiVS
    {
        get => _elencoClientiVS;
        set => SetProperty(ref _elencoClientiVS, value);
    }
    // Proprietà:
    private CollectionViewSource _elencoFornitoriVS;
    public CollectionViewSource ElencoFornitoriVS
    {
        get => _elencoFornitoriVS;
        set => SetProperty(ref _elencoFornitoriVS, value);
    }
    // Proprietà:
    private CollectionViewSource _elencoProvinceVS;
    public CollectionViewSource ElencoProvinceVS
    {
        get => _elencoProvinceVS;
        set => SetProperty(ref _elencoProvinceVS, value);
    }
    // Proprietà:
    private CollectionViewSource _elencoComuniVS;
    public CollectionViewSource ElencoComuniVS
    {
        get => _elencoComuniVS;
        set => SetProperty(ref _elencoComuniVS, value);
    }
    // Proprietà
    private CollectionViewSource _elencoFattureVS;
    public CollectionViewSource ElencoFattureVS
    {
        get => _elencoFattureVS;
        set => SetProperty(ref _elencoFattureVS, value);
    }
    // Proprietà
    private CollectionViewSource _elencoAnniVS;
    public CollectionViewSource ElencoAnniVS
    {
        get => _elencoAnniVS;
        set => SetProperty(ref _elencoAnniVS, value);
    }
    // Proprietà
    private CollectionViewSource _elencoMesiVS;
    public CollectionViewSource ElencoMesiVS
    {
        get => _elencoMesiVS;
        set => SetProperty(ref _elencoMesiVS, value);
    }

    //******************************************************************
    //
    // ELEMENTI SELEZIONATI - SelectedItem delle ListBox di Selezione Filtri
    // Proprietà
    private string _clienteSelezionato;
    public string ClienteSelezionato
    {
        get => _clienteSelezionato;
        set => SetProperty(ref _clienteSelezionato, value);
    }
    // Proprietà
    private string _fornitoreSelezionato;
    public string FornitoreSelezionato
    {
        get => _fornitoreSelezionato;
        set => SetProperty(ref _fornitoreSelezionato, value);
    }
    // Proprietà
    private SoggettoFiltro _soggettoFiltroSelezionato;
    public SoggettoFiltro SoggettoFiltroSelezionato
    {
        get
        {
            if (_soggettoFiltroSelezionato != null)
            {
                if (_soggettoFiltroSelezionato.Denominazione != elencoFornitoriOC.First().Denominazione)
                {
                    if (_soggettoFiltroSelezionato.Tipo == "F")
                    {
                        CanRemoveFiltroFornitore = true;
                        FilterTextFornitore = null;
                    }
                    else
                    {
                        CanRemoveFiltroCliente = true;
                        FilterTextCliente = null;
                    }
                }
            }
            return _soggettoFiltroSelezionato;
        }
        set
        {
            SetProperty(ref _soggettoFiltroSelezionato, value);
            if (_soggettoFiltroSelezionato != null)
            {
                if (_soggettoFiltroSelezionato.Tipo == "F")
                {
                    _fornitoreSelezionato = _soggettoFiltroSelezionato.Denominazione;
                    _sharedData.FornitoreSelezionato = _fornitoreSelezionato;
                    var test = string.IsNullOrEmpty(_fornitoreSelezionato) || _fornitoreSelezionato == elencoFornitoriOC.First().Denominazione;
                    ApplicaFiltro(test ? FilterField.NoFornitore : FilterField.Fornitore);
                }
                else
                {
                    _clienteSelezionato = _soggettoFiltroSelezionato.Denominazione;
                    _sharedData.ClienteSelezionato = _clienteSelezionato;
                    var test = string.IsNullOrEmpty(_clienteSelezionato) || _clienteSelezionato == elencoClientiOC.First().Denominazione;
                    ApplicaFiltro(test ? FilterField.NoCliente : FilterField.Cliente);
                }
            }
        }
    }
    // Proprietà:  
    private string _provinciaSelezionata;
    public string ProvinciaSelezionata
    {
        get
        {
            if (_provinciaSelezionata != null && _provinciaSelezionata != elencoProvinceOC.First())
            {
                CanRemoveFiltroProvincia = true;
                FilterTextProvincia = null;
            }
            return _provinciaSelezionata;
        }
        set
        {
            SetProperty(ref _provinciaSelezionata, value);
            var test = string.IsNullOrEmpty(_provinciaSelezionata) || _provinciaSelezionata == elencoProvinceOC.First();
            ApplicaFiltro(test ? FilterField.NoProvincia : FilterField.Provincia);
        }
    }
    // Proprietà:
    private string _comuneSelezionato;
    public string ComuneSelezionato
    {
        get
        {
            if (_comuneSelezionato != null && _comuneSelezionato != elencoComuniOC.First())
            {
                CanRemoveFiltroComune = true;
                FilterTextComune = null;
            }
            return _comuneSelezionato;
        }
        set
        {
            SetProperty(ref _comuneSelezionato, value);
            var test = string.IsNullOrEmpty(_comuneSelezionato) || _comuneSelezionato == elencoComuniOC.First();
            ApplicaFiltro(test ? FilterField.NoComune : FilterField.Comune);
        }
    }
    // Proprietà:
    private string _annoSelezionato;
    public string AnnoSelezionato
    {
        get => _annoSelezionato;
        set
        {
            SetProperty(ref _annoSelezionato, value);
            if (_annoSelezionato != null)
            {
                var test = string.IsNullOrEmpty(_annoSelezionato) || _annoSelezionato == elencoAnniOC.First();
                ApplicaFiltro(test ? FilterField.NoAnno : FilterField.Anno);
            }
        }
    }
    // Proprietà:
    private string _meseSelezionato;
    public string MeseSelezionato
    {
        get => _meseSelezionato;
        set
        {
            SetProperty(ref _meseSelezionato, value);
           
            
                var test = string.IsNullOrEmpty(_meseSelezionato) || _meseSelezionato == elencoMesiOC.First();
                ApplicaFiltro(test ? FilterField.NoMese : FilterField.Mese);
            
        }
    }
    //*********************************************************
    // Proprietà:
    private bool _canRemoveFiltroCliente;
    public bool CanRemoveFiltroCliente
    {
        get => _canRemoveFiltroCliente;
        set => SetProperty(ref _canRemoveFiltroCliente, value);
    }
    // Proprietà:
    private bool _canRemoveFiltroFornitore;
    public bool CanRemoveFiltroFornitore
    {
        get => _canRemoveFiltroFornitore;
        set => SetProperty(ref _canRemoveFiltroFornitore, value);
    }
    // Proprietà:
    private bool _canRemoveFiltroComune;
    public bool CanRemoveFiltroComune
    {
        get => _canRemoveFiltroComune;
        set => SetProperty(ref _canRemoveFiltroComune, value);
    }
    // Proprietà:
    private bool _canRemoveFiltroProvincia;
    public bool CanRemoveFiltroProvincia
    {
        get => _canRemoveFiltroProvincia;
        set => SetProperty(ref _canRemoveFiltroProvincia, value);
    }
    //**********************************************************
    // Proprietà:
    private string _filterTextCliente;
    public string FilterTextCliente
    {
        get => _filterTextCliente?.ToUpperInvariant();
        set
        {
            if (_filterTextCliente != value)
            {
                SetProperty(ref _filterTextCliente, value);
                ElencoClientiVS.View.Refresh();
            }
        }
    }
    // Proprietà:
    private string _filterTextFornitore;
    public string FilterTextFornitore
    {
        get => _filterTextFornitore?.ToUpperInvariant();
        set
        {
            if (_filterTextFornitore != value)
            {
                SetProperty(ref _filterTextFornitore, value);
                ElencoFornitoriVS.View.Refresh();
            }
        }
    }
    // Proprietà:
    private string _filterTextProvincia;
    public string FilterTextProvincia
    {
        get => _filterTextProvincia?.ToUpperInvariant();
        set
        {
            if (_filterTextProvincia != value)
            {
                SetProperty(ref _filterTextProvincia, value);
                ElencoProvinceVS.View.Refresh();
            }
        }
    }
    // Proprietà:
    private string _filterTextComune;
    public string FilterTextComune
    {
        get => _filterTextComune?.ToUpperInvariant();
        set
        {
            if (_filterTextComune != value)
            {
                SetProperty(ref _filterTextComune, value);
                ElencoComuniVS.View.Refresh();
            }
        }
    }

    public ContentViewModel(IRegionManager regionManager, IDbManager dbManager, IEventAggregator ea, ISharedDataStore sharedData)
    {
        Initialize(regionManager, dbManager, ea, sharedData);
    }

    public void Initialize(IRegionManager regionManager, IDbManager dbManager, IEventAggregator ea, ISharedDataStore sharedData)
    {
        _regionManager = regionManager;
        _dbManager = dbManager;
        _sharedData = sharedData;
        _eventAggregator = ea;
        
        _sharedData = sharedData;
        elencoFattureOC = _sharedData.FattureOC;
        _elencoFattureVS = new() { Source = elencoFattureOC };

        // imposta ambiente per il filtro Clienti delle Fatture
        elencoClientiOC = elencoFattureOC.GroupBy(f => f.IdClienteNavigation.Denominazione,
                              (soggetto, fatture) => new SoggettoFiltro()
                              {
                                  Tipo = "C",
                                  Denominazione = soggetto.ToUpperInvariant(),
                                  Frequenza = fatture.Count()
                              }).ToObservableCollection();
        elencoClientiOC.Insert(0, new SoggettoFiltro() { Tipo = "C", Denominazione = " * Tutti i Clienti *", Frequenza = elencoClientiOC.Count });
        _elencoClientiVS = new() { Source = elencoClientiOC };
        _clienteSelezionato = "";
        _canRemoveFiltroCliente = false;
        _watermarkTextCliente = "Cerca un Cliente...";
        _filterTextCliente = null;
        _elencoClientiVS.View.Filter = ApplicaFiltroElencoClienti;

        // imposta ambiente per il filtro Fornitori delle Fatture
        elencoFornitoriOC = elencoFattureOC.GroupBy(f => f.IdFornitoreNavigation.Denominazione,
                              (soggetto, fatture) => new SoggettoFiltro()
                              {
                                  Tipo = "F",
                                  Denominazione = soggetto.ToUpperInvariant(),
                                  Frequenza = fatture.Count()
                              }).ToObservableCollection();
        elencoFornitoriOC.Insert(0, new SoggettoFiltro() { Tipo = "F", Denominazione = " * Tutti i Fornitori *", Frequenza = elencoFornitoriOC.Count });
        _elencoFornitoriVS = new() { Source = elencoFornitoriOC };
        FornitoreSelezionato = "";
        CanRemoveFiltroFornitore = false;
        WatermarkTextFornitore = "Cerca un Fornitore...";
        FilterTextFornitore = null;
        _elencoFornitoriVS.View.Filter = ApplicaFiltroElencoFornitori;

        // imposta ambiente per il filtro Anni delle Fatture
        elencoAnniOC = elencoFattureOC.Select(f => f.Anno).Distinct().OrderBy(a => a).ToObservableCollection();
        elencoAnniOC.Insert(0, "TUTTI GLI ANNI");
        _elencoAnniVS = new() { Source = elencoAnniOC };
        _annoSelezionato = "";

        // imposta ambiente per il filtro Mesi delle Fatture
        elencoMesiOC = new() {"TUTTI I MESI", "GENNAIO", "FEBBRAIO", "MARZO", "APRILE", "MAGGIO", "GIUGNO", "LUGLIO",
                                              "AGOSTO", "SETTEMBRE", "OTTOBRE", "NOVEMBRE", "DICEMBRE" };
       
        _elencoMesiVS = new() { Source = elencoMesiOC };
        _meseSelezionato = "";


        //// imposta ambiente per la Provincia
        //elencoProvinceOC = elencoClientiOC.Select(c => c.Provincia).Distinct().OrderBy(x => x).ToObservableCollection();
        //elencoProvinceOC.Insert(0, " * Tutte le Province * ");
        //_elencoProvinceVS = new() { Source = elencoProvinceOC };
        //_provinciaSelezionata = elencoProvinceOC.First();
        //_canRemoveFiltroProvincia = false;
        //_watermarkTextProvincia = "Cerca una Provincia...";
        //_filterTextProvincia = null;
        //_elencoProvinceVS.View.Filter = ApplicaFiltroElencoProvince;

        //// imposta ambiente per il Comune
        //elencoComuniOC = clientiOC.Select(c => c.Comune).Distinct().OrderBy(x => x).ToObservableCollection();
        //elencoComuniOC.Insert(0, " * Tutti i Comuni *");
        //_elencoComuniVS = new() { Source = elencoComuniOC };
        //_comuneSelezionato = elencoComuniOC.First();
        //_canRemoveFiltroComune = false;
        //_watermarkTextComune = "Cerca un Comune...";
        //_filterTextComune = null;
        //_elencoComuniVS.View.Filter = ApplicaFiltroElencoComuni;

        // Comando: PulsanteRimuoviFiltroCommand -> passa come parametro una stringa che contiene nella
        //                                          variabile nomeFiltro il nome del filtro
        RimuoviFiltroCommand = new DelegateCommand<string>(RimuoviFiltroElenco);

        AggiornaDBCommand = new DelegateCommand(AggiornaDB);
        ExitCommand = new DelegateCommand(Exit);
    }

    public void RimuoviFiltroElenco(string nomeDelFiltro)
    {
        var nomeFiltro = nomeDelFiltro.ToUpper();
        if (nomeFiltro.Contains("PROVINCIA"))
        {
            if (CanRemoveFiltroProvincia)
            {
                ProvinciaSelezionata = null;
                CanRemoveFiltroProvincia = false;
                FilterTextProvincia = null;
            }
        }
        if (nomeFiltro.Contains("COMUNE"))
        {
            if (CanRemoveFiltroComune)
            {
                ComuneSelezionato = null;
                CanRemoveFiltroComune = false;
                FilterTextComune = null;
            }
        }
        if (nomeFiltro.Contains("CLIENTE"))
        {
            if (CanRemoveFiltroCliente)
            {
                SoggettoFiltroSelezionato = null;
                ClienteSelezionato = null;
                CanRemoveFiltroCliente = false;
                FilterTextCliente = null;
                ApplicaFiltro(FilterField.NoCliente);
            }
        }
        if (nomeFiltro.Contains("FORNITORE"))
        {
            if (CanRemoveFiltroFornitore)
            {
                SoggettoFiltroSelezionato = null;
                FornitoreSelezionato = null;
                CanRemoveFiltroFornitore = false;
                FilterTextFornitore = null;
                ApplicaFiltro(FilterField.NoFornitore);
            }
        }

    }


    // FILTER REGION
    private enum FilterField
    {
        Provincia,
        Comune,
        Cliente,
        Fornitore,
        Anno,
        Mese,
        NoComune,
        NoProvincia,
        NoCliente,
        NoFornitore,
        NoAnno,
        NoMese,
        None
    }
    private void ApplicaFiltro(FilterField field)
    {
        switch (field)
        {
            case FilterField.Provincia:
            case FilterField.Comune:
            case FilterField.Cliente:
            case FilterField.Fornitore:
            case FilterField.Anno:
            case FilterField.Mese:
                AggiungiFiltroView(field);
                break;
            case FilterField.NoProvincia:
            case FilterField.NoComune:
            case FilterField.NoCliente:
            case FilterField.NoFornitore:
            case FilterField.NoAnno:
            case FilterField.NoMese:
                RimuoviFiltroView(field);
                break;
            default:
                break;
        }
    }
    private void AggiungiFiltroView(FilterField field)
    {
        var nomeFiltro = field.ToString().ToUpper();
        if (nomeFiltro.Contains("CLIENTE"))
        {
            if (_canRemoveFiltroCliente)
            {
                _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerCliente);
                _elencoFattureVS.Filter += new FilterEventHandler(FiltraPerCliente);
            }
        }
        if (nomeFiltro.Contains("FORNITORE"))
        {
            if (_canRemoveFiltroFornitore)
            {
                _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerFornitore);
                _elencoFattureVS.Filter += new FilterEventHandler(FiltraPerFornitore);
            }
        }
        if (nomeFiltro.Contains("PROVINCIA"))
        {
            if (_canRemoveFiltroProvincia)
            {
                _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerProvincia);
                _elencoFattureVS.Filter += new FilterEventHandler(FiltraPerProvincia);
            }
        }
        if (nomeFiltro.Contains("COMUNE"))
        {
            if (_canRemoveFiltroComune)
            {
                _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerComune);
                _elencoFattureVS.Filter += new FilterEventHandler(FiltraPerComune);
            }
        }
        if (nomeFiltro.Contains("ANNO"))
        {
            _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerAnno);
            _elencoFattureVS.Filter += new FilterEventHandler(FiltraPerAnno);
            
        }
        if (nomeFiltro.Contains("MESE"))
        {
           _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerMese);
           _elencoFattureVS.Filter += new FilterEventHandler(FiltraPerMese);
        }
    }
    private void RimuoviFiltroView(FilterField field)
    {
        var nomeFiltro = field.ToString().ToUpper();
        if (nomeFiltro.Contains("CLIENTE"))
        {
            _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerCliente);
            //MoveToFirst();
        }
        if (nomeFiltro.Contains("FORNITORE"))
        {
            _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerFornitore);
            //MoveToFirst();
        }

        if (nomeFiltro.Contains("PROVINCIA"))
        {
            _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerProvincia);
            //MoveToFirst();
        }
        if (nomeFiltro.Contains("COMUNE"))
        {
            _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerComune);
            //MoveToFirst();
        }
        if (nomeFiltro.Contains("ANNO"))
        {
            _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerAnno);
            //MoveToFirst();
        }
        if (nomeFiltro.Contains("MESE"))
        {
            _elencoFattureVS.Filter -= new FilterEventHandler(FiltraPerMese);
            //MoveToFirst();
        }
    }
    private void FiltraPerCliente(object sender, FilterEventArgs e)
    {
        // see Notes on Filter Methods:
        if (((Fattura)e.Item).IdClienteNavigation.Denominazione is string src)
        {
            if (string.Compare(_clienteSelezionato, src) != 0)
            {
                e.Accepted = false;
            }
        }
        else
        {
            e.Accepted = false;
        }
    }
    private void FiltraPerFornitore(object sender, FilterEventArgs e)
    {
        // see Notes on Filter Methods:
        if (((Fattura)e.Item).IdFornitoreNavigation.Denominazione is string src)
        {
            if (string.Compare(_fornitoreSelezionato, src) != 0)
            {
                e.Accepted = false;
            }
        }
        else
        {
            e.Accepted = false;
        }
    }
    private void FiltraPerProvincia(object sender, FilterEventArgs e)
    {
        // see Notes on Filter Methods:
        if (((Cliente)e.Item).Provincia is string src)
        {
            if (string.Compare(_provinciaSelezionata, src) != 0)
            {
                e.Accepted = false;
            }
        }
        else
        {
            e.Accepted = false;
        }
    }
    private void FiltraPerComune(object sender, FilterEventArgs e)
    {
        // see Notes on Filter Methods:
        if (((Cliente)e.Item).Comune is string src)
        {
            if (string.Compare(_comuneSelezionato, src) != 0)
            {
                e.Accepted = false;
            }
        }
        else
        {
            e.Accepted = false;
        }
    }
    private void FiltraPerAnno(object sender, FilterEventArgs e)
    {
        // see Notes on Filter Methods:
        if (((Fattura)e.Item).Anno is string src)
        {
            if (string.Compare(_annoSelezionato, src) != 0)
            {
                e.Accepted = false;
            }
        }
        else
        {
            e.Accepted = false;
        }
    }
    private void FiltraPerMese(object sender, FilterEventArgs e)
    {
        // see Notes on Filter Methods:
        if (((Fattura)e.Item).Mese is string src)
        {
            if (string.Compare(elencoMesiOC.IndexOf(_meseSelezionato).ToString().PadLeft(2,'0'), src) != 0)
            {
                e.Accepted = false;
            }
        }
        else
        {
            e.Accepted = false;
        }
    }

    // PROVINCE - COMUNI FILTER REGION
    public bool ApplicaFiltroElencoProvince(object item)
    {
        var nome = (string)item;
        if (nome == null )
        {
            return false;
        }
        if (string.IsNullOrEmpty(FilterTextProvincia) || FilterTextProvincia.StartsWith(" T"))
        {
            return true;
        }
        if (nome.StartsWith(FilterTextProvincia, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        return false;
    }
    public bool ApplicaFiltroElencoComuni(object item)  
    {
        var nome = (string)item;
        if (nome == null)
        {
            return false;
        }
        if (string.IsNullOrEmpty(FilterTextComune) || FilterTextComune.StartsWith(" T"))
        {
            return true;
        }
        if (nome.StartsWith(FilterTextComune, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        return false;
    }
    public bool ApplicaFiltroElencoClienti(object item)
    {
        var nome = ((SoggettoFiltro)item).Denominazione;
        if (nome == null)
        {
            return false;
        }
        if (string.IsNullOrEmpty(FilterTextCliente) || FilterTextCliente.StartsWith(" * T"))
        {
            return true;
        }
        if (nome.StartsWith(FilterTextCliente, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        return false;
    }
    public bool ApplicaFiltroElencoFornitori(object item)
    {
        var nome = ((SoggettoFiltro)item).Denominazione;
        if (nome == null)
        {
            return false;
        }
        if (string.IsNullOrEmpty(FilterTextFornitore) || FilterTextFornitore.StartsWith(" * T"))
        {
            return true;
        }
        if (nome.StartsWith(FilterTextFornitore, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        return false;
    }

    private void AggiornaDB()
    {
        _dbcontext = _dbManager.DBcontext;
        _dbManager.UpdateDatabase(_dbcontext, _sharedData);
    }

    private void Exit()
    {
        _dbManager.DBcontext.Dispose();
        System.Windows.Application.Current.MainWindow.Close();
    }



}
