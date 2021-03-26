using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Utility {
    public class EmployeeUtility {
        static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        SqlConnection databaseConnection;

        public EmployeeUtility() {
           databaseConnection = new SqlConnection(connectionString);
        }

        public int Validate(string firstName, string lastName, string mobileNo, DateTime dob, string pincode) {
            Dictionary<string, bool> validationField = new Dictionary<string, bool>() {
                { "firstName",false }, { "lastName", false }, { "mobileNo", false }, { "dob", false }, { "pincode", false }
            };
            Dictionary<string, string> errors = new Dictionary<string, string>() {
                { "firstName", "Invalid First Name, should start with capital letter" },
                { "lastName", "Invalid Last Name, should start with capital letter" },
                { "dob", "Invalid DOB,\n1) Should be in mm/dd/yyyy\n2) Your age should be more than 22 " },
                { "mobileNo", "Invalid mobile number,\n1) Should be of 10 digits starting with 91 or 0\n  or start with 044 with 6 digits in end" },
                { "pincode", "Invalid pincode, should be in numbers of 6 digit" }
            };

            string nameString = "[A-Z][A-Za-z]*";

            Regex namePattern = new Regex(nameString);

            if (namePattern.IsMatch(firstName)) validationField["firstName"] = true;
            if (namePattern.IsMatch(lastName)) validationField["lastName"] = true;
            if (DateTime.Now.Year - dob.Year > 22) validationField["dob"] = true;
            validationField["mobileNo"] = ValidateMobileNo(mobileNo);
            validationField["pincode"] = ValidatePincode(pincode);

            if (validationField["firstName"] && validationField["lastName"] && validationField["mobileNo"] && validationField["pincode"] && validationField["dob"]) return 1;
            else {
                Console.WriteLine("Following errors were faced while processing inputs: ");
                int errorCounter = 1;
                foreach (KeyValuePair<string, bool> field in validationField) {
                    if (!field.Value) {
                        Console.WriteLine(errorCounter + ". " + errors[(field.Key)]);
                        errorCounter++;
                    }
                }
                return -1;
            }
        }
        public int Validate(string email, string password) {
            string emailString = "[a-z]{2}[0-9]{2}[a-z]+[0-9]{2}[a-z]+[0-9]{4}@domainemployee.com";
            string passwordString = "(?=.*[A-z])(?=.*[A-Z])(?=.*[0-9]){6,12}";

            Regex emailPattern = new Regex(emailString);
            Regex passwordPattern = new Regex(passwordString);

            if (emailPattern.IsMatch(email) && passwordPattern.IsMatch(password)) return 1;
            else {
                int errorEntry = 1;
                if (!emailPattern.IsMatch(email)) {
                    Console.WriteLine(errorEntry + ". Invalid email please enter valid email");
                    errorEntry++;
                }
                if (!passwordPattern.IsMatch(password)) {
                    Console.WriteLine(errorEntry + ". Invalid password");
                }
                return -1;
            }
        }
        public bool ValidateMobileNo(string mobileNo) {
            string mobileString = "((91|0)[0-9]{10}|044[ -]?[0-9]{6})";
            Regex mobilePattern = new Regex(mobileString);
            if (mobilePattern.IsMatch(mobileNo.ToString())) return true;
            return false;
        }
        public bool ValidatePincode(string pincode) {
            string pincodeString = "[0-9]{6}";
            Regex pincodePattern = new Regex(pincodeString);
            if (pincodePattern.IsMatch(pincode.ToString())) return true;
            return false;
        }
        public string GenerateEmail(string firstName, string lastName, DateTime dob) {
            string firstNameLower = firstName.ToLower(), lastNameLower = lastName.ToLower();
            return firstNameLower[0].ToString() + lastNameLower[0].ToString() + dob.Day.ToString("00") + firstNameLower + dob.Month.ToString("00") + lastNameLower + dob.Year.ToString("0000") + "@domainemployee.com";
        }
        public string EnterAndValidatePassword() {
            string passwordString = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]){6,12}";
            string password = "";
            Regex passwordPattern = new Regex(passwordString);
            do {
                Console.WriteLine("Create a new password: ");
                password = Console.ReadLine();
                if (!passwordPattern.IsMatch(password)) {
                    Console.WriteLine("Invalid Password\n1)Should have atleast 1 upper case letter\n2) Should have atleast 1 lower case letter\n3) Should have numbers\n4) Should be between 6-10");
                }
            } while (!passwordPattern.IsMatch(password));
            return password;
        }
        public int StoreInEmployeeTable(string firstName, string lastName, string mobileNo, string pincode, DateTime dob, string email, string password) {
            try {
                databaseConnection.Open();

                string query = "INSERT INTO Employee values ('" + firstName + "', '" + lastName + "', '" + mobileNo + "', '" + dob.ToString("mm/dd/yyyy") + "', '" + pincode + "', '" + email + "', '" + password + "')";
                SqlCommand stringToCommand = new SqlCommand(query, databaseConnection);
                int numberOfRowAffrected = stringToCommand.ExecuteNonQuery();
                databaseConnection.Close();
                return numberOfRowAffrected;

            }
            catch(Exception exception) {
                Console.WriteLine(exception.Message);
            }
            return 0;
        }
        public Dictionary<string, string> ReadFromEmployeeTable() {
            Dictionary<string, string> nameMail = new Dictionary<string, string>();
            try {
                databaseConnection.Open();

                string query = "SELECT CONCAT(firstName, ' ',  lastName) AS Name, email FROM Employee";
                SqlDataAdapter adapter = new SqlDataAdapter(query, databaseConnection);
                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows) {
                    nameMail.Add(row[1].ToString(), row[0].ToString());
                }
            }
            catch(Exception exception) {
                Console.WriteLine(exception.Message);
            }
            return nameMail;
        }
        public int Authenticate(string email, string password, out string name) {
            try {
                databaseConnection.Open();

                string selectQuery = "SELECT * FROM Employee WHERE email = '" + email + "' AND password = '" + password + "'";
                SqlCommand selectStringToCommand = new SqlCommand(selectQuery, databaseConnection);
                SqlDataReader rowReader = selectStringToCommand.ExecuteReader();
                rowReader.Read();
                if (email == rowReader[5].ToString() && password == rowReader[6].ToString()) {
                    name = rowReader[0].ToString() + " " + rowReader[1].ToString();
                    return 1;
                } else {
                    Console.WriteLine("Incorrect email or password");
                    name = "";
                    return -1;
                }
            }
            catch(Exception exception) {
                Console.WriteLine(exception.Message);
            }
            name = "";
            return -1;
        }
        public int UpdateMobileNumber(string mobileNo, string email, string password) {
            try {
                databaseConnection.Open();
                string query = "UPDATE Employee SET mobileNo = '" + mobileNo + "' WHERE email = '" + email + "' AND password = '" + password + "' ";

                SqlCommand stringToCommand = new SqlCommand(query, databaseConnection);
                int numberOfRowsAffected = stringToCommand.ExecuteNonQuery();
                databaseConnection.Close();

                if (numberOfRowsAffected > 0) return 1;
                return -1;
            }
            catch(Exception exception) {
                Console.WriteLine(exception.Message);
            }
            return -1;
        }
        public int UpdatePincode(string pincode, string email, string password) {
            try {
                databaseConnection.Open();
                string query = "UPDATE Employee SET pincode = '" + pincode + "' WHERE email = '" + email + "' AND password = '" + password + "' ";

                SqlCommand stringToCommand = new SqlCommand(query, databaseConnection);
                int numberOfRowsAffected = stringToCommand.ExecuteNonQuery();
                databaseConnection.Close();

                if (numberOfRowsAffected > 0) return 1;
                return -1;
            }
            catch(Exception exception) {
                Console.WriteLine(exception.Message);
            }
            return -1;
        }
        public int UpdatePassword(string pass, string email, string password) {
            try {
                databaseConnection.Open();
                string query = "UPDATE Employee SET password = '" + pass + "' WHERE email = '" + email + "' AND password = '" + password + "' ";

                SqlCommand stringToCommand = new SqlCommand(query, databaseConnection);
                int numberOfRowsAffected = stringToCommand.ExecuteNonQuery();
                databaseConnection.Close();

                if (numberOfRowsAffected > 0) return 1;
                return -1;
            }
            catch(Exception exception) {
                Console.WriteLine(exception.Message);
            }
            return -1;
            
        }
        public int DeleteFromEmployeeTable(string email, string password) {
            try {
                databaseConnection.Open();

                string query = "DELETE FROM Employee WHERE email = '" + email + "' AND password = '" + password + "'";
                SqlCommand stringToCommand = new SqlCommand(query, databaseConnection);

                int numberOfRowAffected = stringToCommand.ExecuteNonQuery();

                databaseConnection.Close();
                if (numberOfRowAffected > 0) return 1;
                return -1;
            }
            catch(Exception exception) {
                Console.WriteLine(exception.Message);
            }
            return -1;
        }
    }
}