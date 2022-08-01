using EmbarkCsv.Model;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using EmbarkCsv;

namespace EmbarkCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            // Control variables
            double cv_G = 75;
            string[] cv_MTA = new string[] { "A1a_MT A388_MT", "A1d_MT A247_MT", "C2_MT C42/54/55_MT" };
            string[] cv_PTA = new string[] { "A1a_Y H1a.29_Y", "A1a_Y H1a.48_Y" };

            List<EmbarkSubject> subjects = new List<EmbarkSubject>();
            string[] fileEntries = Directory.GetFiles(args[0], "*.csv");
            int pf = 0;
            int fe = fileEntries.Length;
            Console.WriteLine($"\n\r+++ Running sanity check for Embark data set");
            Console.Write($"{fe} Embark csv files found, ");
            foreach (var fname in fileEntries)
            {
                List<EmbarkLineItem> data = new List<EmbarkLineItem>();
                using (TextFieldParser parser = new TextFieldParser(fname))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    if (parser == null)
                        continue;
                    pf++;
                    string[]? fields = null;

                    while (!parser.EndOfData)
                    {
                        //Processing row
                        fields = parser.ReadFields();
                        data.Add(new EmbarkLineItem()
                        {
                            Category = fields[0],
                            Name = fields[1],
                            Value = fields[2]
                        });
                    }

                    if (fields.Length != 3)
                    {
                        Console.WriteLine($"Skipping {fname}.  Exctly 3 fields should be found.");
                        continue;
                    }

                }

                subjects.Add(new EmbarkSubject(data));
            }

            Console.Write($"{pf} csv files parsed, ");
            Console.WriteLine($"{subjects.Count} subjects deserialized.");

            // Sanity check Swab code
            Console.WriteLine($"{subjects.Count(a => a.HasSwabId())} subjects contain a swab code.");

            // Sanity english shepherd
            Console.WriteLine($"{subjects.Count(a => a.HasEnglishShepherd())} subjects contain English Shepherd.");
            foreach (var subject in subjects)
            {
                if (!subject.HasEnglishShepherd())
                    subject.Disqualified = true;
            }

            // Sanity check Gender
            Console.Write($"{subjects.Count(a => a.HasSex())} subjects contain a sex value, ");
            Console.Write($"{subjects.Count(a => a.Sex() != null && a.Sex().ToLower() == "male")} subjects are male, ");
            Console.WriteLine($"{subjects.Count(a => a.Sex() != null && a.Sex().ToLower() == "female")} subjects are female.");

            // Sanity check Paternal haplotype
            Console.Write($"{subjects.Count(a => a.HasPaternalHaplotype())} subjects have a value for paternal haplotype, ");

            // Sanity check Paternal haplogroup
            Console.WriteLine($"{subjects.Count(a => a.HasPaternalHaplogroup())} subjects have a value for paternal haplogroup.");

            // Sanity check Maternal haplotype
            Console.Write($"{subjects.Count(a => a.HasMaternalHaplotype())} subjects have a value for maternal haplotype, ");

            // Sanity check Maternal haplogroup
            Console.WriteLine($"{subjects.Count(a => a.HasMaternalHaplogroup())} subjects have a value for maternal haplogroup.");

            // Sanity check A Locus
            Console.WriteLine($"{subjects.Count(a => a.HasLocusA())} subjects have a value for A Locus.");

            // Run Hypothesis
            int isAtAtEs = 0;
            Console.WriteLine($"\n\r+++ Running analysis against {subjects.Count()} subjects.\r\n");
            Console.WriteLine("++ Control Variables for this run are:");
            Console.WriteLine($"G = {cv_G}%");
            string pta = string.Join(',', cv_PTA.Select(x => $"'{x}'"));
            Console.WriteLine($"PTA = {pta}");
            string mta = string.Join(',', cv_MTA.Select(x => $"'{x}'"));
            Console.WriteLine($"MTA = {mta}");

            // Hypothesis A Locus and ES > G
            foreach (var s in subjects.Where(a => a.HasLocusA()))
            {
                if (s.LocusA().ToLower() == "atat")
                {
                    if (s.EnglishShepherdComposition() > cv_G)
                    {
                        s.X = s.X + 2;
                        isAtAtEs++;
                    }
                }
            }
            Console.WriteLine($"\n\r{isAtAtEs} subjects have English Shepherd composition > {cv_G}% AND are AtAt at A Locus");
            Console.WriteLine($"\n\r+++ Running haplotype tests against the ES AtAt subset.");

            // Males
            var queryMales = from s in subjects
                             where s.LocusA().ToLower() == "atat" &&
                                 s.EnglishShepherdComposition() > cv_G &&
                                 s.Sex() != null &&
                                 s.Sex().ToLower() == "male"
                             select s;



            Console.WriteLine($"\n\r++ {queryMales.ToArray().Length} ES males are AtAt at A Locus");
            var groupMales = from gf in queryMales
                             group gf by gf.HaplotypesToTest()[1] into newGroup
                             orderby newGroup.Key
                             select newGroup;

            Console.WriteLine($"Haplotypes of ES males who are AtAt at A Locus");
            foreach (var s in groupMales.ToList())
            {
                string val = cv_PTA.Contains(s.Key) ? "(X = X + 2)" : "";
                Console.WriteLine($"{s.Count()} x {s.Key} {val}");
            }

            // Build dynamic maternal haplotype list from maternal list control variable and males' maternal haplotypes
            List<string> dyn_MTA = new List<string>();
            dyn_MTA.AddRange(cv_MTA);

            // Score males for AtAt && PTA control variable
            foreach (var s in queryMales.ToList())
            {
                // For PTA control variable
                if (cv_PTA.Contains(s.HaplotypesToTest()[1]))
                {
                    s.X = s.X + 2;
                    dyn_MTA.Add(s.HaplotypesToTest()[0]);
                }
            }

            // Females
            dyn_MTA = dyn_MTA.Distinct().ToList();
            string dmta = string.Join(',', dyn_MTA.Select(x => $"'{x}'"));
            Console.WriteLine($"\r\nIn addition to the MTA control variable array, this program will use maternal haplotypes of included males to score females: {dmta}");

            var queryFemales = from s in subjects
                where s.LocusA().ToLower() == "atat" &&
                    s.EnglishShepherdComposition() > cv_G &&
                    s.Sex() != null &&
                    s.Sex().ToLower() == "female"
                select s;

            Console.WriteLine($"\n\r++ {queryFemales.ToArray().Length} ES females are AtAt at A Locus");
            var groupFemales = from gf in queryFemales
                               group gf by gf.HaplotypesToTest()[0] into newGroup
                               orderby newGroup.Key
                               select newGroup;

            Console.WriteLine($"Haplotypes of ES females who are AtAt at A Locus");
            foreach (var s in groupFemales.ToList())
            {
                string val = dyn_MTA.Contains(s.Key) ? "(X = X + 2)" : "";
                Console.WriteLine($"{s.Count()} x {s.Key} {val}");
            }
            
            // Score females for AtAt && PTA control variable
            foreach (var s in queryFemales.ToList())
            {
                // For MTA control variable
                if (dyn_MTA.Contains(s.HaplotypesToTest()[0]))
                {
                    s.X = s.X + 2;
                }
            }

            // Final scores
            Console.WriteLine($"\n\r==== ESBT-X Ratings for {subjects.Count(a => a.HasSwabId())} subjects");
            var groupScores = from gs in subjects
                             group gs by gs.X into newGroup
                             orderby newGroup.Key
                             select newGroup;

            Console.WriteLine($"Based on genotype alone, {subjects.Where(a => a.X > 0).Count()} subjects are on the continuum");

            foreach (var s in groupScores.ToList())
            {
                Console.WriteLine($"ESBT-{s.Key} x {s.Count()}");
            }

            Console.Read();
        }
    }
}
