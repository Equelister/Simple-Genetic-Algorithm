using System;
using System.Collections.Generic;

namespace SimpleGeneticAlgorithm
{
    class Program
    {
        static SortedDictionary<int, int> histogram = new SortedDictionary<int, int>();
        public static string date;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.UserInput();

            date = DateTime.Now.ToString("yyyy-MM-dd---HH-mm-ss");

            for (int i = 0; i < GlobalClass.ile_wyn; i++)
            {
                if (GlobalClass.a == 0 && GlobalClass.b == 0 && GlobalClass.c == 0)
                {
                    ToFileClass.SaveToFile(0, 0, date);
                }
                else
                {
                    p.RunSGA();
                }
            }
        }

        private void RunSGA()
        {
            byte[] population = GeneratePopulation();

            for (int i = 1; i < GlobalClass.lb_pop; i++)
            {
                Crossing(population);// krzyzowanie
                Mutation(population);// mutacja
                double sum = 0;
                double[] functionResults = CalculateFunction(population, out sum);  // wyniki funkcji
                population = Selection(population, functionResults, sum);           // selekcja = nowa populacja
            }
            int bestID;
            double maxFunc = CalculateFunction(population, out bestID);
            ToFileClass.SaveToFile(population[bestID], maxFunc, date); 
        }

        private byte[] Selection(byte[] population, double[] functionResults, double sum)
        {
            List<byte> newPopulation = new List<byte>();

            double min = functionResults[0];
            foreach(double elem in functionResults)
            {
                if(min > elem)
                {
                    min = elem;     //Wyznaczenie najmniejszego wyniku funkcji
                }
            }

            if(min < 0)
            {
                min *= (-1);
                sum *= (-1);
                for(int i = 0; i<functionResults.Length; i++)       //Dodanie najmniejszego wyniku funkcji do kazdego elementu
                {
                    functionResults[i] += min;
                    sum -= min;
                }
            }

            double[] sections = CalcSections(functionResults, sum);
            Random rand = new Random();

            for(int i = 0; i<GlobalClass.ile_os; i++)
            {
                double randomNumber = rand.NextDouble();        //Losowanie wartosc [0;1)
                //bool flag = false;

                for(int j = 1; j<GlobalClass.ile_os; j++)
                {
                    if(randomNumber >= sections[j-1] && randomNumber < sections[j])
                    {
                        newPopulation.Add(population[j]);
                        break;
                    }
                    else if (randomNumber < sections[0])        //Jesli random < sec[0] => [0; fun[0]) => element o id 0
                    {
                        newPopulation.Add(population[0]);       
                        break;
                    }
                }
            }

            return newPopulation.ToArray();
        }

        private double[] CalcSections(double[] functionResults, double sum)
        {
            List<double> resultList = new List<double>();
            double result = 0;
            double previousSum = 0;

            foreach(double elem in functionResults)
            {
                result = elem / sum;            //Wyliczenie udzialu wyniku funkcji w sumie
                previousSum += result;          //Dodanie wszystkich poprzednich wynikow, by utworzyc przedzial od wyniku poprzedniego do obecnego
                resultList.Add(previousSum);    //Np. previousSum = 0.1, result = 0.555 => przedzial [0.1; 0.655]
            }

            if(resultList[2] < 0)               //Dla ujemnych, jesli losowa wartosc [2] bedzie na minusie, to znaczy ze usuwamy minusa z wszystkich przedzialow bo jest [-1;0] zamiast [0;1]
            {                                   //Tylko gdy suma jest ujemna
                for(int i = 0; i< resultList.Count; i++)
                {
                    resultList[i] *= (-1);
                }
            }

            return resultList.ToArray();
        }

        private void Mutation(byte[] population)
        {
            Random rand = new Random();
            double random;
            byte mutagen = 1;

            for(int i = 0; i < population.Length; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    random = rand.NextDouble();
                    if(random <= GlobalClass.pr_mut)
                    {
                        byte temp = (byte)(population[i] & ~mutagen);       //AND z przesuwajacym sie zerem => 11111110 -> 11111101 -> 11111011...
                        if(temp == population[i])       //Jesli FALSE => zmieniany znak byl '1' i zmienil sie na '0', pooniewaz 1*0 = 0
                        {                               //Jesli TRUE => zmieniany znak byl '0', nie zmienil sie, poniewaz 0*0 = 0, wiec
                            temp += mutagen;            //              do danego miejsca wstawiamy '1', np 110'0'1110 + 000'1'0000 = 11011110
                        }
                        population[i] = temp;
                    }
                    mutagen += mutagen; //Potegowanie '2' / przesuwanie '1' w lewo
                }
                mutagen = 1;
            }
        }

        private void Crossing(byte[] population)
        {
            Random rand = new Random();
            List<byte> IDList = new List<byte>();   //Lista do sprawdzania czy element się już nie łączył

            for(int i = 0; i < population.Length/2; i++)
            {
                byte firstID = (byte)rand.Next(0, population.Length);
                for(int j = 0; j < IDList.Count; j++)
                {
                    if (firstID == IDList[j])
                    {
                        firstID = (byte)rand.Next(0, population.Length);        //Sprawdzenie czy element się łączył czy jeszcze nie
                        j = -1;                                                 //Jesli byl to losowanie nowego id i reset petli (j=-1)
                    }
                }
                IDList.Add(firstID);

                byte secondID = (byte)rand.Next(0, population.Length);
                for (int j = 0; j < IDList.Count; j++)
                {
                    if (secondID == IDList[j])
                    {
                        secondID = (byte)rand.Next(0, population.Length);       //Sprawdzenie czy element się łączył czy jeszcze nie
                        j = -1;
                    }
                }
                IDList.Add(secondID);

                double w_lb_psl = rand.NextDouble();
                if(w_lb_psl < GlobalClass.pr_krzyz)
                {
                    byte pc = (byte)rand.Next(1, 8);    //Punkt przeciecia [1-7]

                    byte poweredNumber = (byte)(Math.Pow(2, (8 - pc)) - 1);    //Tworzy '1' od prawej strony => 3 = 00000011
                    byte NEGpoweredNumber = (byte)~poweredNumber;                   //(Neguje)Tworzy '1' od lewej strony => 252 = 11111100
                                                                                    //ByteMax = 255 = 7 + 248

                    byte temp10 = (byte)(population[firstID] & NEGpoweredNumber);   //AND z lewej elementu [1] strony: 01110100 & 11111100 = '011101'00
                    byte temp11 = (byte)(population[secondID] & poweredNumber);     //AND z prawej elementu [2] strony: 10110000 & 00000011 = 000000'00'

                    byte temp20 = (byte)(population[firstID] & poweredNumber);      //AND z prawej elementu [1] strony: 01110100 & 00000011 = 000000'00'
                    byte temp21 = (byte)(population[secondID] & NEGpoweredNumber);  //AND z lewej elementu [2] strony: 10110000 & 11111100 = '101100'00

                    byte a = (byte)(temp10 | temp11);   //Łączy Lewa strone El[1] z prawa strona El[2]: '011101'00 | 000000'00' = '01110100'
                    byte b = (byte)(temp20 | temp21);

                    population[firstID] = a;
                    population[secondID] = b;
                }            
            }
        }

        private double CalculateFunction(byte[] population, out int bestID)     //Wyliczenie w celu znalezienia najlepszego elementu
        {
            bestID = 0;
            double result = 0;
            double max = 0;

            for (int i = 0; i < population.Length; i++)
            {
                result = GlobalClass.a * Math.Pow(population[i], 2) + GlobalClass.b * population[i] + GlobalClass.c;        //a*x^2 + b*x + c 
                if (i == 0)
                {
                    max = result;
                    bestID = 0;
                }
                else if (result > max)
                {
                    max = result;
                    bestID = i;
                }
            }

            return max;
        }

        private double[] CalculateFunction(byte[] population, out double sum)       //Wyliczenie do utworzenia sekcji
        {
            List<double> functionResults = new List<double>();
            double result = 0;
            sum = 0;

            for (int i = 0; i < population.Length; i++)
            {
                result = GlobalClass.a * Math.Pow(population[i], 2) + GlobalClass.b * population[i] + GlobalClass.c;
                functionResults.Add(result);
                sum += result;
            }

            return functionResults.ToArray();
        }

        private byte[] GeneratePopulation()
        {
            List<Byte> list = new List<byte>();
            Random rand = new Random();

            for(int i = 0; i < GlobalClass.ile_os; i++)
            {
                list.Add((byte)(rand.Next(0, 256)));        //Losowanie [0;256)
            }

            return list.ToArray();
        }

        private void UserInput()
        {
            int a, b, c, ile_wyn, lb_pop, ile_os;
            double pr_krzyz, pr_mut;

            do {
                Console.Write("\r\nPodaj współczynnik 'a' równania kwadratowego: ");
            } while(Int32.TryParse(Console.ReadLine(), out a) == false);
            do
            {
                Console.Write("\r\nPodaj współczynnik 'b' równania kwadratowego: ");
            } while (Int32.TryParse(Console.ReadLine(), out b) == false);
            do
            {
                Console.Write("\r\nPodaj współczynnik 'c' równania kwadratowego: ");
            } while (Int32.TryParse(Console.ReadLine(), out c) == false);

            do
            {
                do
                {
                    Console.Write("\r\nPodaj liczbę uruchomień programu: ");
                } while (Int32.TryParse(Console.ReadLine(), out ile_wyn) == false);
            } while (ile_wyn <= 0);

            do
            {
                do
                {
                    do
                    {
                        Console.Write("\r\nPodaj liczbę populacji (wliczajac wygenerowana) [x>1]: ");
                    } while (Int32.TryParse(Console.ReadLine(), out lb_pop) == false);
                } while (lb_pop <= 1);

                do
                {
                    do
                    {
                        Console.Write("\r\nPodaj liczbę osobników w populacji: ");
                    } while (Int32.TryParse(Console.ReadLine(), out ile_os) == false);
                } while (ile_os <= 0);
            } while ((ile_os * lb_pop) > 150);

            do
            {
                do
                {
                    Console.Write("\r\nPodaj prawdopodobieństwo krzyżowania: ");
                } while (Double.TryParse(Console.ReadLine(), out pr_krzyz) == false);
            } while ((pr_krzyz >= 0 && pr_krzyz <= 1) == false);

            do
            {
                do
                {
                    Console.Write("\r\nPodaj prawdopodobieństwo mutacji: ");
                } while (Double.TryParse(Console.ReadLine(), out pr_mut) == false);
            } while ((pr_mut >= 0 && pr_mut <= 1) == false);

            GlobalClass.a = a;
            GlobalClass.b = b;
            GlobalClass.c = c;
            GlobalClass.ile_wyn = ile_wyn;
            GlobalClass.lb_pop = lb_pop;
            GlobalClass.ile_os = ile_os;
            GlobalClass.pr_krzyz = pr_krzyz;
            GlobalClass.pr_mut = pr_mut;
        }
    }
}
