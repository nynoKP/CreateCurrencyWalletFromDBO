using Camunda.Worker.Variables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions
{
    static class CustomExt
    {
        public static T? GetValue<T>(this VariableBase? variable)
        {
            if (variable == null) return default(T);
            try
            {
                return (T)((dynamic)variable).Value;
            }
            catch
            {
                return default(T);
            }
        }
        
        public static T? GetValue<T>(this IReadOnlyDictionary<string, VariableBase> ? variables, string key)
        {
            try
            {
                var variable = variables[key];
                return variable.GetValue<T>();
            }
            catch
            {
                return default(T);
            }
            
        }
    }
}
