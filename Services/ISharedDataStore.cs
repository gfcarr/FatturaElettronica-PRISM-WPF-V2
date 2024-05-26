using FatturaElettronica_PRISM_WPF_V2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FatturaElettronica_PRISM_WPF_V2.Services;

public interface ISharedDataStore
{
    string FattureInputDirectory { get; set; }   
    
    // interfacce per la variabili globali che rappresentano le collezioni lette dal database che così sono disponibili globalmente alla partenza 
    ObservableCollection<Cliente> ClientiOC { get; set; }
    ObservableCollection<Fornitore> FornitoriOC { get; set; }
    ObservableCollection<Fattura> FattureOC { get; set; }
    ObservableCollection<RigaDettaglio> RigheDettaglioOC { get; set; }


    // interfacce per le variabili globali per memorizzare il valore selezionato nelle ComboBox di filtro (proprietà SelectedItem)
    string ProvinciaSelezionata { get; set; }
    string ComuneSelezionato { get; set; }
    string TipoSelezionato { get; set; }
    string FornitoreSelezionato { get; set; }
    string ClienteSelezionato { get; set; }
    string AnnoSelezionato { get; set; }
    string MeseSelezionato { get; set; }
    string FatturaSelezionataFileName { get; set; }

    // interfacce per le variabili globali per memorizzare gli elenchi dei possibili valori da inserire nelle ComboBox di filtro (proprietà ItemsSource)
    Dictionary<string, List<string>> ElencoComuniProvinciaClientiDict { get; set; }
    Dictionary<string, List<string>> ElencoComuniProvinciaFornitoriDict { get; set; }
    Dictionary<string, List<string>> ElencoTipiDenominazioniFattureDict { get; set; }
    ObservableCollection<string> ElencoTipiOC { get; set; }
    ObservableCollection<string> ElencoFornitoriOC { get; set; }
    ObservableCollection<string> ElencoClientiOC { get; set; }
    ObservableCollection<string> ElencoAnniOC { get; set; }
    ObservableCollection<string> ElencoMesiOC { get; set; }

}

