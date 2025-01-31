using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AkademineSistema
{
    public abstract class Vartotojas
    {
        public string PrisijungimoVardas { get; private set; }
        private string Slaptazodis;
        public string Role { get; private set; }

        public Vartotojas(string prisijungimoVardas, string slaptazodis, string role)
        {
            PrisijungimoVardas = prisijungimoVardas;
            Slaptazodis = slaptazodis;
            Role = role;
        }
        public bool PatikrintiSlaptazodi(string ivestasSlaptazodis)
        {
            return Slaptazodis == ivestasSlaptazodis;
        }
        public abstract void ParodytiMeniu();
    }



    public class Studentas : Vartotojas
    {
        public int StudentoID { get; set; }
        public string Vardas { get; set; }
        public string Pavarde { get; set; }
        public string GrupesPavadinimas { get; set; }
        public Studentas(string prisijungimoVardas, string slaptazodis, string role, string vardas, string pavarde, string grupesPavadinimas)
            : base(prisijungimoVardas, slaptazodis, role)
        {
            Vardas = vardas;
            Pavarde = pavarde;
            GrupesPavadinimas = grupesPavadinimas;
        }

        public override void ParodytiMeniu()
        {
            Console.WriteLine("1. Peržiūrėti pažymius");
            Console.WriteLine("2. Grįžti atgal");
        }


        public void PerziuretiPazymius(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string findStudentQuery = @"
            SELECT StudentoID FROM Studentai 
            WHERE Vardas = @Vardas AND Pavarde = @Pavarde";

                SqlCommand findCommand = new SqlCommand(findStudentQuery, connection);
                findCommand.Parameters.AddWithValue("@Vardas", Vardas);
                findCommand.Parameters.AddWithValue("@Pavarde", Pavarde);

                connection.Open();
                object result = findCommand.ExecuteScalar();

                if (result != null)
                {
                    StudentoID = Convert.ToInt32(result);
                }

                string query = @"
            SELECT d.DalykoPavadinimas, p.Pazymys
            FROM Pazymiai p
            JOIN Dalykai d ON p.Dalykas = d.DalykoPavadinimas
            WHERE p.StudentoID = @StudentoID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentoID", StudentoID);

                SqlDataReader reader = command.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Jokiu pazymiu nerasta");
                    return;
                }

                Console.WriteLine("\nStudentas turi siuos pazymius:\n");

                while (reader.Read())
                {
                    Console.WriteLine($"Dalykas: {reader["DalykoPavadinimas"]}, Pazymys: {reader["Pazymys"]}");
                }
            }
        }

        public class Destytojas : Vartotojas
        {
            public int DestytojoID { get; set; }
            public string Vardas { get; set; }
            public string Pavarde { get; set; }
            public string DalykoPavadinimas { get; set; }
            public Destytojas(string prisijungimoVardas, string slaptazodis, string role, string vardas, string pavarde, string dalykoPavadinimas)
                : base(prisijungimoVardas, slaptazodis, role)
            {
                Vardas = vardas;
                Pavarde = pavarde;
                DalykoPavadinimas = dalykoPavadinimas;
            }
            public override void ParodytiMeniu()
            {
                Console.WriteLine("1. Įvesti pažymį");
                Console.WriteLine("2. Pašalinti pažymį");
                Console.WriteLine("3. Grįžti atgal");
            }


            public void IvestiPazymi(string connectionString, int studentoID, string dalykas, int pazymys)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Pazymiai (StudentoID, Dalykas, Pazymys) VALUES (@StudentoID, @Dalykas, @Pazymys)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@StudentoID", studentoID);
                    command.Parameters.AddWithValue("@Dalykas", dalykas);
                    command.Parameters.AddWithValue("@Pazymys", pazymys);

                    connection.Open();
                    command.ExecuteNonQuery();
                    Console.WriteLine("Pazymys sekmingai ivestas");
                }
            }


            public class Administratorius : Vartotojas
            {
                public Administratorius(string prisijungimoVardas, string slaptazodis, string role)
                    : base(prisijungimoVardas, slaptazodis, role)
                {
                }

                public override void ParodytiMeniu()
                {
                    Console.WriteLine("1. Pridėti grupę");
                    Console.WriteLine("2. Pridėti dėstytoją");
                    Console.WriteLine("3. Pašalinti studentą");
                    Console.WriteLine("4. Pašalinti dėstytoją");
                    Console.WriteLine("5. Grįžti atgal");
                }

                public void PridetiGrupe(string connectionString, string pavadinimas)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = "INSERT INTO Grupes (GrupesPavadinimas) VALUES (@GrupesPavadinimas)";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@GrupesPavadinimas", pavadinimas);


                        connection.Open();
                        command.ExecuteNonQuery();
                        Console.WriteLine("Grupe sekmingai prideta");
                    }
                }

                public void PridetiDalyka(string connectionString, string pavadinimas, string grupesPavadinimas)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = "INSERT INTO Dalykai (DalykoPavadinimas, GrupesPavadinimas) VALUES (@DalykoPavadinimas, @GrupesPavadinimas)";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@DalykoPavadinimas", pavadinimas);
                        command.Parameters.AddWithValue("@GrupesPavadinimas", grupesPavadinimas);

                        connection.Open();
                        command.ExecuteNonQuery();
                        Console.WriteLine("Dalykas sekmingai pridetas");
                    }
                }

                public void PridetiStudenta(string connectionString, string vardas, string pavarde, string grupesPavadinimas)
                {
                    string prisijungimoVardas = vardas;
                    string slaptazodis = pavarde;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = "INSERT INTO Vartotojai (PrisijungimoVardas, Slaptazodis, Role) VALUES (@PrisijungimoVardas, @Slaptazodis, @Role); SELECT SCOPE_IDENTITY();";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@PrisijungimoVardas", prisijungimoVardas);
                        command.Parameters.AddWithValue("@Slaptazodis", slaptazodis);
                        command.Parameters.AddWithValue("@Role", "Studentas");

                        connection.Open();
                        int vartotojoID = Convert.ToInt32(command.ExecuteScalar());

                        query = "INSERT INTO Studentai (VartotojoID, Vardas, Pavarde, GrupesPavadinimas) VALUES (@VartotojoID, @Vardas, @Pavarde, @GrupesPavadinimas)";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@VartotojoID", vartotojoID);
                        command.Parameters.AddWithValue("@Vardas", vardas);
                        command.Parameters.AddWithValue("@Pavarde", pavarde);
                        command.Parameters.AddWithValue("@GrupesPavadinimas", grupesPavadinimas);

                        command.ExecuteNonQuery();
                        Console.WriteLine("Studentas sekmingai pridetas");
                    }
                }


                public void PridetiDestytoja(string connectionString, string vardas, string pavarde, string dalykoPavadinimas)
                {
                    string prisijungimoVardas = vardas;
                    string slaptazodis = pavarde;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = "INSERT INTO Vartotojai (PrisijungimoVardas, Slaptazodis, Role) VALUES (@PrisijungimoVardas, @Slaptazodis, @Role); SELECT SCOPE_IDENTITY();";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@PrisijungimoVardas", prisijungimoVardas);
                        command.Parameters.AddWithValue("@Slaptazodis", slaptazodis);
                        command.Parameters.AddWithValue("@Role", "Destytojas");

                        connection.Open();
                        int vartotojoID = Convert.ToInt32(command.ExecuteScalar());

                        query = "INSERT INTO Destytojai (VartotojoID, Vardas, Pavarde, DalykoPavadinimas) VALUES (@VartotojoID, @Vardas, @Pavarde, @DalykoPavadinimas)";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@VartotojoID", vartotojoID);
                        command.Parameters.AddWithValue("@Vardas", vardas);
                        command.Parameters.AddWithValue("@Pavarde", pavarde);
                        command.Parameters.AddWithValue("@DalykoPavadinimas", dalykoPavadinimas);

                        command.ExecuteNonQuery();
                        Console.WriteLine("Destytojas sekmingai pridetas");
                    }
                }

                public void SalintiGrupe(string connectionString, string grupesPavadinimas)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Grupes WHERE GrupesPavadinimas = @GrupesPavadinimas";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@GrupesPavadinimas", grupesPavadinimas);

                    connection.Open();
                    command.ExecuteNonQuery();
                    Console.WriteLine("Grupe sekmingai pasalinta");
                }
            }

                public void SalintiDalyka(string connectionString, string dalykoPavadinimas)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Dalykai WHERE DalykoPavadinimas = @DalykoPavadinimas";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@DalykoPavadinimas", dalykoPavadinimas);

                    connection.Open();
                    command.ExecuteNonQuery();
                    Console.WriteLine("Dalykas sekmingai pasalintas");
                }
            }

                public void SalintiStudenta(string connectionString, int studentoID)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string deletePazymiaiQuery = "DELETE FROM Pazymiai WHERE StudentoID = @StudentoID";
                        SqlCommand deletePazymiaiCommand = new SqlCommand(deletePazymiaiQuery, connection);
                        deletePazymiaiCommand.Parameters.AddWithValue("@StudentoID", studentoID);
                        deletePazymiaiCommand.ExecuteNonQuery();

                        string deleteStudentQuery = "DELETE FROM Studentai WHERE StudentoID = @StudentoID";
                        SqlCommand deleteStudentCommand = new SqlCommand(deleteStudentQuery, connection);
                        deleteStudentCommand.Parameters.AddWithValue("@StudentoID", studentoID);
                        deleteStudentCommand.ExecuteNonQuery();

                        Console.WriteLine("Studentas sekmingai pasalintas");
                    }
                }

                public void SalintiDestytoja(string connectionString, int destytojoID)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Destytojai WHERE DestytojoID = @DestytojoID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@DestytojoID", destytojoID);

                    connection.Open();
                    command.ExecuteNonQuery();
                    Console.WriteLine("Destytojas sekmingai pasalintas");
                }
            }

            }
            

            public void SalintiPazymi(string connectionString, int pazymioID)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Pazymiai WHERE PazymioID = @PazymioID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PazymioID", pazymioID);

                    connection.Open();
                    command.ExecuteNonQuery();
                    Console.WriteLine("Pazymys sekmingai pasalintas");
                }
            }

            class Program
            {
                static void Main(string[] args)
                {
                    string connectionString = "Server=PC5121;Database=AcademicSystem;User ID=root;Password=root; TrustServerCertificate=True;";

                    while (true)
                    {
                        Console.WriteLine("Pasirinkite naudotojo tipa:");
                        Console.WriteLine("1. Administratorius");
                        Console.WriteLine("2. Destytojas");
                        Console.WriteLine("3. Studentas");
                        Console.WriteLine("4. Iseiti");
                        string pasirinkimas = Console.ReadLine();

                        switch (pasirinkimas)
                        {
                            case "1":
                                AdministratoriausMeniu(connectionString);
                                break;
                            case "2":
                                DestytojoMeniu(connectionString);
                                break;
                            case "3":
                                StudentoMeniu(connectionString);
                                break;
                            case "4":
                                return;
                            default:
                                Console.WriteLine("Neteisingas pasirinkimas, bandykite dar karta");
                                break;
                        }
                    }
                }

                static void AdministratoriausMeniu(string connectionString)
                {
                    Administratorius admin = new Administratorius("admin", "admin", "Administratorius");

                    while (true)
                    {
                        Console.WriteLine("Administratoriaus meniu:");
                        Console.WriteLine("1. Prideti grupe");
                        Console.WriteLine("2. Prideti dalyka");
                        Console.WriteLine("3. Prideti studenta");
                        Console.WriteLine("4. Prideti dėstytoja");
                        Console.WriteLine("5. Salinti grupe");
                        Console.WriteLine("6. Salinti dalyka");
                        Console.WriteLine("7. Salinti studenta");
                        Console.WriteLine("8. Salinti dėstytoja");
                        Console.WriteLine("9. Grizti atgal");
                        string pasirinkimas = Console.ReadLine();

                        switch (pasirinkimas)
                        {
                            case "1":
                                Console.WriteLine("Iveskite grupes pavadinima:");
                                string grupesPavadinimas = Console.ReadLine();
                                admin.PridetiGrupe(connectionString, grupesPavadinimas);
                                break;
                            case "2":
                                Console.WriteLine("Iveskite dalyko pavadinima:");
                                string dalykoPavadinimas = Console.ReadLine();
                                Console.WriteLine("Iveskite grupes pavadinima:");
                                grupesPavadinimas = Console.ReadLine();
                                admin.PridetiDalyka(connectionString, dalykoPavadinimas, grupesPavadinimas);
                                break;
                            case "3":
                                Console.WriteLine("Iveskite studento varda:");
                                string studentoVardas = Console.ReadLine();
                                Console.WriteLine("Iveskite studento pavarde:");
                                string studentoPavarde = Console.ReadLine();
                                Console.WriteLine("Iveskite grupes pavadinima:");
                                string studentoGrupesPavadinimas = Console.ReadLine();
                                admin.PridetiStudenta(connectionString, studentoVardas, studentoPavarde, studentoGrupesPavadinimas);
                                break;
                            case "4":
                                Console.WriteLine("Iveskite destytojo varda:");
                                string destytojoVardas = Console.ReadLine();
                                Console.WriteLine("Iveskite destytojo pavarde:");
                                string destytojoPavarde = Console.ReadLine();
                                Console.WriteLine("Iveskite dalyko pavadinima:");
                                string destytojoDalykoPavadinimas = Console.ReadLine();
                                admin.PridetiDestytoja(connectionString, destytojoVardas, destytojoPavarde, destytojoDalykoPavadinimas);
                                break;
                            case "5":
                                Console.WriteLine("Iveskite grupes pavadinima, kuria norite salinti:");
                                grupesPavadinimas = Console.ReadLine();
                                admin.SalintiGrupe(connectionString, grupesPavadinimas);
                                break;
                            case "6":
                                Console.WriteLine("Iveskite dalyko pavadinima, kuri norite salinti:");
                                dalykoPavadinimas = Console.ReadLine();
                                admin.SalintiDalyka(connectionString, dalykoPavadinimas);
                                break;
                            case "7":
                                Console.WriteLine("Iveskite studento ID, kuri norite salinti:");
                                int studentoID = int.Parse(Console.ReadLine());
                                admin.SalintiStudenta(connectionString, studentoID);
                                break;
                            case "8":
                                Console.WriteLine("Iveskite destytojo ID, kuri norite salinti:");
                                int destytojoID = int.Parse(Console.ReadLine());
                                admin.SalintiDestytoja(connectionString, destytojoID);
                                break;
                            case "9":
                                return;
                            default:
                                Console.WriteLine("Neteisingas pasirinkimas, bandykite dar karta");
                                break;
                        }
                    }
                }

                static void DestytojoMeniu(string connectionString)
                {
                    Console.WriteLine("Iveskite destytojo varda:");
                    string vardas = Console.ReadLine();
                    Console.WriteLine("Iveskite destytojo pavarde:");
                    string pavarde = Console.ReadLine();
                    Console.WriteLine("Iveskite dalyko pavadinima:");
                    string dalykas = Console.ReadLine();

                    Destytojas destytojas = new Destytojas(vardas, pavarde, "Destytojas", vardas, pavarde, dalykas);

                    while (true)
                    {
                        Console.WriteLine("Destytojo meniu:");
                        Console.WriteLine("1. Ivesti pazymi");
                        Console.WriteLine("2. Salinti pazymi");
                        Console.WriteLine("3. Grizti atgal");
                        string pasirinkimas = Console.ReadLine();

                        switch (pasirinkimas)
                        {
                            case "1":
                                Console.WriteLine("Iveskite studento ID:");
                                int studentoID = int.Parse(Console.ReadLine());
                                Console.WriteLine("Iveskite pazymi:");
                                int pazymys = int.Parse(Console.ReadLine());
                                destytojas.IvestiPazymi(connectionString, studentoID, dalykas, pazymys);
                                break;
                            case "2":
                                Console.WriteLine("Iveskite pazymio ID, kuri norite salinti:");
                                int pazymioID = int.Parse(Console.ReadLine());
                                destytojas.SalintiPazymi(connectionString, pazymioID);
                                break;
                            case "3":
                                return;
                            default:
                                Console.WriteLine("Neteisingas pasirinkimas, bandykite dar karta");
                                break;
                        }
                    }
                }


                static void StudentoMeniu(string connectionString)
                {
                    Console.WriteLine("Iveskite studento varda:");
                    string vardas = Console.ReadLine();
                    Console.WriteLine("Iveskite studento pavarde:");
                    string pavarde = Console.ReadLine();
                    Console.WriteLine("Iveskite grupes pavadinima:");
                    string grupesPavadinimas = Console.ReadLine();
                    Studentas studentas = new Studentas(vardas, pavarde, "Studentas", vardas, pavarde, grupesPavadinimas);


                    while (true)
                    {
                        Console.WriteLine("Studento meniu:");
                        Console.WriteLine("1. Perziureti pazymius");
                        Console.WriteLine("2. Grizti atgal");
                        string pasirinkimas = Console.ReadLine();

                        switch (pasirinkimas)
                        {
                            case "1":
                                studentas.PerziuretiPazymius(connectionString);
                                break;
                            case "2":
                                return;
                            default:
                                Console.WriteLine("Neteisingas pasirinkimas, bandykite dar karta");
                                break;
                        }
                    }
                }
            }
        }
    }
}