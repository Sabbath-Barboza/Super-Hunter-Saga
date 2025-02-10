using OctoberStudio.Save;
using System;
using UnityEngine;

namespace OctoberStudio
{
    public class CurrencySave: ISave
    {
        [SerializeField] int amount;
        public int Amount => amount;

        public event Action<int> onGoldAmountChanged;

        public void Deposit(int depositedAmount)
        {
            amount += depositedAmount;

            onGoldAmountChanged?.Invoke(amount);
        }

        public void Withdraw(int withdrawnAmount)
        {
            amount -= withdrawnAmount;
            if (amount < 0) amount = 0;

            onGoldAmountChanged?.Invoke(amount);
        }

        public bool TryWithdraw(int withdrawnAmount)
        {
            var canAfford = CanAfford(withdrawnAmount);

            if(canAfford) 
            {
                amount -= withdrawnAmount;

                onGoldAmountChanged?.Invoke(amount);
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