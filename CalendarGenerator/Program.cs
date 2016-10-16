﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("fullschedule.txt"))
            {
                Console.WriteLine("Le fichier fullschedule.txt est manquant. Il doit être dans le même répertoire que CalendarGenerator.exe");
                Console.Read();
                return;
            }

            Console.WriteLine("****GENERATEUR DE CALENDRIER DE LA NCHL****");
            Console.WriteLine("Avant de commencer, assurez-vous que le format du fichier fullschedule.txt est similaire a celui-ci. Sinon, le programme doit être modifié.");
            Console.WriteLine("Wednesday, Oct. 12");
            Console.WriteLine("Toronto at Ottawa, 7:00 p.m.");
            Console.WriteLine("St. Louis at Chicago, 8:00 p.m.");
            Console.WriteLine("Thursday, Oct. 13");
            Console.WriteLine("Montreal at Buffalo, 7:00 p.m.");
            Console.WriteLine("");
            Console.WriteLine("Entrer la date du premier match de la saison:");

            // Receive user input for the start date of the season. Essential to
            // know for the league's weeks
            Console.Write("Jour (1-31): ");
            int day = Convert.ToInt32(Console.ReadLine());
            Console.Write("Mois (1-12): ");
            int month = Convert.ToInt32(Console.ReadLine());
            Console.Write("An (2000-2099): ");
            int year = Convert.ToInt32(Console.ReadLine());

            DateTime startDate = new DateTime(year, month, day, new System.Globalization.GregorianCalendar());
            Calendar cal = new Calendar();
            cal.CreateMatchups(startDate);

            cal.OutputSeasonCalendar();
            cal.OutputWeekCalendar();
        }
    }
}