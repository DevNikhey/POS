-- =====================================================================
--  schema.sql  -  Filmsammlung fuer UE4_Medien
--
--  Erzeugt die Tabelle 'films' und fuellt sie mit Beispieldaten.
--  DB erzeugen (im Projektordner UE4_Medien/UE4_Medien):
--      sqlite3 movies.db < ../schema.sql
--
--  Danach das linq2db-Modell generieren (scaffold-db) -> MoviesDB + DataModels.Film
-- =====================================================================

DROP TABLE IF EXISTS films;

CREATE TABLE films (
    id       INTEGER PRIMARY KEY AUTOINCREMENT,
    title    TEXT    NOT NULL,
    director TEXT,
    year     INTEGER,
    rating   INTEGER          -- Bewertung 0..10
);

INSERT INTO films (title, director, year, rating) VALUES
    ('Inception',          'Christopher Nolan', 2010, 9),
    ('The Matrix',         'Lana Wachowski',    1999, 9),
    ('Interstellar',       'Christopher Nolan', 2014, 8),
    ('Parasite',           'Bong Joon-ho',      2019, 10),
    ('Spirited Away',      'Hayao Miyazaki',    2001, 9),
    ('The Room',           'Tommy Wiseau',      2003, 3),
    ('Blade Runner 2049',  'Denis Villeneuve',  2017, 8);
