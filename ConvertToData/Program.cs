using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ConvertToData
{
    class Program
    {
        Dictionary<string, int> clickBaitWords = new Dictionary<string, int>();
        Dictionary<string, int> nonClickBaitWords = new Dictionary<string, int>();

        int Clickbait = 0;
        int NonClickBait = 0;

        public string BaseUrl => @"C:\Users\martijn\Coding\IRNLPProject\fulldata\";

        static void Main(string[] args)
        {
            try
            {
                Program program = new Program();
                program.Run();
                program.WriteDictionaries();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadLine();
            }
        }

        class WeightedCombinedClickBaitWord : ClickBaitWord
        {
            public int amount { get; set; }
            public string key { get; set; }
        }

        class ClickBaitWord
        {
            public int clickbait { get; set; }
            public int nonclickbait { get; set; }
            public string key { get; set; }
        }

        private void WriteDictionaries()
        {
            StreamWriter clickBaitWordsWriter = new StreamWriter(BaseUrl + "clickBaitWords.csv");
            CsvWriter csvWriter = new CsvWriter(clickBaitWordsWriter);
            csvWriter.WriteRecords(clickBaitWords);
            csvWriter.NextRecord();
            clickBaitWordsWriter.Close();

            StreamWriter nonClickBaitWordsWriter = new StreamWriter(BaseUrl + "nonClickBaitWords.csv");
            CsvWriter csvWritern = new CsvWriter(nonClickBaitWordsWriter);
            csvWritern.WriteRecords(nonClickBaitWords);
            csvWritern.NextRecord();
            nonClickBaitWordsWriter.Close();

            StreamWriter combinedClickBaitWordsWriter = new StreamWriter(BaseUrl + "combinedClickBaitWord.csv");
            CsvWriter csvWriterC = new CsvWriter(combinedClickBaitWordsWriter);
            csvWriterC.WriteHeader(typeof(ClickBaitWord));
            csvWriterC.Flush();
            combinedClickBaitWordsWriter.WriteLine();


            StreamWriter weightedCombinedClickBaitWordsWriter = new StreamWriter(BaseUrl + "weightedCombinedClickBaitWordsWriter.csv");
            CsvWriter csvWriterCW = new CsvWriter(weightedCombinedClickBaitWordsWriter);
            csvWriterCW.WriteHeader(typeof(WeightedCombinedClickBaitWord));
            csvWriterCW.Flush();
            weightedCombinedClickBaitWordsWriter.WriteLine();

            foreach (var word in clickBaitWords.Keys)
            {
                var cboc = clickBaitWords[word]; //clickbait occurance
                var ncboc = 0;
                if(nonClickBaitWords.ContainsKey(word))
                {
                    ncboc = nonClickBaitWords[word];
                }

                csvWriterC.WriteRecord(new ClickBaitWord
                {
                    key = word,
                    clickbait = cboc,
                    nonclickbait = ncboc
                });
                csvWriterC.NextRecord();

                csvWriterCW.WriteRecord(new WeightedCombinedClickBaitWord
                {
                    key = word,
                    amount = (int) (((double)cboc)/Clickbait*NonClickBait) - ncboc,
                    clickbait = (int)(((double)cboc) / Clickbait * NonClickBait),
                    nonclickbait = ncboc
                });
                csvWriterCW.NextRecord();
            }

            combinedClickBaitWordsWriter.Close();
            weightedCombinedClickBaitWordsWriter.Close();

        }

        private void Run()
        {
            int counter = 0;
            string line;
            StreamReader instances = new StreamReader(BaseUrl + "instances.jsonl");
            StreamWriter csvWriter = new StreamWriter(BaseUrl + "training.csv");
            StreamReader truthreader = new StreamReader(BaseUrl + "truth.jsonl");
            var csv = new CsvWriter(csvWriter);
            csv.WriteHeader(typeof(CsvItem));
            csv.Flush();
            csvWriter.WriteLine();

            List<Item> items = new List<Item>();
            List<Truth> truths = new List<Truth>();


            while ((line = instances.ReadLine()) != null)
            {
                items.Add(JsonConvert.DeserializeObject<Item>(line));
                truths.Add(JsonConvert.DeserializeObject<Truth>(truthreader.ReadLine()));
                

                //if (t.Id != item.Id)
                //{
                //    throw new Exception($"No longer synchronous at {counter}, {t.Id}, {item.Id}");
                //}

                
            }
            csv.Flush();

            instances.Close();
            foreach (var item in items)
            {
                Truth t = truths.First(i => i.Id == item.Id);
                truths.Remove(t);


                csv.WriteRecord(Convert(item, t));
                csv.NextRecord();

                if (t.TruthMean > 0.5)
                {
                    Clickbait++;
                    AddWordTo(clickBaitWords, item.TargetTitle);
                }
                else
                {
                    NonClickBait++;
                    AddWordTo(nonClickBaitWords, item.TargetTitle);
                }

                if(counter % 500 == 0)
                {
                    csv.Flush();
                }

                counter++;
            }
            csvWriter.Close();
        }



        private static void AddWordTo(Dictionary<string, int> dict, string targetTitle)
        {
            foreach (var oldWord in targetTitle.Split(" "))
            {
                var word = new string(oldWord.ToLower().Where(c => !char.IsPunctuation(c)).ToArray());
                if (dict.ContainsKey(word))
                {
                    dict[word] = dict[word] + 1;
                }
                else
                {
                    dict.Add(word, 1);
                }
            }
        }

        static CsvItem Convert(Item item, Truth truth)
        {
            var wordsPerParagraph = item.TargetParagraphs.Select(i => i.Count(j => j == ' ') + 1).ToList();
            var charsPerParagraph = item.TargetParagraphs.Select(i => i.Count()).ToList();

            var res = new CsvItem
            {
                NumberOfQuestionMarksInTitle = item.TargetTitle.Count(i => i == '?'),
                TitleLength = item.TargetTitle.Count(),
                NumberOfWordsInTitle = item.TargetTitle.Count(i => i == ' ') + 1,
                NumberOfCharsInTitle = item.TargetTitle.Count(),
                NumberOfWordsInArticle = wordsPerParagraph.Sum(),
                NumberOfCharsInArticle = charsPerParagraph.Sum(),
                NumberOfParagraphs = item.TargetParagraphs.Count,
                NumberOfCharsInDescription = item.TargetDescription.Count(),
                NumberOfWordsInDescription = item.TargetDescription.Count(i => i == ' ') + 1,
                ClickBaitMeanRating = truth.TruthMean,
                ClickBaitMedianRating = truth.TruthMedian,
                HasImage = item.PostMedia.Count
            };

            res.AverageWordLengthTitle = (double) res.NumberOfCharsInTitle / res.NumberOfWordsInTitle;
            res.AverageWordLengthArticle = (double) res.NumberOfCharsInArticle / res.NumberOfWordsInArticle;
            res.AverageWordLengthDescription = (double) res.NumberOfCharsInDescription / res.NumberOfWordsInDescription;

            try
            {
                var hour = DateTime.ParseExact(item.PostTimestamp.Replace("+0000 ", ""), "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture).Hour;
                if (hour > 6 && hour <= 8)
                {
                    res.EarlyMorning = 1;
                }
                else if (hour > 8 && hour <= 12)
                {
                    res.Morning = 1;
                }
                else if (hour > 12 && hour <= 18)
                {
                    res.Afternoon = 1;
                }
                else if (hour > 18 && hour <= 21)
                {
                    res.Evening = 1;
                }
                else
                {
                    res.Night = 1;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Error at" + item.PostTimestamp); 
            }

            return res;
        }
    }
}
