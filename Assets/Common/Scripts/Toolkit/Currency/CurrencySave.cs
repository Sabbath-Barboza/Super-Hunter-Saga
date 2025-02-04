using OctoberStudio.Save;
using System;
using UnityEngine;

namespace OctoberStudio
{
    public class CurrencySave: ISave
    {
        [SerializeField] int amount;
        public int Amount => amount;
        private static CurrencySave _instance;
        public static CurrencySave Instance
        {
            get
            {
                _instance ??= new CurrencySave();
                return _instance;
            }
        }

        public event Action<int> OnGoldAmountChanged;
     
        public void Deposit(int depositedAmount)
        {
            amount += depositedAmount;

            OnGoldAmountChanged?.Invoke(amount);
        }

        public void Withdraw(int withdrawnAmount)
        {
            amount -= withdrawnAmount;
            if (amount < 0) amount = 0;

            OnGoldAmountChanged?.Invoke(amount);
        }

        public bool TryWithdraw(int withdrawnAmount)
        {
            var canAfford = CanAfford(withdrawnAmount);

            if(canAfford) 
            {
                amount -= withdrawnAmount;

                OnGoldAmountChanged?.Invoke(amount);
            }

            return canAfford;
        }

        public bool CanAfford(int requiredAmount)
        {
            return amount >= requiredAmount;
        }

        public void Flush()
        {

        }
    }
}