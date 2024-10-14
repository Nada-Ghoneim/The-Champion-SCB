DROP DATABASE ChessLeagueDB;
CREATE DATABASE ChessLeagueDB1;
GO

USE ChessLeagueDB1;

CREATE TABLE Participants (
    ID INT PRIMARY KEY IDENTITY(1,1), 
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL
);

CREATE TABLE League (
    ID INT PRIMARY KEY IDENTITY(1,1), 
    Name NVARCHAR(100),
    Champion INT NULL FOREIGN KEY REFERENCES Participants(ID),
    StartDate DATETIME,
    EndDate DATETIME
);


CREATE TABLE Matches (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Player1 INT FOREIGN KEY REFERENCES Participants(ID),
    Player2 INT FOREIGN KEY REFERENCES Participants(ID),
    Winner INT NULL FOREIGN KEY REFERENCES Participants(ID),
    MatchTime DATETIME NOT NULL ,-- Will store both the day and time of each match
	IsClosed BIT DEFAULT 0 ,
	RoundNumber INT NOT NULL,
);




CREATE TABLE Groups (
    ID INT PRIMARY KEY IDENTITY(1,1), 
    GroupNum INT NOT NULL,
    ParticipantID INT FOREIGN KEY REFERENCES Participants(ID)
);

--ALTER TABLE Matches ADD GroupNum INT Not Null FOREIGN KEY REFERENCES Groups(ID);



insert into Participants(Name,Email) values('nada1','nada1@gmail.com')
insert into Participants(Name,Email) values('nada2','nada1@gmail.com')
insert into Participants(Name,Email) values('nada3','nada1@gmail.com')
insert into Participants(Name,Email) values('nada4','nada1@gmail.com')
insert into Participants(Name,Email) values('nada5','nada1@gmail.com')
insert into Participants(Name,Email) values('nada6','nada1@gmail.com')
insert into Participants(Name,Email) values('nada7','nada1@gmail.com')
insert into Participants(Name,Email) values('nada8','nada1@gmail.com')
insert into Participants(Name,Email) values('nada9','nada1@gmail.com')
insert into Participants(Name,Email) values('nada10','nada1@gmail.com')
insert into Participants(Name,Email) values('nada11','nada1@gmail.com')
insert into Participants(Name,Email) values('nada12','nada1@gmail.com')

select * from Participants ;
select * from Groups ;
select * from Matches ;
select * from League ;

DROP TABLE IF EXISTS League;
DROP TABLE IF EXISTS  Groups;
DROP TABLE IF EXISTS Matches;
DROP TABLE IF EXISTS Participants;

delete from Matches;
delete  from League ;
delete  from Groups ;
delete  from Participants ;

update matches set Winner=26 , isClosed=1  where id=18;
update matches set Winner=30 , isClosed=1 where id=23;
update matches set Winner=31  ,isClosed=1 where id=20;
update matches set Winner=28 , isClosed=1  where id=15;
update matches set Winner=33  , isClosed=1 where id=16;
update matches set Winner=28  , isClosed=1 where id=17;



delete matches where id=105
delete matches where id=106
delete matches where id=107
delete matches where id=114
delete matches where id=115





