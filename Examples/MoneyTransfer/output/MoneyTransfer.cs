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
role____Source= source;
role____Destination= destination;
role____Amount= amount;
        }

        public void Trans()
        {
self__Source__Transfer(role____Amount);
        }
private dynamic role____Source;private dynamic role____Destination;private dynamic role____Amount;            void  self__Source__Withdraw(decimal amount)
            {
role____Source.DecreaseBalance(amount);
            }
            void  self__Source__Transfer(decimal amount)
            {
                Console.WriteLine("Source balance is: " + role____Source.Balance);
                Console.WriteLine("Destination balance is: " + role____Destination.Balance);
self__Destination__Deposit(amount);
self__Source__Withdraw(amount);

                Console.WriteLine("Source balance is now: " + role____Source.Balance);
                Console.WriteLine("Destination balance is now: " + role____Destination.Balance);
            }
            void  self__Destination__Deposit(decimal amount)
            {
role____Destination.IncreaseBalance(amount);
            }
    }
}