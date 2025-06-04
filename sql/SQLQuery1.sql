CREATE DATABASE HotelManagementSystem;
GO
USE HotelManagementSystem;
GO
CREATE SCHEMA Authentication;
GO
CREATE SCHEMA Hotel;
GO
CREATE SCHEMA Bookings;
GO

CREATE TABLE Authentication.Login (
	LoginID INT IDENTITY(1,1),
	Password NVARCHAR (30) NOT NULL,
	Username NVARCHAR(30) NOT NULL ,
	NewUser CHAR(5) CHECK (NewUser IN ('Yes', 'No')) DEFAULT 'Yes',
	TypeAccount INT,
	CONSTRAINT PK_Username PRIMARY KEY (LoginID),
);

CREATE TABLE Hotel.Guests(
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
INSERT INTO HotelService.Services (ServiceName, ServiceDescription, ServiceCost)
VALUES 
('Laundry', 'Washing and ironing clothes', 5),           
('Airport Pickup', 'Pickup service from airport', 20),   
('Spa', 'Relaxing body massage', 30),                    
('Breakfast', 'Buffet breakfast', 10),                   
('Room Cleaning', 'Daily room cleaning service', 3);  
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
	REFERENCES Hotel.Guests(GuestId) ON DELETE NO ACTION ON UPDATE NO ACTION,
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
INSERT INTO Rooms.RoomType ( Name, Cost) 
VALUES
('Deluxe Suite',120),
('Standard Room',75),
('Family Room',150),
('Single Room',50);

CREATE TABLE Rooms.Room (
	RoomId INT IDENTITY (1,1),
	RoomNumber NVARCHAR (10) NOT NULL,
	RoomTypeId INT,
	Available NVARCHAR (5) NOT NULL CHECK (Available IN ('Yes', 'No')) DEFAULT 'Yes',
	Image VARCHAR(255)  NULL,
	CONSTRAINT PK_RoomId PRIMARY KEY (RoomId),
	CONSTRAINT FK_RoomTypeID_Room FOREIGN KEY (RoomTypeId)
	REFERENCES Rooms.RoomType (RoomTypeId) ON DELETE CASCADE ON UPDATE NO ACTION
);
INSERT INTO Rooms.Room (RoomNumber, RoomTypeId, Available, Image)
VALUES 
('A101', 1, 'Yes', 'https://tse3.mm.bing.net/th/id/OIP.f653efCDL592sv5Z0y75EwHaE7?cb=iwp2&rs=1&pid=ImgDetMain'),
('B202', 2, 'No', 'https://tse2.mm.bing.net/th/id/OIP.GqL7sKHuM8iiVWX3Xqct7QHaEh?cb=iwp2&rs=1&pid=ImgDetMain'),
('C303', 3, 'Yes', 'https://i2.wp.com/thecarriedeer.com/wp-content/uploads/2022/02/living-room-and-family-room-ideas-scaled.jpg'),
('D404', 4, 'Yes', 'https://tse4.mm.bing.net/th/id/OIP.7irKaSESaO2vdOB13R_1zAHaE8?cb=iwp2&rs=1&pid=ImgDetMain'),
('B201', 1, 'Yes', 'https://tse3.mm.bing.net/th/id/OIP.f653efCDL592sv5Z0y75EwHaE7?cb=iwp2&rs=1&pid=ImgDetMain'),
('C301', 1, 'Yes', 'https://tse3.mm.bing.net/th/id/OIP.f653efCDL592sv5Z0y75EwHaE7?cb=iwp2&rs=1&pid=ImgDetMain'),
('D401', 1, 'Yes', 'https://tse3.mm.bing.net/th/id/OIP.f653efCDL592sv5Z0y75EwHaE7?cb=iwp2&rs=1&pid=ImgDetMain');

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
      
DROP TABLE Bookings.Payments;

ALTER TABLE Bookings.Payments DROP CONSTRAINT FK_BookingId_Payments;
ALTER TABLE Bookings.Payments ADD CONSTRAINT FK_BookingId_Payments FOREIGN KEY(BookingId)
	REFERENCES Bookings.Booking(BookingId) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE Bookings.Payments ADD HotelId INT NOT NULL;
ALTER TABLE Bookings.Payments ADD CONSTRAINT FK_HotelId_Payments FOREIGN KEY (HotelId)
	REFERENCES Hotels.Hotel(HotelId) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE Hotels.Guests
ADD UserId INT;
ALTER TABLE Hotels.Guests
ADD CONSTRAINT FK_Guests_Login
FOREIGN KEY (UserId) REFERENCES Authentication.Login(LoginId);











