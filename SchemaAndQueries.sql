-- DROP TABLE Member;
-- DROP TABLE Household;

-- 1) Run Schema --
CREATE DATABSE [GRANTDB];

CREATE TABLE [GRANTDB].[dbo].[Household] (
    ObjectID UNIQUEIDENTIFIER PRIMARY KEY,
    IsDeleted INT NOT NULL,
    HousingType NVARCHAR(20) NOT NULL
);

CREATE TABLE [GRANTDB].[dbo].[Member] (
    [ObjectID] UNIQUEIDENTIFIER PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Gender] NVARCHAR(10) NOT NULL,
    [MaritalStatus] NVARCHAR(50) NOT NULL,
    [SpouseID] UNIQUEIDENTIFIER,
    [OccupationType] NVARCHAR(20) NOT NULL,
    [AnnualIncome] DECIMAL(10, 2) NOT NULL,
    [DateOfBirth] DATETIME2 NOT NULL,
    [HouseholdID] UNIQUEIDENTIFIER,
    [IsDeleted] INT NOT NULL,
    FOREIGN KEY (HouseholdID) REFERENCES Household(ObjectID) ON DELETE SET NULL
);

ALTER TABLE Member
ADD CONSTRAINT FK_SpousalRelation
FOREIGN KEY (SpouseID) REFERENCES Member(ObjectID);

-----------------------END OF SCHEMA -----------------------

-- 5) Queries 
-- i) student encoruagement bonus -- 
SELECT * FROM [Household] H 
LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
WHERE DATEDIFF(year, M.DateOfBirth, GETDATE()) < 16 AND 
M.OccupationType = 'STUDENT'
AND H.IsDeleted = 0 AND H.ObjectID IN 
            (   SELECT M1.HouseholdID FROM [Member] M1 
                WHERE M1.HouseholdID is NOT NULL 
                AND M1.IsDeleted = 0
                GROUP BY M1.HouseholdID
                HAVING SUM(M1.AnnualIncome) < 150000)

---ii)  family togetherness grant -- 
SELECT * FROM [Household] H 
LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
WHERE H.ObjectID IN (
    SELECT M1.[HouseholdID] FROM [MEMBER] M1 WHERE 
    M1.[SpouseID] IN (SELECT M2.[OBJECTID] FROM [MEMBER] M2 WHERE M2.[HouseholdID] = M1.[HouseholdID]) AND 
    M1.[HouseholdID] IN (SELECT M3.[HouseholdID] FROM [MEMBER] M3 WHERE DATEDIFF(year, M3.DateOfBirth, GETDATE()) < 18))

--- iii) elder bonus --
SELECT * FROM [Household] H 
LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
WHERE DATEDIFF(year, M.DateOfBirth, GETDATE()) > 50

-- iv) baby sunshine grant -- 
SELECT * FROM [Household] H 
LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
WHERE DATEDIFF(year, M.DateOfBirth, GETDATE()) < 5;

-- v) yolo gst grant -- 
SELECT * FROM [Household] H 
WHERE OBJECTID IN (
    SELECT M1.HouseholdID FROM [Member] M1 
    WHERE M1.IsDeleted = 0 
    GROUP BY M1.HouseholdID
    HAVING SUM(M1.AnnualIncome) < 100000
);


-- additional search query -- 
SELECT A.HouseholdID FROM [Member] A 
WHERE A.IsDeleted = 0 
GROUP BY A.HouseholdID
HAVING SUM(A.AnnualIncome) > 0 AND SUM(A.AnnualIncome) < 10000
AND COUNT(A.HouseholdID) > 1 AND COUNT(A.HouseholdID) < 1;

                    