﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbarkCsv.Model
{
    public class EmbarkSubject
    {
        public bool Disqualified { get; set; }

        public IEnumerable<EmbarkLineItem> Tests { get; private set; }

        public int X { get; set; }

        public EmbarkSubject(IEnumerable<EmbarkLineItem> lineItems)
        {
            Tests = lineItems;
        }

        public bool HasSwabId()
        {
            return Tests.Any(a => a.Name.ToLower().Contains("swab code"));
        }

        #region Breed

        public bool HasEnglishShepherd()
        {
            if (Tests.Any(a => a.Name.ToLower().Contains("english shepherd")))
                return true;
            else
                return false;
        }

        public double EnglishShepherdComposition()
        {
            double result = 0;

            if (HasEnglishShepherd())
            {
                var raw = Tests.Where(a => a.Name.ToLower().Contains("english shepherd")).First();
                double.TryParse(raw.Value.Replace("%", ""), out result);
            }

            return result;
        }

        #endregion


        #region A Locus

        public bool HasLocusA()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("a locus") || a.Name.ToLower().Contains("patterning")).FirstOrDefault();

            return eli != null && !string.IsNullOrEmpty(eli.Value);
        }

        public string LocusA()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("a locus") || a.Name.ToLower().Contains("patterning")).First();
            if (eli == null)
                throw new NullReferenceException("Field 'a locus' not found");

            return eli.Value;
        }

        #endregion

        #region Maternal Haplogroup

        public bool HasMaternalHaplogroup()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("mt haplogroup")).FirstOrDefault();

            return eli != null && !string.IsNullOrEmpty(eli.Value);
        }

        public string MaternalHaplogroup()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("mt haplogroup")).First();
            if (eli == null)
                throw new NullReferenceException("Field 'mt haplogroup' not found");

            return eli.Value;
        }

        #endregion

        #region Maternal Haplotype

        public bool HasMaternalHaplotype()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("mt haplotype")).FirstOrDefault();

            return eli != null && !string.IsNullOrEmpty(eli.Value);
        }

        public string MaternalHaplotype()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("mt haplotype")).First();
            if (eli == null)
                throw new NullReferenceException("Field 'mt haplotype' not found");

            return eli.Value;
        }

        #endregion

        #region Paternal Haplogroup

        public bool HasPaternalHaplogroup()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("y haplogroup")).FirstOrDefault();

            return eli != null && !string.IsNullOrEmpty(eli.Value);
        }

        public string PaternalHaplogroup()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("y haplogroup")).First();
            if (eli == null)
                throw new NullReferenceException("Field 'y haplogroup' not found");

            return eli.Value;
        }

        #endregion

        #region Paternal Haplotype

        public bool HasPaternalHaplotype()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("y haplotype")).FirstOrDefault();

            return eli != null && !string.IsNullOrEmpty(eli.Value);
        }

        public string PaternalHaplotype()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Contains("y haplotype")).First();
            if (eli == null)
                throw new NullReferenceException("Field 'y haplotype' not found");

            return eli.Value;
        }

        #endregion

        #region Sex

        public bool HasSex()
        {
            return Tests.Any(a => a.Name.ToLower().Equals("sex"));
        }

        public string Sex()
        {
            EmbarkLineItem eli = Tests.Where(a => a.Name.ToLower().Equals("sex")).First();
            if (eli == null)
                throw new NullReferenceException("Field 'sex' not found");

            return eli.Value;
        }

        #endregion

        public string[] HaplotypesToTest()
        {
            List<string> retval = new List<string>();
            retval.Add($"{this.MaternalHaplogroup()} {this.MaternalHaplotype()}");

            if (this.HasSex())
            {
                if (this.Sex().ToLower() == "male")
                {
                    retval.Add($"{this.PaternalHaplogroup()} {this.PaternalHaplotype()}");
                }
            }

            return retval.ToArray();
        }
    }
}
