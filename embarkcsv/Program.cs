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
            List<EmbarkSubject> subjects = new List<EmbarkSubject>();
            string[] fileEntries = Directory.GetFiles(args[0], "*.csv");
            int pf = 0;
            int fe = fileEntries.Length;
            Console.WriteLine($"\n\r+++ Running sanity check for Embark data set");
            Console.WriteLine($"{fe} Embark csv files found.");
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

            Console.WriteLine($"{pf} csv files parsed.");
            Console.WriteLine($"{subjects.Count} subjects deserialized.");

            // Sanity check Swab code
            Console.WriteLine($"{subjects.Count(a => a.HasSwabId())} subjects contain a swab code.");

            // Sanity check Gender
            Console.WriteLine($"{subjects.Count(a => a.HasSex())} subjects contain a gender value.");
            Console.WriteLine($"{subjects.Count(a => a.Sex() != null && a.Sex().ToLower() == "male")} subjects are male.");
            Console.WriteLine($"{subjects.Count(a => a.Sex() != null && a.Sex().ToLower() == "female")} subjects are female.");

            // Sanity check Paternal haplotype
            Console.WriteLine($"{subjects.Count(a => a.HasPaternalHaplotype())} subjects have a value for paternal haplotype.");

            // Sanity check Maternal haplotype
            Console.WriteLine($"{subjects.Count(a => a.HasMaternalHaplotype())} subjects have a value for maternal haplotype.");

            // Sanity check Paternal haplogroup
            Console.WriteLine($"{subjects.Count(a => a.HasPaternalHaplogroup())} subjects have a value for paternal haplogroup.");

            // Sanity check Maternal haplogroup
            Console.WriteLine($"{subjects.Count(a => a.HasMaternalHaplogroup())} subjects have a value for maternal haplogroup.");

            // Sanity check A Locus
            Console.WriteLine($"{subjects.Count(a => a.HasLocusA())} subjects have a value for A Locus.");

            // Run Hypothesis
            int isAtAt = 0;
            Console.WriteLine($"\n\r+++ Running analysis against {fe} subjects.");

            // Hypothesis A Locus
            foreach (var s in subjects.Where(a => a.HasLocusA()))
            {
                if (s.LocusA().ToLower() == "atat")
                {
                    isAtAt++;
                    s.X = s.X + 2;
                    //Console.WriteLine(s.LocusA());
                }
            }
            Console.WriteLine($"{isAtAt} subjects are AtAt at A Locus");
            Console.WriteLine($"\n\r+++ Running haplotype tests against the AtAt subset.");

            // Females
            var queryFemales = from s in subjects
                where s.LocusA().ToLower() == "atat" &&
                    s.Sex() != null &&
                    s.Sex().ToLower() == "female"
                select s;

            Console.WriteLine($"\n\r++ {queryFemales.ToArray().Length} females are AtAt at A Locus");
            var groupFemales = from gf in queryFemales
                               group gf by gf.HaplotypeToTest() into newGroup
                               orderby newGroup.Key
                               select newGroup;

            Console.WriteLine($"Haplotypes of females who are AtAt at A Locus");
            foreach (var s in groupFemales.ToList())
            {
                Console.WriteLine($"{s.Count()} x {s.Key}");
            }
            
            // Score females for AtAt && PTA control variable
            foreach (var s in queryFemales.ToList())
            {
                // For MTA control variable
                if (s.HaplotypeToTest() == "A1a_MT A388_MT" ||
                   s.HaplotypeToTest() == "A1d_MT A247_MT" ||
                   s.HaplotypeToTest() == "C2_MT C42/54/55_MT")
                {
                    s.X = s.X + 2;
                }
            }

            // Males
            var queryMales = from s in subjects
                               where s.LocusA().ToLower() == "atat" &&
                                   s.Sex() != null &&
                                   s.Sex().ToLower() == "male"
                               select s;

            Console.WriteLine($"\n\r++ {queryMales.ToArray().Length} males are AtAt at A Locus");
            var groupMales = from gf in queryMales
                               group gf by gf.HaplotypeToTest() into newGroup
                               orderby newGroup.Key
                               select newGroup;

            Console.WriteLine($"Haplotypes of males who are AtAt at A Locus");
            foreach (var s in groupMales.ToList())
            {
                Console.WriteLine($"{s.Count()} x {s.Key}");
            }

            // Score males for AtAt && PTA control variable
            foreach (var s in queryMales.ToList())
            {
                // For PTA control variable
                if (s.HaplotypeToTest() == "A1a_Y H1a.29_Y" ||
                   s.HaplotypeToTest() == "A1a_Y H1a.48_Y")
                {
                    s.X = s.X + 2;
                }
            }

            Console.WriteLine($"\n\r==== Ratings for {subjects.Count(a => a.HasSwabId())} subjects");
            var groupScores = from gs in subjects
                             group gs by gs.X into newGroup
                             orderby newGroup.Key
                             select newGroup;

            Console.WriteLine($"Based on genotype alone, {subjects.Where(a => a.X > 0).Count()} subjects are on the continuum");

            foreach (var s in groupScores.ToList())
            {
                Console.WriteLine($"ESBT-{s.Key} x {s.Count()}");
            }
        }
    }
}
