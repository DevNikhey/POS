using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace EmployeeDirectory
{
    public static class CsvData
    {
        public const string EmbeddedCsv =
@"id;firstName;lastName;email;department;position;salary;hireDate
1;Anna;Mueller;anna.mueller@firma.de;IT;Senior Developer;72000;2018-03-15
2;Thomas;Schmidt;thomas.schmidt@firma.de;Marketing;Marketing Manager;65000;2019-07-01
3;Julia;Weber;julia.weber@firma.de;HR;HR Specialist;55000;2020-01-10
4;Markus;Fischer;markus.fischer@firma.de;Finance;Financial Analyst;62000;2017-09-20
5;Sandra;Wagner;sandra.wagner@firma.de;Sales;Sales Representative;48000;2021-04-05
6;Stefan;Becker;stefan.becker@firma.de;Engineering;Mechanical Engineer;70000;2016-11-12
7;Laura;Hoffmann;laura.hoffmann@firma.de;IT;System Administrator;60000;2019-02-28
8;Michael;Schulz;michael.schulz@firma.de;Marketing;Content Creator;45000;2022-06-15
9;Christina;Koch;christina.koch@firma.de;HR;HR Manager;68000;2015-08-03
10;Andreas;Bauer;andreas.bauer@firma.de;Finance;Senior Accountant;67000;2018-01-22
11;Sabrina;Richter;sabrina.richter@firma.de;Sales;Sales Manager;72000;2017-05-30
12;Daniel;Klein;daniel.klein@firma.de;Engineering;Software Engineer;65000;2020-03-18
13;Nicole;Wolf;nicole.wolf@firma.de;IT;Database Administrator;63000;2019-10-07
14;Patrick;Schroeder;patrick.schroeder@firma.de;Marketing;SEO Specialist;50000;2021-09-14
15;Katharina;Neumann;katharina.neumann@firma.de;HR;Recruiter;47000;2022-02-01
16;Florian;Schwarz;florian.schwarz@firma.de;Finance;Controller;75000;2016-04-25
17;Maria;Zimmermann;maria.zimmermann@firma.de;Sales;Account Manager;58000;2020-07-11
18;Jan;Braun;jan.braun@firma.de;Engineering;Project Manager;78000;2015-12-01
19;Lisa;Krueger;lisa.krueger@firma.de;IT;Frontend Developer;58000;2021-01-20
20;Robert;Hartmann;robert.hartmann@firma.de;Marketing;Brand Manager;62000;2018-08-09
21;Eva;Lange;eva.lange@firma.de;HR;Training Coordinator;52000;2019-11-15
22;Tobias;Werner;tobias.werner@firma.de;Finance;Tax Specialist;64000;2020-05-03
23;Monika;Schmitt;monika.schmitt@firma.de;Sales;Business Developer;55000;2021-08-22
24;Christian;Meier;christian.meier@firma.de;Engineering;QA Engineer;57000;2019-06-17
25;Petra;Lehmann;petra.lehmann@firma.de;IT;IT Manager;80000;2014-10-30
26;Oliver;Koenig;oliver.koenig@firma.de;Marketing;Social Media Manager;47000;2022-03-08
27;Claudia;Huber;claudia.huber@firma.de;Engineering;DevOps Engineer;71000;2018-12-05
28;Felix;Meyer;felix.meyer@firma.de;Finance;Risk Analyst;59000;2021-02-14
29;Hannah;Frank;hannah.frank@firma.de;Sales;Inside Sales;44000;2022-07-19
30;Lukas;Simon;lukas.simon@firma.de;IT;Security Analyst;66000;2020-09-01";

        public static List<Employee> Parse()
        {
            return ParseCsvText(EmbeddedCsv);
        }

        public static List<Employee> LoadFromFile(string path)
        {
            string text = File.ReadAllText(path);
            return ParseCsvText(text);
        }

        private static List<Employee> ParseCsvText(string csvText)
        {
            List<Employee> employees = new List<Employee>();
            string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Skip header line
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(';');
                if (parts.Length < 8)
                    continue;

                Employee emp = new Employee
                {
                    Id = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
                    FirstName = parts[1].Trim(),
                    LastName = parts[2].Trim(),
                    Email = parts[3].Trim(),
                    Department = parts[4].Trim(),
                    Position = parts[5].Trim(),
                    Salary = decimal.Parse(parts[6].Trim(), CultureInfo.InvariantCulture),
                    HireDate = DateTime.ParseExact(parts[7].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture)
                };

                employees.Add(emp);
            }

            return employees;
        }
    }
}
