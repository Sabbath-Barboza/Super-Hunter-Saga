using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Currency
{
    [CreateAssetMenu(fileName = "Currencies Database", menuName = "October/Currencies Database")]
    public class CurrenciesDatabase : ScriptableObject
    {
        [SerializeField] List<CurrencyData> currencies;

        public CurrencyData GetCurrency(string id)
        {
            for(int i = 0; i < currencies.Count; i++)
            {
                if (currencies[i].ID == id) return currencies[i];
            }

            return null;
        }
    }
}