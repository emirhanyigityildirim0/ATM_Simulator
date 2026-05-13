\# 🏧 CultureBank - ATM Simulator



A desktop banking application that simulates real-world ATM operations. Built with \*\*C# Windows Forms\*\* and \*\*Microsoft SQL Server\*\*, this project focuses on secure database connections, transaction logging, and object-oriented programming (OOP) principles.



\## 🚀 Key Features

\* \*\*User Authentication:\*\* Secure login mechanism via database validation.

\* \*\*Account Management:\*\* Users can register and create new bank accounts.

\* \*\*Financial Operations:\*\*

&#x20; \* Cash Deposit (With specific limit constraints)

&#x20; \* Cash Withdrawal (With insufficient funds control)

&#x20; \* Peer-to-Peer Transfer (Sending funds to other registered users)

\* \*\*Transaction Logging:\*\* All financial movements are automatically recorded with timestamps (`LogTime`) using Relational Database (RDBMS) architecture.



\## 🛠️ Tech Stack

\* \*\*Language:\*\* C#

\* \*\*Framework:\*\* .NET Framework / Windows Forms (WinForms)

\* \*\*Database:\*\* Microsoft SQL Server

\* \*\*Data Access:\*\* ADO.NET (`Microsoft.Data.SqlClient`)



\## ⚙️ Installation \& Setup (Kurulum)

To run this application locally, you need to set up the database first.



##### 1\. \*\*Clone the repository:\*\*

&#x20;  ```bash

* git clone \[https://github.com/emirhanyigityildirim0/ATM\_Simulator.git](https://github.com/emirhanyigityildirim0/ATM\_Simulator.git)
  
*Original Repository (Upstream):*
* git clone [https://github.com/Anes-Zerrougui/ATM_Simulator.git](https://github.com/Anes-Zerrougui/ATM_Simulator.git)


##### 2.\*\*Database Setup:\*\*



* Open Microsoft SQL Server Management Studio (SSMS).



* Connect to your local server (.).



* Create a new blank database named CultureBankDB.



* Open the database\_setup.sql script (located in the project folder) in SSMS.



* Click Execute to automatically create the Clients and History tables.



##### 3.\*\*Run the Application:\*\*



* Open ATM\_Simulator.sln using Visual Studio.



* Press F5 to build and run the application.





## **👥Contributors**

*This project is a collaborative effort developed by:*

###### **Anes ZerrouguiLead Developer \& Core Logic** [**@Anes-Zerrougui**](https://github.com/Anes-Zerrougui)

###### **Emirhan Yiğit Yıldırım Database Architecture \& Refactoring** [**@emirhanyigityildirim0**](https://github.com/emirhanyigityildirim0)

