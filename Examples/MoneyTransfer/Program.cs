using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marvin.Examples
{
    public class Program
    {

        public static void Main()
        {
            var source = new Account<List<LedgerEntry>>(new List<LedgerEntry> { new LedgerEntry("start", 0m), new LedgerEntry("first deposit", 1000m) });
            var destination = new Account<List<LedgerEntry>>(new List<LedgerEntry>());
            var context = new MoneyTransfer<List<LedgerEntry>, List<LedgerEntry>>(source, destination, 245m);
            context.Trans();
            Console.WriteLine("Press enter...");
            Console.ReadLine();
        }
    }
}
