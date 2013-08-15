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
role____Ledgers= ledgers;
        }

        public decimal Balance
        {
            get
            {
                return self__Ledgers__GetBalance();
            }
        }

        public void IncreaseBalance(decimal amount)
        {
self__Ledgers__AddEntry("depositing",amount);
        }

        public void DecreaseBalance(decimal amount)
        {
self__Ledgers__AddEntry("withdrawing",0 - amount);
        }
private dynamic role____Ledgers;            decimal  self__Ledgers__GetBalance()
            {
                return ((ICollection<LedgerEntry>)role____Ledgers).Sum(e => e.Amount);
            }
            void  self__Ledgers__AddEntry(string message, decimal amount)
            {
role____Ledgers.Add(new LedgerEntry(message, amount));
            }
    }
}
