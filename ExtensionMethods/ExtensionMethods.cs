using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatturaElettronica_PRISM_WPF_V2.ExtensionMethods;

public static class ExtensionMethods
{

    // ToObservableCollection<T> -> input = IEnumerable<T>, output = ObservableCollection<T>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerableList)
    {
        if (enumerableList != null)
        {   //create an observable collection object
            //you can pass the IEnumerable inside the ObservableCollection Constructor
            return new ObservableCollection<T>(enumerableList);
        }
        return null;
    }
}
