using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Microsoft.Identity.Client;

namespace FatturaElettronica_PRISM_WPF_V2.Models;

public class SoggettoFiltro 
{
    public string Tipo { get; set; }
    public string Denominazione { get; set; }
    public int Frequenza { get; set; }
        
}
