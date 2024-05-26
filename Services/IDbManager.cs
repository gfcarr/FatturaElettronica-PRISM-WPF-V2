using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatturaElettronica_PRISM_WPF_V2.Services;

public interface IDbManager
{
    DbFattureContext DBcontext {  get; }
    DbFattureContext ReadDatabase();

    void UpdateDatabase(DbFattureContext dbcontext, ISharedDataStore sharedData);

    void ShareDatabase(DbFattureContext dbcontext, ISharedDataStore sharedData);
}
