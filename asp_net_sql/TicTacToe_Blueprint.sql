CREATE DATABASE TicTacToe_Blueprint;

USE TicTacToe_Blueprint;

---- language specific strings ----

CREATE TABLE ResxStrings(
	ID INT IDENTITY(1,1),
	[Name] NVARCHAR(64),
	[Value] NVARCHAR(2048)
);
ALTER TABLE ResxStrings ADD
CONSTRAINT PK_RxS_ID PRIMARY KEY (ID);

CREATE TABLE MenuStrings(
	ID INT IDENTITY(1,1),
	ParentID INT,
	IDX INT NOT NULL,
	[Value] NVARCHAR(2048)
);
ALTER TABLE MenuStrings ADD
CONSTRAINT PK_MnS_ID PRIMARY KEY (ID),
CONSTRAINT FK_MnS_ID__MnS_ParentID FOREIGN KEY (ParentID) REFERENCES MenuStrings(ID);

---- enums ----

CREATE TABLE EnumGameRoster(
	ID INT,
	Origin VARCHAR(64) NOT NULL,
	IDX INT NOT NULL,
	[Identity] NVARCHAR(64) NOT NULL,
);
ALTER TABLE EnumGameRoster ADD
CONSTRAINT PK_EGR_ID PRIMARY KEY (ID),
CONSTRAINT UQ_EGR_Ent UNIQUE (Origin, IDX),
CHECK (Origin IN ('None', 'Human', 'AI'));

CREATE TABLE EnumGameStates(
	ID INT,
    [Value] NVARCHAR(64) NOT NULL,
);
ALTER TABLE EnumGameStates ADD
CONSTRAINT PK_EGS_ID PRIMARY KEY (ID);

---- game state ----

CREATE TABLE Games(
	ID VARCHAR(1024), -- SHA-512 cookie
	[Name] NVARCHAR(64) NOT NULL,
	[State] INT,
	TurnWheelHead INT NOT NULL,
);
ALTER TABLE Games ADD
CONSTRAINT PK_Gms_ID PRIMARY KEY (ID),
CONSTRAINT FK_Gms_ID__EGS_Val FOREIGN KEY ([State]) REFERENCES EnumGameStates(ID);

CREATE TABLE Chosen(
	RosterId,
	Identity,
	Side,
	Origin
);


CREATE TABLE GameBoard (
	[Row] INT IDENTITY(1,1),
    Col1 INT,
    Col2 INT,
    Col3 INT,
);
ALTER TABLE GameBoard ADD
CONSTRAINT PK_GBr_ID PRIMARY KEY ([Row]),
CONSTRAINT FK_GBr_Col1__EGR_ID FOREIGN KEY (Col1) REFERENCES EnumGameRoster(ID),
CONSTRAINT FK_GBr_Col2__EGR_ID FOREIGN KEY (Col2) REFERENCES EnumGameRoster(ID),
CONSTRAINT FK_GBr_Col3__EGR_ID FOREIGN KEY (Col3) REFERENCES EnumGameRoster(ID);



"Chosen":[{"RosterId":1,"IdentityName":"Ironheart","side":1,"OriginType":"Human"},{"RosterId":4,"IdentityName":"Syncstorm","side":2,"OriginType":"AI"}]}

---- DATA ----

INSERT INTO EnumGameStates ([Value]) VALUES
(N'Countdown'),
(N'Started'),
(N'Won'),
(N'Tie');

INSERT INTO EnumGameRoster (ID, Origin, IDX, [Identity]) VALUES
(0, 'None', 1, N'None'),
(1, 'Human', 1, N'Ironheart'),
(2, 'Human', 2, N'Silverlight'),
(3, 'AI', 1, N'Quantum'),
(4, 'AI', 2, N'Syncstorm');

INSERT INTO GameBoard (Col1, Col2, Col3) VALUES
(0, 0, 0),
(0, 0, 0),
(0, 0, 0);

INSERT INTO MenuStrings (ParentID, IDX, [Value]) VALUES

(NULL, 1, N'Load'),
(NULL, 2, N'Save'),
(NULL, 3, N'Help'),
(NULL, 4, N'Game name:'),

(1, 1, N'Open saved game...'),
(1, 2, N'Saved games'),

(2, 1, N'Save game as...'),

(3, 1, N'About');

INSERT INTO ResxStrings ([Name], [Value]) VALUES
('Title', N'Tic-Tac-Toe'),
('DefaultGameName', N'Default'),
('MenuHelpAboutButton', N'Leave the narrative'),
('MenuHelpAboutContent', 
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

