using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marvin.Examples
{
    public class Account
    {
        public Account(ICollection<LedgerEntry> ledgers)
        {
            Ledgers = ledgers;
        }
        [Role]
        private class Ledgers
        {
            decimal GetBalance()
            {
                return ((ICollection<LedgerEntry>)this).Sum(e => e.Amount);
            }
            void AddEntry(string message, decimal amount)
            {
                this.Add(new LedgerEntry(message, amount));
            }

        }

        public decimal Balance
        {
            get
            {
                return Ledgers.GetBalance();
            }
        }

        public void IncreaseBalance(decimal amount)
        {
            Ledgers.AddEntry("depositing", amount);
        }

        public void DecreaseBalance(decimal amount)
        {
            Ledgers.AddEntry("withdrawing", 0 - amount);
        }
    }
}
