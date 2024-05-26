using FatturaElettronica.Ordinaria;
using FatturaElettronica;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using FatturaElettronica_PRISM_WPF_V2.ExtensionMethods;
using FatturaElettronica_PRISM_WPF_V2.Models;

namespace FatturaElettronica_PRISM_WPF_V2.Services;

public class DbManager : IDbManager
{
    public const string databasename = "DBFatture";
    public const string fattureInputDir = "C:\\Users\\Gianfranco\\Documents\\Agricoltura\\FATTURE ELETTRONICHE PROVA\\";

    private ISharedDataStore _sharedData;

    public DbFattureContext DBcontext { get; }

    // costruttore della classe ->  legge il database e crea il dbcontext, poi carica le tabelle nelle OC globali della classe SharedDataStore
    //                              che possono essere lette dai ViewModel dei dati senza accedere al database
    public DbManager(ISharedDataStore sharedData)
    {
        DBcontext = ReadDatabase();
        _sharedData = sharedData;    
        ShareDatabase(DBcontext, _sharedData);
    }
    public DbFattureContext ReadDatabase()
    {
        var dbcontext = new DbFattureContext();
        return dbcontext;
    }
    public void UpdateDatabase(DbFattureContext dbcontext, ISharedDataStore sharedData)
    {
        var MessageText = "";
        var res = UpdateDb(dbcontext, ref MessageText);
        if (res)
        {
            MessageBox.Show(MessageText, "Aggiorna Database Fatture", MessageBoxButton.OK, MessageBoxImage.Information);
            ShareDatabase(dbcontext, sharedData);
        }
        else
        {
            MessageBox.Show(MessageText, "Aggiorna Database Fatture", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    public void ShareDatabase(DbFattureContext dbcontext, ISharedDataStore sharedData)
    {
        ShareDbData(dbcontext, sharedData);
    }
    public static bool UpdateDb(DbFattureContext dbcontext, ref string MessageText)
    {
        // Programma che legge i file xml / p7m delle fatture elettroniche dalla directory di input ed aggiorna il database
        // ATTENZIONE: il database deve essere già esistente, a creato prima con l'altra applicazione

        var anniNumDict = new Dictionary<string, int>();
        var res = false;

        res = DbExists(databasename);
        if (!res)
        {
            MessageText = ("AGGDB - ERRORE 01 - Aggiornamento del database NON RIUSCITO perchè il database \"" + databasename + "\" NON ESISTE nel SQLServer LocalDB");
            return false;
        }
        var context = dbcontext;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");

        // Register the CodePages encoding provider at application startup to enable using single and double byte encodings.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var windows1252Encoding = Encoding.GetEncoding(1252); // Western European (Windows)

        // Impostazioni generali per la lettura di un file XML
        var readerSettings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };

        // carica le tabelle del database in dizionari corrispondenti e l'elenco dei nomi dei file delle fatture in una lista
        var clientiDict = context.Clienti.Select(x => new
        {
            Id = x.PartitaIVA,
            value = x
        }).ToDictionary(x => x.Id, x => x.value);
        var fornitoriDict = context.Fornitori.Select(x => new
        {
            Id = x.PartitaIVA,
            value = x
        }).ToDictionary(x => x.Id, x => x.value);
        var fattureDict = context.Fatture.Select(x => new
        {
            Id = x.NomeFile,
            value = x
        }).ToDictionary(x => x.Id, x => x.value);
        var righedettaglioDict = context.RigheDettaglio.Select(x => new
        {
            Id = x.IdRigaDettaglio,
            value = x
        }).ToDictionary(x => x.Id, x => x.value);

        // legge tutti i file .XMl o XML.P7M dalla directory di input dei file XML / P7M delle fatture elettroniche,
        // escludendo quelli già presenti nel database -->> seleziona solo le nuove fatture
        var numprog = 0;
        var filenames = Directory.GetFiles(fattureInputDir, "*.xml*").Where(f => !fattureDict.ContainsKey(Path.GetFileName(f)));
        if (!filenames.Any())
        {
            MessageText = "AGGDB 01 - Non ci sono NUOVE FATTURE da inserire nel Database";
            return true;
        }
        foreach (var filename in filenames)
        {
            // ciclo solo sulle NUOVE FATTURE
            // legge lo stream del file XML della fattura
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            // trasforma il file xml o p7m nell'oggetto di tipo FatturaOrdinaria usando la libreria OpenSource FatturaElettronica
            var fattura = (FatturaOrdinaria)FatturaBase.CreateInstanceFromXml(stream);
            // nei file delle nostre fatture elettroniche c'è sempre 1 solo documento (fattura) quindi non serve un ciclo foreach
            // legge i campi dell'header della fattura
            var header = fattura.FatturaElettronicaHeader;

            // dati del CessionarioCommittente (FORNITORE --> chi EMETTE LA FATTURA)
            // vede se il fornitore della nuova fattura è presente o no nel database (nel dizionario generato dal database) -> se no lo aggiunge nel dizionario e nel database
            var partitaIVACedentePrestatore = "IT" + header.CedentePrestatore.DatiAnagrafici.IdFiscaleIVA.IdCodice;
            if (!fornitoriDict.ContainsKey(partitaIVACedentePrestatore))
            {
                // legge gli altri dati del nuovo fornitore dall'oggetto FatturaElettronica
                var denominazioneCedentePrestatore = header.CedentePrestatore.DatiAnagrafici.Anagrafica.Denominazione;
                denominazioneCedentePrestatore ??= header.CedentePrestatore.DatiAnagrafici.Anagrafica.CognomeNome;
                var provinciaCedentePrestatore = header.CedentePrestatore.Sede.Provincia ?? "FG";
                var capCedentePrestatore = header.CedentePrestatore.Sede.CAP;
                var comuneCedentePrestatore = header.CedentePrestatore.Sede.Comune.ToUpper();
                var indirizzoCedentePrestatore = header.CedentePrestatore.Sede.Indirizzo;
                var numerocivicoCedentePrestatore = header.CedentePrestatore.Sede.NumeroCivico;
                if (numerocivicoCedentePrestatore != null)
                {
                    indirizzoCedentePrestatore += " " + numerocivicoCedentePrestatore;
                }
                // crea il nuovo oggetto Fornitore
                Fornitore f = new()
                {
                    PartitaIVA = partitaIVACedentePrestatore.ToUpper(),
                    Denominazione = denominazioneCedentePrestatore.ToUpper(),
                    Provincia = provinciaCedentePrestatore.ToUpper(),
                    Comune = comuneCedentePrestatore.ToUpper(),
                    CAP = capCedentePrestatore,
                    Indirizzo = indirizzoCedentePrestatore.ToUpper(),
                    FattureFornitoreOC = []
                };
                // cerca di aggiungere il nuovo fornitore al dizionario dei fornitori
                res = fornitoriDict.TryAdd(partitaIVACedentePrestatore, f);
                if (!res)
                {
                    MessageText = "AGGDB - ERRORE 02 - Il fornitore con Partita IVA " + partitaIVACedentePrestatore + " è già presente nel database";
                    return false;
                }
                // aggiunge il nuovo fornitore alla Tabella Fornitori del database DBFatture
                context.Fornitori.Add(f);
                context.SaveChanges();
            }

            // dati del CessionarioCommittente (CLIENTE --> chi RICEVE LA  FATTURA)
            // vede se il cliente della nuova fattura è presente o no nel database (nel dizionario generato dal database) -> se no lo aggiunge nel dizionario e nel database
            var partitaIVACessionarioCommittente = "IT" + header.CessionarioCommittente.DatiAnagrafici.IdFiscaleIVA.IdCodice;
            if (!clientiDict.ContainsKey(partitaIVACessionarioCommittente))
            {
                // legge gli altri dati del nuovo cliente dall'oggetto FatturaElettronica
                var denominazioneCessionarioCommittente = header.CessionarioCommittente.DatiAnagrafici.Anagrafica.Denominazione
                                                          ?? header.CessionarioCommittente.DatiAnagrafici.Anagrafica.CognomeNome;
                var provinciaCessionarioCommittente = header.CessionarioCommittente.Sede.Provincia ?? "FG";
                var capCessionarioCommittente = header.CessionarioCommittente.Sede.CAP;
                var comuneCessionarioCommittente = header.CessionarioCommittente.Sede.Comune;
                var indirizzoCessionarioCommittente = header.CessionarioCommittente.Sede.Indirizzo;
                var numerocivicoCessionarioCommittente = header.CessionarioCommittente.Sede.NumeroCivico;
                if (numerocivicoCessionarioCommittente != null)
                {
                    indirizzoCessionarioCommittente += " " + numerocivicoCessionarioCommittente;
                }
                // crea il nuovo oggetto Cliente
                Cliente c = new()
                {
                    PartitaIVA = partitaIVACessionarioCommittente.ToUpper(),
                    Denominazione = denominazioneCessionarioCommittente.ToUpper(),      
                    Provincia = provinciaCessionarioCommittente.ToUpper(),
                    Comune = comuneCessionarioCommittente.ToUpper(),
                    CAP = capCessionarioCommittente.ToUpper(),
                    Indirizzo = indirizzoCessionarioCommittente.ToUpper(),
                    FattureClienteOC = []
                };
                // cerca di aggiungere il nuovo cliente al dizionario
                res = clientiDict.TryAdd(partitaIVACessionarioCommittente, c);
                if (!res)
                {
                    MessageText = "AGGDB - ERRORE 03 - Il cliente con Partita IVA " + partitaIVACessionarioCommittente + " è già presente nel dizionario dei clienti";
                    return false;
                }
                // aggiunge il nuovo cliente alla Tabella Clienti del database DBFatture
                context.Clienti.Add(c);
                context.SaveChanges();
            }

            // legge i dati generali della fattura occorrenti per generare una stringa di controllo dell'unicità della fattura nel dizionario delle fatture
            var doc = fattura.FatturaElettronicaBody.Single();
            var datiDocumento = doc.DatiGenerali.DatiGeneraliDocumento;
            var numeroDocumento = datiDocumento.Numero;
            var dataEmissioneDocumento = datiDocumento.Data.ToShortDateString();
            // per controllare se la fattura corrente è unica, genera una stringa di controllo che identifica univocamente la fattura
            // check_fattura =  partitaIVA fornitore - partitaIVA cliente - data di emissione - numero fattura
            // non possono esistere due fatture corrette con tale stringa identica
            // var check_fattura = partitaIVACedentePrestatore + "-" + partitaIVACessionarioCommittente + "-" + dataEmissioneDocumento + "-" + numeroDocumento;
            // se la fattura corrente non è già presente nel dizionario delle fatture (non è presente il file xml, il cui nome è univoco)
            // crea un nuovo oggetto di tipo Fattura lo aggiunge al dizionario delle fatture ed alla tabella Fatture del database DBFatture
            var NomeFile = Path.GetFileName(filename);
            if (!fattureDict.ContainsKey(NomeFile))
            {
                // legge gli altri dati della fattura corrente
                var anno = dataEmissioneDocumento[6..];
                var mese = dataEmissioneDocumento[3..5];
                var giorno = dataEmissioneDocumento[0..2];
                numprog++;
                res = anniNumDict.TryAdd(anno, numprog);
                if (!res)
                {
                    numprog = anniNumDict[anno];
                    anniNumDict[anno] = ++numprog;
                }
                // genera il codice univoco IdFattura che è la chiave della Tabella Fatture del database DBFatture
                // IdFatture = Anno (4 cifre) - mese (2 cifre) - giorno (2 cifre) - numero progressivo per anno delle fatture lette dai file della directory di input (5 cifre) -> es. "2024-05-00137" 
                var IdFattura = anno + "-" + mese + "-" + giorno + "-" + numprog.ToString().PadLeft(5, '0');
                var importoTotaleDocumento = datiDocumento.ImportoTotaleDocumento;
                var datiBeniServizi = doc.DatiBeniServizi;
                var datiRiepilogo = datiBeniServizi.DatiRiepilogo;
                var imponibile_totale = datiRiepilogo.Sum(item => item.ImponibileImporto);
                var imposta_totale = datiRiepilogo.Sum(item => item.Imposta);
                // crea l'oggetto Fattura con i dati della nuova fattura
                var ft = new Fattura
                {
                    IdFattura = IdFattura,
                    // se la partita IVA è di Patrizia la fattura è EMESSA, altrimenti è RICEVUTA
                    Tipo = (partitaIVACedentePrestatore == "IT04079640712") ? "E" : "R",
                    NomeFile = NomeFile,
                    IdFornitore = partitaIVACedentePrestatore,
                    IdCliente = partitaIVACessionarioCommittente,
                    Numero = numeroDocumento,
                    Anno = anno,
                    Mese = mese,
                    Giorno = giorno,
                    Imponibile = imponibile_totale,
                    Imposta = imposta_totale,
                    Importo = imponibile_totale + imposta_totale,
                    IdClienteNavigation = clientiDict[partitaIVACessionarioCommittente],
                    IdFornitoreNavigation = fornitoriDict[partitaIVACedentePrestatore],
                    RigheDettaglioFatturaOC = []
                };
                res = fattureDict.TryAdd(NomeFile, ft);
                if (!res)
                {
                    Console.WriteLine("AGGDB - ERRORE 04 - La fattura nel file " + NomeFile + " è già presente nel Database");
                    return false;
                }
                // aggiunge la nuova fattura alla Tabella Fatture del database DBFatture
                context.Fatture.Add(ft);
                context.SaveChanges();

                // aggiunge la nuova fattura alla ObservableCollection FattureFornitoreOC dell'oggetto Fornitore corrente, trovato inserendo la 
                // partitaIVACedentePrestatore  del file xml/p7m corrente come chiave nel dizionario dei fornitori
                fornitoriDict[partitaIVACedentePrestatore].FattureFornitoreOC.Add(ft);

                // aggiunge la nuova fattura alla ObservableCollection FattureClientiOC dell'oggetto Cliente corrente, trovato inserendo la 
                // partitaIVACessionarioCommittente  del file xml/p7m corrente come chiave nel dizionario dei clienti
                clientiDict[partitaIVACessionarioCommittente].FattureClienteOC.Add(ft);

                // legge le linee di dettaglio (articoli) della fattura
                var datiDettaglioLinee = datiBeniServizi.DettaglioLinee;
                var n_linea = 1;
                foreach (var linea in datiDettaglioLinee)
                {
                    // se la riga di dettaglio non è una semplice riga di descrizione ma contiene una quantità ed un prezzo unitario elabora la riga
                    if (linea.Quantita > 0 && linea.PrezzoUnitario > 0)
                    {

                        var codart_tot = (linea.CodiceArticolo.Count > 0) ? linea.CodiceArticolo.First().CodiceValore : "";
                        var codart = (codart_tot.Length > 15) ? codart_tot[..15] : codart_tot.PadRight(15);
                        var desc_tot = linea.Descrizione;
                        var desc = (desc_tot.Length > 50) ? desc_tot[..50] : desc_tot.PadRight(50);
                        var qtà = linea.Quantita.Value;
                        var pr_unitario = linea.PrezzoUnitario;
                        // per controllare se la riga di dettaglio è unica nella fattura, genera una stringa di controllo che identifica univocamente la riga di dettaglio
                        // check_riga =  NomeFile - codice articolo - quantità - prezzo unitario
                        // non possono esistere due righe di dettaglio con tale stringa identica
                        var check_riga = NomeFile + "-" + codart + "-" + qtà.ToString() + "-" + pr_unitario.ToString();
                        // se la riga dettaglio non è già presente nel dizionario delle righee dettaglio (non è presente il codice univoco di ogni riga dettaglio IdRigaDettaglio)
                        // crea un nuovo oggetto di tipo RigaDettaglio lo aggiunge al dizionario delle righe di dettaglio ed alla tabella RigheDettaglio del database DBFatture
                        if (!righedettaglioDict.ContainsKey(check_riga))
                        {
                            // legge gli altri dati della riga di dettaglio
                            var aliquota = linea.AliquotaIVA;
                            var um = linea.UnitaMisura ?? " ";
                            var pr_imponibile = linea.PrezzoTotale;
                            var imposta = (aliquota * pr_imponibile / 100);
                            var prezzo = (linea.PrezzoTotale + imposta);
                            // genera il codice univoco IdRigaDettaglio che è la chiave della Tabella RigheDettaglio del database DBFatture
                            // IdRigaDettaglio = Anno (4 cifre) - mese (2 cifre) - giorno - numero progressivo fatturaper anno (5 cifre) - numero progressivo riga dettaglio nella fattura (3 cifre)
                            // es. "2024-05-00137-003" --> 3° riga di dettaglio della fattura n° 7 del mese di Maggio 2024
                            var IdRigaDettaglio = IdFattura + "-" + n_linea.ToString().PadLeft(3, '0');
                            // crea l'oggetto RigaDettaglio
                            var rd = new RigaDettaglio
                            {
                                IdRigaDettaglio = IdRigaDettaglio,
                                IdFattura = IdFattura,
                                CodiceArticolo = codart.ToUpper(),
                                DescrizioneArticolo = desc.ToUpper(),
                                UnitàMisura = um.ToUpper(),
                                Quantità = qtà,
                                PrezzoUnitario = pr_unitario,
                                Imponibile = pr_imponibile,
                                AliquotaIva = Convert.ToInt32(aliquota),
                                Imposta = imposta,
                                PrezzoTotale = prezzo,
                                IdFatturaNavigation = fattureDict[NomeFile]
                            };
                            res = righedettaglioDict.TryAdd(check_riga, rd);
                            if (!res)
                            {
                                MessageText = "AGGDB - ERRORE 05 - La riga di dettaglio codice " + check_riga + " è già presente nel Database";
                                return false;
                            }

                            // aggiunge la nuova riga di dettaglio alla Tabella RigheDettaglio del database DBFatture
                            context.RigheDettaglio.Add(rd);
                            context.SaveChanges();

                            // aggiunge la nuova riga di dettaglio nella ObservableCollection RigheDettaglioFatturaOC dell'oggetto Fattura corrente, trovata inserendo  
                            // la stringa check_fattura come chiave nel dizionario delle fatture
                            fattureDict[NomeFile].RigheDettaglioFatturaOC.Add(rd);

                            // incrementa il numero progressivo delle righe di dettaglio della fattura 
                            n_linea++;
                        }
                    }
                }
            }
        }
        // alla fine salva i cambiamenti nel database DBFatture
        context.SaveChanges();
        // context.Dispose();
        MessageText = "AGGDB 02 - Inserite nuove fatture, Database AGGIORNATO ";
        return true;
    }
    public static bool DbExists(string databaseName)
    {
        var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Master;Integrated Security=True";
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand("SELECT db_id(@databaseName)", connection);
        command.Parameters.Add(new SqlParameter("databaseName", databaseName));
        connection.Open();
        var res = (command.ExecuteScalar() != DBNull.Value);
        connection.Close();
        return res;
    }
    public void ShareDbData(DbFattureContext dbcontext, ISharedDataStore sharedData)
    {
        _sharedData = sharedData;
        _sharedData.ClientiOC = dbcontext.Clienti.OrderBy(x => x.Denominazione).ToObservableCollection();
        _sharedData.FornitoriOC = dbcontext.Fornitori.OrderBy(x => x.Denominazione).ToObservableCollection();
        _sharedData.FattureOC = dbcontext.Fatture.OrderByDescending(x => x.IdFattura).ToObservableCollection();
        _sharedData.RigheDettaglioOC = dbcontext.RigheDettaglio.OrderByDescending(x => x.IdRigaDettaglio).ToObservableCollection();
        _sharedData.FattureInputDirectory = fattureInputDir;
    }
}
