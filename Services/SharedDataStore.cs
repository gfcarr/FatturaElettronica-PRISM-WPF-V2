using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FatturaElettronica_PRISM_WPF_V2.Models;

namespace FatturaElettronica_PRISM_WPF_V2.Services;

public class SharedDataStore : ISharedDataStore
{
    // put public properties and methods here...
    public string FattureInputDirectory { get; set; }

    // variabili globali che rappresentano le collezioni lette dal database che così sono disponibili globalmente alla partenza 
    public ObservableCollection<Cliente> ClientiOC { get; set; }
    public ObservableCollection<Fornitore> FornitoriOC { get; set; }
    public ObservableCollection<Fattura> FattureOC { get; set; }
    public ObservableCollection<RigaDettaglio> RigheDettaglioOC { get; set; }


    // variabili globali per memorizzare il valore selezionato nelle ComboBox di filtro (proprietà SelectedItem)
    public string ProvinciaSelezionata { get; set; }
    public string ComuneSelezionato { get; set; }
    public string TipoSelezionato { get; set; }
    public string FornitoreSelezionato { get; set; }
    public string ClienteSelezionato { get; set; }
    public string AnnoSelezionato { get; set; }
    public string MeseSelezionato { get; set; }
    public string FatturaSelezionataFileName { get; set; }


    // variabili globali per memorizzare gli elenchi dei possibili valori da inserire nelle ComboBox di filtro (proprietà ItemsSource)

    public Dictionary<string, List<string>> ElencoComuniProvinciaClientiDict { get; set; }
    public Dictionary<string, List<string>> ElencoComuniProvinciaFornitoriDict { get; set; }
    public Dictionary<string, List<string>> ElencoTipiDenominazioniFattureDict { get; set; }
    public ObservableCollection<string> ElencoTipiOC { get; set; }
    public ObservableCollection<string> ElencoFornitoriOC { get; set; }
    public ObservableCollection<string> ElencoClientiOC { get; set; }
    public ObservableCollection<string> ElencoAnniOC { get; set; }
    public ObservableCollection<string> ElencoMesiOC { get; set; }
}
