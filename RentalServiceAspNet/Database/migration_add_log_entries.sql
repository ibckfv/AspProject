CREATE TABLE IF NOT EXISTS "LogEntries" (
    "Id" SERIAL PRIMARY KEY,
    "Timestamp" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Level" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "Message" TEXT NOT NULL,
    "Exception" TEXT,
    "UserId" TEXT,
    "RequestPath" TEXT
);

CREATE INDEX IF NOT EXISTS "IX_LogEntries_Timestamp" ON "LogEntries" ("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_LogEntries_Level" ON "LogEntries" ("Level");
CREATE INDEX IF NOT EXISTS "IX_LogEntries_Category" ON "LogEntries" ("Category");
