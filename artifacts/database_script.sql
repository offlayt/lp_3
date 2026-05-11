CREATE TABLE "MaterialTypes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaterialTypes" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "LossPercent" REAL NOT NULL
);


CREATE TABLE "ProductTypes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductTypes" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Coefficient" REAL NOT NULL
);


CREATE TABLE "Suppliers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Suppliers" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "SupplierType" TEXT NOT NULL,
    "Inn" TEXT NOT NULL,
    "Rating" INTEGER NOT NULL,
    "StartDate" TEXT NOT NULL
);


CREATE TABLE "Materials" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Materials" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "MaterialTypeId" INTEGER NOT NULL,
    "UnitPrice" TEXT NOT NULL,
    "StockQuantity" TEXT NOT NULL,
    "MinQuantity" TEXT NOT NULL,
    "PackQuantity" TEXT NOT NULL,
    "Unit" TEXT NOT NULL,
    CONSTRAINT "FK_Materials_MaterialTypes_MaterialTypeId" FOREIGN KEY ("MaterialTypeId") REFERENCES "MaterialTypes" ("Id") ON DELETE CASCADE
);


CREATE TABLE "MaterialSuppliers" (
    "MaterialId" INTEGER NOT NULL,
    "SupplierId" INTEGER NOT NULL,
    CONSTRAINT "PK_MaterialSuppliers" PRIMARY KEY ("MaterialId", "SupplierId"),
    CONSTRAINT "FK_MaterialSuppliers_Materials_MaterialId" FOREIGN KEY ("MaterialId") REFERENCES "Materials" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MaterialSuppliers_Suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES "Suppliers" ("Id") ON DELETE CASCADE
);


CREATE INDEX "IX_Materials_MaterialTypeId" ON "Materials" ("MaterialTypeId");


CREATE INDEX "IX_MaterialSuppliers_SupplierId" ON "MaterialSuppliers" ("SupplierId");


