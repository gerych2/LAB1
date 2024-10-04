using System;
using System.Collections.Generic;
using System.IO;

namespace GeneticsProject
{
    public struct GeneticInfo
    {
        public string ProteinName; // название белка
        public string SourceOrganism; // организм, из которого белок
        public string SequenceFormula; // формула белка
    }

    class Program
    {
        static List<GeneticInfo> proteinData = new List<GeneticInfo>(); // коллекция белков
        //static int entryCounter = 0;

        // Метод поиска формулы белка по имени
        static string RetrieveFormula(string protein)
        {
            foreach (var entry in proteinData)
            {
                if (entry.ProteinName == protein) return entry.SequenceFormula;
            }
            return string.Empty;
        }

        // Метод для загрузки данных о белках из файла
        static void LoadProteinData(string filepath)
        {
            using var reader = new StreamReader(filepath);
            
                string content;
                while ((content = reader.ReadLine()) != null)
                {
                    var parts = content.Split('\t');
                    GeneticInfo info = new GeneticInfo
                    {
                        ProteinName = parts[0],
                        SourceOrganism = parts[1],
                        SequenceFormula = parts[2]
                    };
                    proteinData.Add(info);
                    //entryCounter++;
                }
        }

        // Метод для обработки команд из файла
        static void ProcessCommands(string filepath, StreamWriter writer)
        {
            using (var reader = new StreamReader(filepath))
            {
                string command;
                int commandNumber = 0;

                while ((command = reader.ReadLine()) != null)
                {
                    commandNumber++;
                    var args = command.Split('\t');
                    switch (args[0])
                    {
                        case "search":
                            writer.WriteLine($"{commandNumber:D3} search {DecodeSequence(args[1])}");
                            writer.WriteLine($"organism                protein");
                            int foundIndex = LocateProtein(args[1]);
                            if (foundIndex >= 0)
                                writer.WriteLine($"{proteinData[foundIndex].SourceOrganism}    {proteinData[foundIndex].ProteinName}");
                            else
                                writer.WriteLine("NOT FOUND");
                            writer.WriteLine("================================================");
                            break;

                        case "diff":
                            writer.WriteLine($"{commandNumber:D3} diff {args[1]} {args[2]}");
                            int diffResult = CompareProteins(args[1], args[2]);
                            writer.WriteLine(diffResult == -1 ? "MISSING" : $"amino-acids difference: {diffResult}");
                            writer.WriteLine("================================================");
                            break;

                        case "mode":
                            writer.WriteLine($"{commandNumber:D3} mode {args[1]}");
                            writer.WriteLine("amino-acid occurs:");
                            AnalyzeMode(args[1], writer);
                            writer.WriteLine("================================================");
                            break;

                        default:
                            writer.WriteLine($"{commandNumber:D3} UNKNOWN COMMAND");
                            writer.WriteLine("================================================");
                            break;
                    }
                }
            }
        }

        // Декодирование формулы белка
        static string DecodeSequence(string formula)
        {
            var decodedFormula = new System.Text.StringBuilder();

            for (int i = 0; i < formula.Length; i++)
            {
                if (char.IsDigit(formula[i]))
                {
                    char aminoAcid = formula[i + 1];
                    int repeatCount = formula[i] - '0';

                    decodedFormula.Append(new string(aminoAcid, repeatCount));
                    i++;
                }
                else
                {
                    decodedFormula.Append(formula[i]);
                }
            }
            return decodedFormula.ToString();
        }

        // Поиск белка по его аминокислотной последовательности
        static int LocateProtein(string aminoSequence)
        {
            string expandedSequence = DecodeSequence(aminoSequence);
            return proteinData.FindIndex(entry => entry.SequenceFormula.Contains(expandedSequence));
        }

        // Сравнение двух белков по количеству различий
        static int CompareProteins(string protein1, string protein2)
        {
            string formula1 = DecodeSequence(RetrieveFormula(protein1));
            string formula2 = DecodeSequence(RetrieveFormula(protein2));

            if (string.IsNullOrEmpty(formula1) || string.IsNullOrEmpty(formula2)) return -1;

            int minLen = Math.Min(formula1.Length, formula2.Length);
            int diffCount = 0;

            for (int i = 0; i < minLen; i++)
            {
                if (formula1[i] != formula2[i]) diffCount++;
            }

            return diffCount + Math.Abs(formula1.Length - formula2.Length);
        }

        // Метод для нахождения самой частой аминокислоты
        static void AnalyzeMode(string proteinName, StreamWriter writer)
        {
            string proteinFormula = RetrieveFormula(proteinName);
            if (string.IsNullOrEmpty(proteinFormula))
            {
                writer.WriteLine($"MISSING: {proteinName}");
                return;
            }

            Dictionary<char, int> frequency = new Dictionary<char, int>();

            foreach (char aminoAcid in proteinFormula)
            {
                if (!frequency.ContainsKey(aminoAcid))
                {
                    frequency[aminoAcid] = 0;
                }
                frequency[aminoAcid]++;
            }

            char mostCommon = '\0';
            int maxCount = 0;

            foreach (var pair in frequency)
            {
                if (pair.Value > maxCount || (pair.Value == maxCount && pair.Key < mostCommon))
                {
                    mostCommon = pair.Key;
                    maxCount = pair.Value;
                }
            }

            writer.WriteLine($"{mostCommon} {maxCount}");
        }

        static void Main(string[] args)
        {
            string resultFile = "genedata.txt";

            using (var writer = new StreamWriter(resultFile))
            {
                writer.WriteLine("Trosko German");
                writer.WriteLine("================================================");

                LoadProteinData("sequences.2.txt");

                ProcessCommands("commands.2.txt", writer);
            }

            Console.WriteLine("Operations completed. Results saved to genedata.txt");
        }
    }
}
