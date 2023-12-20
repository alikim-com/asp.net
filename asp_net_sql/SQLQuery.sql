SELECT
    DB_NAME(dbid) as 'Database name',
    COUNT(dbid) as 'Number of Connections'
FROM
    sys.sysprocesses
WHERE
    dbid > 0
GROUP BY
    dbid


DROP DATABASE TicTacToe;

ALTER DATABASE TicTacToe
SET SINGLE_USER
WITH ROLLBACK IMMEDIATE;


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

use test

CREATE TABLE ParentTable (
    ParentID INT PRIMARY KEY,
	UniColumn INT UNIQUE
);

CREATE TABLE ChildTable (
    ChildID INT PRIMARY KEY,
    ParentID INT,
    FOREIGN KEY (ParentID) REFERENCES ParentTable(UniColumn)
);

ALTER TABLE ParentTable
ADD CONSTRAINT UNIQUE_ParentTable_UniColumn UNIQUE (UniColumn)

use master