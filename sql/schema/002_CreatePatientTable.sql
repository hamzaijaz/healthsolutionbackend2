CREATE TABLE [Patient]
(
    [PatientId] int PRIMARY KEY IDENTITY(1, 1),
    [PatientKey] uniqueidentifier NOT NULL,
    [FirstName] varchar(100) NOT NULL,
    [LastName] varchar(100) NOT NULL,
    [DateOfBirth] varchar(15) NOT NULL,
    [Gender] varchar(10) NOT NULL,
    [StreetAddress] varchar(100) NOT NULL,
    [Suburb] varchar(100) NOT NULL,
    [Postcode] varchar(10) NOT NULL,
    [HealthCoverType] varchar(100) NOT NULL,
    [PolicyNumber] varchar(20) NOT NULL,
    [LastUpdatedAtUtc] datetime NOT NULL
)
GO

CREATE UNIQUE INDEX [Patient_AK1] ON [Patient] ("PatientKey")
GO
