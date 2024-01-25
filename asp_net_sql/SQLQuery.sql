use master;

SELECT
    DB_NAME(dbid) as 'Database name',
    COUNT(dbid) as 'Number of Connections'
FROM
    sys.sysprocesses
WHERE
    dbid > 0
GROUP BY
    dbid


CREATE DATABASE TicTacToe;
use TicTacToe;

DROP DATABASE TicTacToe;

ALTER DATABASE TicTacToe
SET SINGLE_USER
WITH ROLLBACK IMMEDIATE;

ALTER DATABASE TicTacToe
SET MULTI_USER;


EXEC sp_rename 'MenuStrings2', 'MenuStrings';

SELECT
    session_id,
    login_name,
    host_name,
    program_name,
    status
FROM
    sys.dm_exec_sessions
WHERE
    database_id = DB_ID('master');


CREATE TABLE ParentTable (
    ParentID INT NOT NULL,
	UniColumn INT NOT NULL
);

CREATE TABLE ChildTable (
    ChildID INT NOT NULL,
);

ALTER TABLE ParentTable
ADD CONSTRAINT PK_PT_ParentID 
PRIMARY KEY (ParentID)

ALTER TABLE ParentTable
ADD CONSTRAINT UQ_PT_UniColumn 
UNIQUE (UniColumn)

ALTER TABLE ChildTable
ADD CONSTRAINT PK_CT_ChildID
PRIMARY KEY (ChildID)

ALTER TABLE ChildTable
ADD CONSTRAINT FK_ChildID__PT_UniColumn 
FOREIGN KEY (ChildID) REFERENCES ParentTable(UniColumn)

use master;

use TicTacToe;

select * from ParentTable;

select * from ChildTable;

insert into ParentTable (ParentID, UniColumn) values (1, 10), (2, 11), (3, 12)

delete from ParentTable where UniColumn = 10

ALTER DATABASE TicTacToe MODIFY NAME = TicTacToe_Blueprint;