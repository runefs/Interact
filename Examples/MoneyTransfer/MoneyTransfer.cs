using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marvin.Examples
{
    public class MoneyTransfer
    {
        public MoneyTransfer(Account source, Account destination, decimal amount)
        {
            Source = source;
            Destination = destination;
            Amount = amount;
        }

        [Role]
        private class Source
        {
            void Withdraw(decimal amount)
            {
                Source.DecreaseBalance(amount);
            }
            void Transfer(decimal amount)
            {
                Console.WriteLine("Source balance is: " + Source.Balance);
                Console.WriteLine("Destination balance is: " + Destination.Balance);

                Destination.Deposit(amount);
                Source.Withdraw(amount);

                Console.WriteLine("Source balance is now: " + Source.Balance);
                Console.WriteLine("Destination balance is now: " + Destination.Balance);
            }
        }

        [Role]
        private class Destination
        {
            void Deposit(decimal amount)
            {
                Destination.IncreaseBalance(amount);
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