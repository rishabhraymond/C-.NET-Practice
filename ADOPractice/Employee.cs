using System;
using System.Collections.Generic;
using Utility;

namespace ADOPractice {
    class Employee {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Dob { get; set; }
        public int MobileNo { get; set; }
        public int Pincode { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        static EmployeeUtility  employeeUtility = new EmployeeUtility();

        public Employee() {
            
        }

        public int AddEmployee(string firstName, string lastName, DateTime dob, string mobileNo, string pincode) {
            if (employeeUtility.Validate(firstName, lastName, mobileNo, dob, pincode) == -1) return -1;
            string email = employeeUtility.GenerateEmail(firstName, lastName, dob);
            string password = employeeUtility.EnterAndValidatePassword();
            if (employeeUtility.StoreInEmployeeTable(firstName, lastName, mobileNo, pincode, dob, email, password) > 0) return 1;
            return -1;
        }
        public void ViewEmployee() {
            Console.Clear();
            Dictionary<string, string> EmployeeNameMail = employeeUtility.ReadFromEmployeeTable();
            int entryNo = 1;
            foreach (KeyValuePair<string, string> nameMail in EmployeeNameMail) {
                Console.WriteLine("Entry No: " + entryNo);
                Console.WriteLine("Name: " + nameMail.Value);
                Console.WriteLine("Email: " + nameMail.Key);
                Console.WriteLine("------------------------------------------------------------------------------");
                entryNo++;
            }
        }
        public int UpdateEmployee(string email, string password) {
            string name;
            if (employeeUtility.Validate(email, password) == -1) return -1;
            if (employeeUtility.Authenticate(email, password, out name) == -1) return -1;
            int option;
            Console.WriteLine("Welcome " + name + "what do you want to update?");
            Console.WriteLine("1. Mobile Number");
            Console.WriteLine("2. Pincode");
            Console.WriteLine("3. Password");
            Console.WriteLine("4. Back");
            option = Int32.Parse(Console.ReadLine());
            switch (option) {
                case 1:
                    Console.WriteLine("Enter new mobile number");
                    string mobileNo = Console.ReadLine();
                    if (employeeUtility.ValidateMobileNo(mobileNo)) {
                        if(employeeUtility.UpdateMobileNumber(mobileNo, email, password) == 1)  Console.WriteLine("Mobile number successfully updated");
                    } else Console.WriteLine("Invalid mobile number");
                    break;

                case 2:
                    Console.WriteLine("Enter new pincode");
                    string pincode = Console.ReadLine();
                    if (employeeUtility.ValidatePincode(pincode)) {
                        if (employeeUtility.UpdatePincode(pincode, email, password) == 1) Console.WriteLine("Pincode successfully updated");
                    } else Console.WriteLine("Invalid pincode");
                    break;

                case 3:
                    string pass = employeeUtility.EnterAndValidatePassword();
                    if (employeeUtility.UpdatePassword(pass, email, password) == 1) Console.WriteLine("Password successfully updated");
                    break;

                case 4:
                    return 1;

            }
            return 1;
        }
        public int RemoveEmployee(string email, string password) {
            string name;
            employeeUtility.Authenticate(email, password, out name);
            Console.WriteLine("Hi, " + name + " if you want to remove your details then please enter remove then write your full name");
            string command = Console.ReadLine();
            if (command == "remove " + name) {
                if (employeeUtility.DeleteFromEmployeeTable(email, password) == 1) return 1;
            }
            return -1;
        }
    }
}
