-- 1. Veritabanını kullanmaya başla
USE CultureBankDB;
GO

-- 2. Clients (Müşteriler) Tablosu
-- Müşterilerin hesap no, şifre ve bakiye bilgilerini tutar.

CREATE TABLE Clients (
    ClientNo NVARCHAR(50) PRIMARY KEY, -- Hesap numarası benzersiz olmalı (Birincil Anahtar)
    Password NVARCHAR(50) NOT NULL,    -- Kullanıcı şifresi (Boş bırakılamaz)
    Funds DECIMAL(18, 2) DEFAULT 0     -- Bakiye (Kuruşlu işlemler için Decimal 18,2 standardı)
);
GO

-- 3. History (İşlem Geçmişi) Tablosu
-- Kim, ne zaman, ne kadarlık işlem yaptı?
CREATE TABLE History (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY, -- Her işleme otomatik artan benzersiz ID (1, 2, 3...)
    ClientNo NVARCHAR(50) NOT NULL,          -- İşlemi kim yaptı?
    Operation NVARCHAR(100) NOT NULL,        -- İşlem tipi (Deposit, Transfer vb.)
    Amount DECIMAL(18, 2) NOT NULL,          -- İşlem tutarı
    LogTime DATETIME DEFAULT GETDATE(),      -- İşlem zamanı

    --  (Foreign Key):
    FOREIGN KEY (ClientNo) REFERENCES Clients(ClientNo)
);
GO