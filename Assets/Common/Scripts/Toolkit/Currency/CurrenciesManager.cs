using UnityEngine;

namespace OctoberStudio.Currency
{
    public class CurrenciesManager : MonoBehaviour
    {
        private static CurrenciesManager instance;

        [SerializeField] CurrenciesDatabase database;

        public void Init()
        {
            if(instance != null)
            {
                Destroy(this);

                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);
        }

        public Sprite GetIcon(string currencyId)
        {
            var data = database.GetCurrency(currencyId);

            if(data == null) return null;

            return data.Icon;
        }

        public string GetName(string currencyId)
        {
            var data = database.GetCurrency(currencyId);

            if (data == null) return null;

            return data.Name;
        }
    }
}