CREATE DATABASE TicTacToe_Blueprint;
USE TicTacToe_Blueprint;

CREATE TABLE Resx_Strings(
	Id INT PRIMARY KEY IDENTITY(1,1),
	[Name] NVARCHAR(64),
	[Value] NVARCHAR(2048)
);


---- DATA ----

INSERT INTO Resx_Strings
([Name], [Value]) VALUES
('About', 
N'About
***
Boop The Snoop For Fun And Profit is an open-source initiative aimed at finding Uncle Serge a job. We believe in the power of collaboration and community-driven development.


Open Source
***
This project is released under an open-source MIT license.
This means that the source code is freely available for inspection, modification, and distribution. You can use it, contribute to it, and even fork it to create your own version.


Disclaimer
***
While we strive to maintain the quality and security of our software, it is important to note that the software is provided "as is," without warranty of any kind. Users are encouraged to use it at their own risk. We do not make any guarantees regarding its fitness for a particular purpose, and we are not liable for any physical or mental damage or loss incurred through its use.

We welcome contributions from the community to help improve this project.
If you choose to contribute, please contact us on 
https://github.com/alikim-com/tafe 
for more information.

Thank you for being part of our open-source community!');