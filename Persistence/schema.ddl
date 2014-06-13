CREATE TABLE emails(
   id INTEGER PRIMARY KEY NOT NULL,
   date TEXT,
   timestamp INTEGER, 
   sender TEXT,
   intro TEXT
);

CREATE TABLE calendar_entries(
   id INTEGER PRIMARY KEY NOT NULL,
   date TEXT,
   timestamp INTEGER, 
   title TEXT,
   location TEXT,
   description TEXT,
   rsvp TEXT,
   email INTEGER NOT NULL REFERENCES emails (id)
);
