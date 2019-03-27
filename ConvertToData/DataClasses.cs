using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertToData
{
    class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("postTimestamp")]
        public string PostTimestamp { get; set; }

        [JsonProperty("postText")]
        public List<string> PostText { get; set; }

        [JsonProperty("postMedia")]
        public List<string> PostMedia { get; set; }

        [JsonProperty("targetTitle")]
        public string TargetTitle { get; set; }

        [JsonProperty("targetDescription")]
        public string TargetDescription { get; set; }

        [JsonProperty("targetKeywords")]
        public string TargetKeywords { get; set; }

        [JsonProperty("targetParagraphs")]
        public List<string> TargetParagraphs { get; set; }

        [JsonProperty("targetCaptions")]
        public List<string> TargetCaptions { get; set; }
    }

    public class Truth
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("truthJudgments")]
        public List<double> TruthJudgments { get; set; }

        [JsonProperty("truthMean")]
        public double TruthMean { get; set; }

        [JsonProperty("truthMedian")]
        public long TruthMedian { get; set; }

        [JsonProperty("truthMode")]
        public long TruthMode { get; set; }

        [JsonProperty("truthClass")]
        public string TruthClass { get; set; }
    }

    class CsvItem
    {
        // Title
        public int TitleLength { get; set; }
        public int NumberOfWordsInTitle { get; set; }
        public int NumberOfCharsInTitle { get; set; }
        public double AverageWordLengthTitle { get; internal set; }
        public int NumberOfQuestionMarksInTitle { get; set; }

        //Article
        public int NumberOfWordsInArticle { get; set; }
        public int NumberOfCharsInArticle { get; set; }
        public double AverageWordLengthArticle { get; internal set; }
        public int NumberOfParagraphs { get; internal set; }


        //Description
        public int NumberOfWordsInDescription { get; set; }
        public int NumberOfCharsInDescription { get; set; }
        public double AverageWordLengthDescription { get; internal set; }

        //Image
        public int HasImage { get; internal set; }


        //Time of day posted
        public int EarlyMorning { get; set; } //6-8
        public int Morning { get; set; } //8-12
        public int Afternoon { get; set; } //12-18
        public int Evening { get; set; } //18-21
        public int Night { get; set; } //21-6
        public DateTime Time { get; set; }

        //Result
        public double ClickBaitMeanRating { get; set; }
        public double ClickBaitMedianRating { get; set; }
    }

}
