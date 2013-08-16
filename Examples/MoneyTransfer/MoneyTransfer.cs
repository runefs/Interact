using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marvin.Examples
{
    [Context]
    public class MoneyTransfer<TSource, TDestination>
        where TSource : ICollection<LedgerEntry>
        where TDestination : ICollection<LedgerEntry>
    {
        public MoneyTransfer(Account<TSource> source, Account<TDestination> destination, decimal amount)
        {
            Source = source;
            Destination = destination;
            Amount = amount;
        }

        [Role]
        private class Source : Account<TSource>
        {
            void Withdraw(decimal amount)
            {
                this.DecreaseBalance(amount);
            }
            void Transfer(decimal amount)
            {
                Console.WriteLine("Source balance is: " + this.Balance);
                Console.WriteLine("Destination balance is: " + Destination.Balance);

                Destination.Deposit(amount);
                this.Withdraw(amount);

                Console.WriteLine("Source balance is now: " + this.Balance);
                Console.WriteLine("Destination balance is now: " + Destination.Balance);
            }
        }

        [Role]
        private class Destination : Account<TDestination>
        {
            void Deposit(decimal amount)
            {
                this.IncreaseBalance(amount);
            }
        }

        [Role]
        public class Amount { }

        public void Trans()
        {
            Source.Transfer(Amount);
        }
    }
}