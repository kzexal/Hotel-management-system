﻿CREATE DATABASE HotelManagementSystem;


USE HotelManagementSystem;

GO
CREATE SCHEMA Authentication;
GO

GO
CREATE SCHEMA Hotels;
GO

GO
CREATE SCHEMA Bookings;
GO




CREATE TABLE Authentication.Login (
	Password NVARCHAR (30) NOT NULL,
	Username NVARCHAR(30) NOT NULL ,
	NewUser CHAR(5) CHECK (NewUser IN ('Yes', 'No')) DEFAULT 'Yes',
	TypeAccount INT,
	CONSTRAINT PK_Username PRIMARY KEY (Username),
);

CREATE TABLE Hotels.Guests(
	GuestId NVARCHAR(30) NOT NULL,
	GuestFirstName NVARCHAR (50) NOT NULL,
	GuestLastName NVARCHAR (50) NOT NULL,
	GuestEmailAddress NVARCHAR(50) NOT NULL ,
	GuestContactNumber NVARCHAR (15) NOT NULL,
	Street NVARCHAR(50) NOT NULL,
	City NVARCHAR(20) NOT NULL,
	Zip NVARCHAR(50) NOT NULL,
	Status NVARCHAR(20) CHECK (Status IN ('Reserved', 'Not Reserved')) NOT NULL DEFAULT 'Not Reserved',
	CONSTRAINT PK_GuestId PRIMARY KEY (GuestId),
	
);

GO
CREATE SCHEMA HotelService;
GO

CREATE TABLE HotelService.Services (
	ServiceId INT IDENTITY(1,1),
	ServiceName NVARCHAR (50) NOT NULL,
	ServiceDescription NVARCHAR (50) NOT NULL,
	ServiceCost INT NOT NULL,
	CONSTRAINT PK_ServicesId PRIMARY KEY (ServiceId),

);


CREATE TABLE Bookings.Booking(
	BookingId INT IDENTITY (1,1),
	BookingDate DATE NOT NULL,
	CheckInDate DATE NOT NULL,
	CheckOutDate DATE NOT NULL,
	BookingAmount INT NOT NULL,
	GuestId NVARCHAR(30) NOT NULL,
	Status NVARCHAR (10) NOT NULL CHECK (Status IN ('Checkin', 'Checkout')) DEFAULT 'Checkin',
	CONSTRAINT PK_BookingId PRIMARY KEY (BookingId),
	CONSTRAINT FK_GuestId_Booking FOREIGN KEY (GuestId)
	REFERENCES Hotels.Guests(GuestId) ON DELETE NO ACTION ON UPDATE NO ACTION,
);


CREATE TABLE HotelService.ServicesUsed (
	ServicesUserId INT IDENTITY (1,1),
	ServiceId INT,
	BookingId INT NOT NULL,
	CONSTRAINT FK_ServiceId_ServicesUsed FOREIGN KEY (ServiceId)
	REFERENCES HotelService.Services(ServiceId) ON DELETE CASCADE ON UPDATE NO ACTION,
	CONSTRAINT FK_BookingId_ServicesUsed FOREIGN KEY (BookingId)
	REFERENCES Bookings.Booking (BookingId) ON DELETE NO ACTION ON UPDATE NO ACTION 
);

GO
CREATE SCHEMA Rooms;
GO

CREATE TABLE Rooms.RoomType (
	RoomTypeId INT IDENTITY (1,1),
	Name NVARCHAR (50) NOT NULL,
	Cost INT NOT NULL,
	CONSTRAINT PK_RoomTypeId PRIMARY KEY (RoomTypeId),
);

CREATE TABLE Rooms.Room (
	RoomId INT IDENTITY (1,1),
	RoomNumber NVARCHAR (10) NOT NULL,
	RoomTypeId INT,
	Available NVARCHAR (5) NOT NULL CHECK (Available IN ('Yes', 'No')) DEFAULT 'Yes',
	CONSTRAINT PK_RoomId PRIMARY KEY (RoomId),
	CONSTRAINT FK_RoomTypeID_Room FOREIGN KEY (RoomTypeId)
	REFERENCES Rooms.RoomType (RoomTypeId) ON DELETE CASCADE ON UPDATE NO ACTION
);

CREATE TABLE Rooms.RoomBooked (
	RoomBookedId INT IDENTITY (1,1),
	BookingId INT NOT NULL,
	RoomId INT NOT NULL,
	CONSTRAINT PK_RoomBookedId PRIMARY KEY (RoomBookedId),
	CONSTRAINT FK_BookingId_RoomBooked FOREIGN KEY (BookingId)
	REFERENCES Bookings.Booking (BookingId) ON DELETE CASCADE ON UPDATE NO ACTION,
	CONSTRAINT FK_RoomId_RoomBooked FOREIGN KEY (RoomId)
	REFERENCES Rooms.Room (RoomId) ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE Bookings.Payments(
	PaymentId INT IDENTITY (1,1),
	PaymentStatus NVARCHAR (6) NOT NULL,
	PaymentType NVARCHAR(50) NOT NULL,
	PaymentAmount INT NOT NULL,
	BookingId INT NOT NULL,
	CONSTRAINT PK_PaymentID PRIMARY KEY (PaymentId),
	CONSTRAINT FK_BookingId_Payments FOREIGN KEY(BookingId)
	REFERENCES Bookings.Booking(BookingId) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE Hotels.StayHistory (
    StayId INT IDENTITY(1,1),
    GuestId NVARCHAR(30) NOT NULL,
    PaymentId INT,
    RoomId INT NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    CONSTRAINT PK_StayId PRIMARY KEY (StayId),
    CONSTRAINT FK_GuestId_StayHistory FOREIGN KEY (GuestId)
        REFERENCES Hotels.Guests(GuestId) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_RoomId_StayHistory FOREIGN KEY (RoomId)
        REFERENCES Rooms.Room(RoomId) ON DELETE NO ACTION ON UPDATE NO ACTION
);

-- Tạo bảng Khách hàng thân thiết
CREATE TABLE Hotels.LoyalCustomers (
    LoyalCustomerId INT IDENTITY(1,1),
    GuestId NVARCHAR(30) NOT NULL,
    MembershipLevel NVARCHAR(20) NOT NULL CHECK (MembershipLevel IN ('Silver', 'Gold', 'Platinum')),
    Points INT NOT NULL DEFAULT 0,
    JoinDate DATE NOT NULL,
    CONSTRAINT PK_LoyalCustomerId PRIMARY KEY (LoyalCustomerId),
    CONSTRAINT FK_GuestId_LoyalCustomers FOREIGN KEY (GuestId)
        REFERENCES Hotels.Guests(GuestId) ON DELETE NO ACTION ON UPDATE NO ACTION
		);
SELECT SUM(PaymentAmount) FROM Bookings.Payments WHERE BookingId IN (SELECT BookingId FROM Bookings.Booking WHERE HotelId = 3); 

SELECT PaymentId, PaymentStatus, PaymentType, PaymentAmount, BookingId FROM Bookings.Payments WHERE BookingId = (SELECT BookingId FROM Bookings.Booking WHERE HotelId = 3);

DROP TABLE Bookings.Payments;

ALTER TABLE Bookings.Payments DROP CONSTRAINT FK_BookingId_Payments;
ALTER TABLE Bookings.Payments ADD CONSTRAINT FK_BookingId_Payments FOREIGN KEY(BookingId)
	REFERENCES Bookings.Booking(BookingId) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE Bookings.Payments ADD HotelId INT NOT NULL;
ALTER TABLE Bookings.Payments ADD CONSTRAINT FK_HotelId_Payments FOREIGN KEY (HotelId)
	REFERENCES Hotels.Hotel(HotelId) ON DELETE NO ACTION ON UPDATE NO ACTION;


SELECT Bookings.Booking.BookingId, Bookings.Booking.BookingDate, Bookings.Booking.StayDuration, Bookings.Booking.CheckInDate, Bookings.Booking.CheckOutDate, Bookings.Booking.Status, Hotels.Guests.GuestId, Hotels.Guests.GuestFirstName, Hotels.Guests.GuestLastName, Hotels.Guests.GuestContactNumber, Hotels.Guests.GuestPassportNumber, Hotels.Guests.HotelId, Hotels.Employees.EmployeeId, Hotels.Employees.EmployeeFirstName, Hotels.Employees.EmployeeLastName, Hotels.Employees.EmployeeContactNumber, Bookings.Payments.PaymentId, Bookings.Payments.PaymentStatus,Bookings.Payments.PaymentAmount FROM Bookings.Booking INNER JOIN Hotels.Employees ON Bookings.Booking.EmployeeId = Hotels.Employees.EmployeeId FULL JOIN Hotels.Guests ON Bookings.Booking.GuestId = Hotels.Guests.GuestId FULL JOIN Bookings.Payments ON Bookings.Booking.BookingId = Bookings.Payments.BookingId WHERE Bookings.Booking.HotelId = 3;

-- QUERIES

-- How many distinct guest have made bookings for a particular month?
SELECT GuestFirstName, GuestLastName, GuestContactNumber
FROM Hotels.Guests
WHERE GuestId IN ( 
	SELECT DISTINCT GuestId 		
	FROM Bookings.Booking
	WHERE MONTH(CheckInDate) = 5
);

SELECT * FROM Bookings.Booking;

-- How many books has a customer made in one year?
SELECT COUNT(*) AS 'Total Bookings' 				
FROM Bookings.Booking
WHERE YEAR(BookingDate) = 2022 AND GuestId = 14;	

-- How many rooms are available for particular hotel?
SELECT COUNT(RoomId) AS 'Available Rooms'
FROM Rooms.Room
WHERE HotelId = 3 AND Available = 'Yes';

-- TRIGGER Hotels.Hotel

CREATE TABLE AuditHotels (
	AuditId INT IDENTITY (1,1) PRIMARY KEY,
	AuditData NVARCHAR (1000)
);

CREATE TRIGGER tr_HotelsInsert 
ON Hotels.Hotel
FOR INSERT
AS
BEGIN
DECLARE @Id INT 

	SELECT @Id = HotelId FROM inserted

	INSERT INTO AuditHotels VALUES ('New hotel with HotelId = ' + CAST(@Id AS NVARCHAR) + ' inserted on ' + CAST(GETDATE() AS NVARCHAR))
END

CREATE TRIGGER tr_HotelsDelete 
ON Hotels.Hotel
FOR DELETE
AS
BEGIN
DECLARE @Id INT 

	SELECT @Id = HotelId FROM deleted

	INSERT INTO AuditHotels VALUES ('Existing hotel with HotelId = ' + CAST(@Id AS NVARCHAR) + ' deleted on ' + CAST(GETDATE() AS NVARCHAR))
END

CREATE TRIGGER trgHotelForUpdate
ON Hotels.Hotel
FOR UPDATE
AS
BEGIN
	DECLARE @HotelId  INT, @AuditString NVARCHAR(1000)
	DECLARE @OldName NVARCHAR(100),@NewName NVARCHAR(100)
	DECLARE @OldContactNumber NVARCHAR (15),@NewContactNumber NVARCHAR (15)
	DECLARE @OldEmail NVARCHAR (50),@NewEmail NVARCHAR (50)
	DECLARE @OldWebsite NVARCHAR (50),@NewWebsite NVARCHAR (50)
	DECLARE @OldDescription NVARCHAR (100),@NewDescription NVARCHAR (100)
	DECLARE @OldFloorCount INT,@NewFloorCount INT
	DECLARE @OldTotalRooms INT,@NewTotalRooms INT
	DECLARE @OldAddressLine NVARCHAR(50),@NewAddressLine NVARCHAR(50)
	DECLARE @OldStreet NVARCHAR(50),@NewStreet NVARCHAR(50)
	DECLARE @OldCity NVARCHAR(20),@NewCity NVARCHAR(20)
	DECLARE @OldZip NVARCHAR(50),@NewZip NVARCHAR(50)
	DECLARE @OldCountry NVARCHAR(30),@NewCountry NVARCHAR(30)

	SELECT * INTO #TempTable FROM Inserted

	WHILE(EXISTS(SELECT HotelId FROM #TempTable))
	BEGIN
		SET @AuditString = ''

		SELECT TOP 1 @HotelId = HotelId, @NewName = Name, @NewEmail = Email, @NewWebsite = Website,
		@NewDescription = Description, @NewFloorCount= FloorCount, @NewTotalRooms = TotalRooms,@NewAddressLine = AddressLine,@NewStreet= Street,
		@NewCity = City, @NewZip= Zip, @NewCountry = Country
		FROM #TempTable

		SELECT @HotelId = HotelId, @OldName = Name, @OldEmail = Email, @OldWebsite = Website,
		@OldDescription = Description, @OldFloorCount= FloorCount, @OldTotalRooms = TotalRooms,@OldAddressLine = AddressLine,@OldStreet= Street,
		@OldCity = City, @OldZip= Zip, @OldCountry = Country
		FROM Deleted
		WHERE HotelId =@HotelId
		
		SET @AuditString = 'Hotel With Id ' + CAST(@HotelId AS NVARCHAR) + ' changed '

		IF(@NewName<>@OldName)
			SET @AuditString =  @AuditString + ' Name from ' + CAST(@OldName AS NVARCHAR) + ' to ' +
			CAST(@NewName AS NVARCHAR)
		IF(@NewEmail<>@OldEmail)
			SET @AuditString =  @AuditString + ' Email from ' + CAST(@OldEmail AS NVARCHAR) + ' to ' +
			CAST(@NewEmail AS NVARCHAR)
		IF(@NewWebsite<>@OldWebsite)
			SET @AuditString =  @AuditString + ' Website from ' + CAST(@OldWebsite AS NVARCHAR) + ' to ' +
			CAST(@NewWebsite AS NVARCHAR)
		IF(@NewDescription<>@OldDescription)
			SET @AuditString =  @AuditString + ' Description from ' + CAST(@OldDescription AS NVARCHAR) + ' to ' +
			CAST(@NewDescription AS NVARCHAR)
		IF(@NewFloorCount<>@OldFloorCount)
			SET @AuditString =  @AuditString + ' Floor count from ' + CAST(@OldFloorCount AS NVARCHAR) + ' to ' +
			CAST(@NewFloorCount AS NVARCHAR)
		IF(@NewTotalRooms<>@OldTotalRooms)
			SET @AuditString =  @AuditString + ' Total from ' + CAST(@OldTotalRooms AS NVARCHAR) + ' to ' +
			CAST(@NewTotalRooms AS NVARCHAR)
		IF(@NewAddressLine<>@OldAddressLine)
			SET @AuditString =  @AuditString + ' Address line from ' + CAST(@OldAddressLine AS NVARCHAR) + ' to ' +
			CAST(@NewAddressLine AS NVARCHAR)
		IF(@NewStreet<>@OldStreet)
			SET @AuditString =  @AuditString + ' Street from ' + CAST(@OldStreet AS NVARCHAR) + ' to ' +
			CAST(@NewStreet AS NVARCHAR)
		IF(@NewCity<>@OldCity)
			SET @AuditString =  @AuditString + ' City from ' + CAST(@OldCity AS NVARCHAR) + ' to ' +
			CAST(@NewCity AS NVARCHAR)
		IF(@NewZip<>@OldZip)
			SET @AuditString =  @AuditString + ' ZIp from ' + CAST(@OldZip AS NVARCHAR) + ' to ' +
			CAST(@NewZip AS NVARCHAR)
		IF(@NewCountry<>@OldCountry)
			SET @AuditString =  @AuditString + ' COuntry from ' + CAST(@OldCountry AS NVARCHAR) + ' to ' +
			CAST(@NewCountry AS NVARCHAR)

		INSERT INTO AuditTable Values (@AuditString)
		DELETE FROM #TempTable Where HotelId = @HotelId
    END
END

SELECT * FROM AuditHotels;

SELECT * FROM Authentication.Admin;
SELECT * FROM Authentication.Login;
SELECT * FROM Bookings.Booking;
SELECT * FROM Bookings.Discount;
SELECT * FROM Bookings.Payments;
SELECT * FROM Hotels.Departments;
SELECT * FROM Hotels.Employees;
SELECT * FROM Hotels.Guests;
SELECT * FROM Hotels.Hotel;
SELECT * FROM HotelService.Services;
SELECT * FROM HotelService.ServicesUsed;
SELECT * FROM Rooms.Room;
SELECT * FROM Rooms.RoomBooked;
SELECT * FROM Rooms.RoomType;
