﻿
namespace Domain.MetaData
{
   public class ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod
    {
        public string EntityPath { get; set; }
        public int CodeLines { get; set; }
        public int Revisions { get; set; }
        public string Authors { get; set; }
        public bool WasModifiedInSpecificPeriod { get; set; }
    }
}